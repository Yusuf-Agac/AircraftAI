using System;
using System.Collections;
using System.Linq;
using DefaultNamespace;
using Oyedoyin.Common;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AircraftFlightAgent : Agent
{
    public bool trainingMode;
    [Range(0.1f, 25f), Space(10)] public float manoeuvreSpeed = 10f;
    public float maxWindSpeed = 5;
    public float maxTurbulence = 5;
    public int numOfOptimumDirections = 2;
    public float gapBetweenOptimumDirections = 25f;
    [Space(10)]
    public FlightPathNormalizer flightPathNormalizer;
    public FixedController aircraftController;
    [Space(10)]
    public Slider pitchSlider;
    public Slider rollSlider;
    public Slider throttleSlider;
    
    private float[] _previousActions = new float[3] {0, 0, 0};
    private float _sparseRewards;
    private float _denseRewards;
    private float _optimalRewards;
    private float _actionPenalty;
    
    private bool _episodeStarted;
    
    private void Start () 
    {
        aircraftController = GetComponent<FixedController>();
    }
    
    public override void OnEpisodeBegin()
    {
        if (trainingMode)
        {
            _episodeStarted = false;
            StartCoroutine(AfterBegin());
        }
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        AtmosphereController.SmoothlyChangeWindAndTurbulence(aircraftController, maxWindSpeed, maxTurbulence);
        var windData = AircraftNormalizer.NormalizedWind(aircraftController, maxWindSpeed, maxTurbulence);
        
        // AIRCRAFT VELOCITY
        sensor.AddObservation(AircraftNormalizer.NormalizedSpeed(aircraftController));
        sensor.AddObservation(aircraftController.m_rigidbody.velocity.normalized);
        
        // AIRCRAFT GLOBAL ROTATION
        sensor.AddObservation(transform.forward);
        sensor.AddObservation(Vector3.Dot(transform.forward, Vector3.up));
        sensor.AddObservation(Vector3.Dot(transform.up, Vector3.down));
        
        // OPTIMUM POINT
        sensor.AddObservation(flightPathNormalizer.NormalizedClosestOptimumPositionDistance(transform.position));
        var optimumDirections = flightPathNormalizer.NormalizedClosestOptimumPointDirections(transform, numOfOptimumDirections, gapBetweenOptimumDirections);
        foreach (var optimumDirection in optimumDirections)
        {
            sensor.AddObservation(optimumDirection);
        }
        
        // AIRCRAFT INPUTS
        sensor.AddObservation(_previousActions);
        
        // ATMOSPHERE
        sensor.AddObservation(windData);
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        aircraftController.m_input.SetAgentInputs(actionBuffers, manoeuvreSpeed);
        
        var illegalAircraftRotation = Vector3.Dot(transform.forward, Vector3.up) > 0.4f || Vector3.Dot(transform.forward, Vector3.up) < -0.4f || Vector3.Dot(transform.up, Vector3.down) > 0;
        var distanceToRoute = flightPathNormalizer.NormalizedClosestOptimumPositionDistance(transform.position);
        var distanceToTarget = flightPathNormalizer.TargetDistance(transform.position);
        
        if ((distanceToRoute > 1f || illegalAircraftRotation) && trainingMode && _episodeStarted)
        {
            _episodeStarted = false;
            SetReward(-1);
            _sparseRewards--;
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Debug.Log("Sparse: " + _sparseRewards + " / Dense: " + _denseRewards + " / Optimal: " + _optimalRewards + " / Action: " + _actionPenalty + " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
            EndEpisode();
        }
        else if (distanceToTarget < 30f)
        {
            _episodeStarted = false;
            SetReward(20);
            _sparseRewards++;
            Debug.Log("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Debug.Log("Time: " + DateTime.UtcNow.ToString("HH:mm"));
            Debug.Log("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            EndEpisode();
        }
        else
        {
            AddReward(Mathf.Clamp01(1 - distanceToRoute) * 0.008f);
            _denseRewards += Mathf.Clamp01(1 - distanceToRoute) * 0.008f;
            _optimalRewards += Mathf.Clamp01(1 - distanceToRoute) * 0.008f;
            AddReward(-Mathf.Clamp01(distanceToRoute) * 0.004f);
            _denseRewards -= Mathf.Clamp01(distanceToRoute) * 0.004f;
            _optimalRewards -= Mathf.Clamp01(distanceToRoute) * 0.004f;

            for (var i = 0; i < _previousActions.Length; i++)
            {
                var actionDifference = Mathf.Abs(_previousActions[i] - actionBuffers.ContinuousActions[i]);
                AddReward(-0.004f * actionDifference);
                _denseRewards -= 0.004f * actionDifference;
                _actionPenalty -= 0.004f * actionDifference;
            }
        }
        _previousActions = actionBuffers.ContinuousActions.ToArray();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = pitchSlider.value;
        continuousActionsOut[1] = rollSlider.value;
        continuousActionsOut[2] = throttleSlider.value;
        aircraftController.m_input.SetAgentInputs(actionsOut, manoeuvreSpeed);
    }
    
    IEnumerator AfterBegin()
    {
        aircraftController.m_rigidbody.isKinematic = true;
        yield return null;
        aircraftController.TurnOnEngines();
        yield return new WaitForSeconds(0.1f);
        flightPathNormalizer.ResetAirportTransform();
        flightPathNormalizer.ResetAircraftPosition(transform);
        yield return new WaitForSeconds(0.1f);
        aircraftController.m_rigidbody.isKinematic = false;
        aircraftController.PositionAircraft();
        aircraftController.m_core.Compute(Time.fixedDeltaTime);
        //RAISE GEAR
        if (aircraftController.gearActuator != null && aircraftController.gearActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) { aircraftController.gearActuator.DisengageActuator(); }
        else { aircraftController.m_gearState = Controller.GearState.Up; }
        yield return new WaitForSeconds(0.5f);
        _episodeStarted = true;
    }
}
