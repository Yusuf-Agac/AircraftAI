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
using UnityEngine.UI;

public class AircraftFlightAgent : Agent
{
    [Space(10)]
    [SerializeField] private bool _trainingMode = true;
    [Space(10)]
    [Range(0.1f, 25f)] public float manoeuvreSpeed = 10f;
    public float maxWindSpeed = 5;
    public float maxTurbulence = 5;
    public int numOfOptimumDirections = 2;
    public float gapBetweenOptimumDirections = 25f;
    [Space(10)]
    public FlightPathNormalizer flightPathNormalizer;
    public FixedController aircraftController;
    [Space(10)]
    public Slider PitchSlider;
    public Slider RollSlider;
    public Slider ThrottleSlider;
    
    private float[] _previousActions = new float[3] {0, 0, 0};
    private float _sparseRewards;
    private float _denseRewards;
    private float _optimalRewards;
    private float _actionPenalty;
    
    private void Start () 
    {
        aircraftController = GetComponent<FixedController>();
    }
    
    public override void OnEpisodeBegin()
    {
        aircraftController.RestoreAircraft();
        flightPathNormalizer.ResetAircraftPosition(transform);
        StartCoroutine(AfterBegin());
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
        
        if ((distanceToRoute > 1f || illegalAircraftRotation) && _trainingMode)
        {
            SetReward(-1);
            _sparseRewards--;
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Debug.Log("Sparse: " + _sparseRewards + " / Dense: " + _denseRewards + " / Optimal: " + _optimalRewards + " / Action: " + _actionPenalty + " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
            EndEpisode();
        }
        else if (distanceToTarget < 10f)
        {
            SetReward(20);
            _sparseRewards++;
            Debug.Log("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            Debug.Log("Time: " + DateTime.UtcNow.ToString("HH:mm"));
            Debug.Log("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            EndEpisode();
        }
        else
        {
            AddReward(Mathf.Clamp01(1 - distanceToRoute) * 0.004f);
            _denseRewards += Mathf.Clamp01(1 - distanceToRoute) * 0.004f;
            _optimalRewards += Mathf.Clamp01(1 - distanceToRoute) * 0.004f;
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
        continuousActionsOut[0] = PitchSlider.value;
        continuousActionsOut[1] = RollSlider.value;
        continuousActionsOut[2] = ThrottleSlider.value;
        aircraftController.m_input.SetAgentInputs(actionsOut, manoeuvreSpeed);
    }
    
    IEnumerator AfterBegin()
    {
        yield return null;
        aircraftController.TurnOnEngines();
        yield return null;
        flightPathNormalizer.ResetAircraftPosition(transform);
        aircraftController.PositionAircraft();
        //RAISE GEAR
        if (aircraftController.gearActuator != null && aircraftController.gearActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) { aircraftController.gearActuator.DisengageActuator(); }
        else { aircraftController.m_gearState = Controller.GearState.Up; }
    }
}
