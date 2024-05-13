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
    [Range(0.1f, 25f), Space(10)] 
    public float manoeuvreSpeed = 10f;
    public float maxWindSpeed = 5;
    public float maxTurbulence = 5;
    public int numOfOptimalDirections = 2;
    public float gapBetweenOptimalDirections = 25f;
    [Space(10)]
    public ObservationCanvas observationCanvas;
    public FlightPathNormalizer flightPathNormalizer;
    public FixedController aircraftController;
    [Space(10)]
    public Slider pitchSlider;
    public Slider rollSlider;
    public Slider throttleSlider;
    
    private DecisionRequester _decisionRequester;
    
    private float[] _previousActions = new float[3] {0, 0, 0};
    private float _sparseRewards;
    private float _denseRewards;
    private float _optimalRewards;
    private float _actionPenalty;
    private Vector3 _aircraftForward;
    private Vector3 _aircraftUp;
    private float _dotForwardUp;
    private float _dotUpDown;
    private float _normalizedSpeed;
    private Vector3 _relativeVelocityDir;
    private float _normalizedOptimalDistance;
    private Vector3[] _optimalDirections;
    private float _dotVelRot;
    private float _dotVelOpt;
    private float _dotRotOpt;
    private float _normalizedPitchRate;
    private float _normalizedRollRate;
    private float _normalizedYawRate;
    private float[] _windData;
    private float _windAngle;
    private float _windSpeed;
    private float _turbulence;
    private bool _episodeStarted;
    
    private void Start () 
    {
        aircraftController = GetComponent<FixedController>();
        _decisionRequester = GetComponent<DecisionRequester>();
    }
    
    public override void OnEpisodeBegin()
    {
        if (!trainingMode) return;
        _episodeStarted = false;
        StartCoroutine(AfterBegin());
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        AtmosphereController.SmoothlyChangeWindAndTurbulence(aircraftController, maxWindSpeed, maxTurbulence, _decisionRequester.DecisionPeriod);
        
        _aircraftForward = transform.forward;
        _aircraftUp = transform.up;
        _dotForwardUp = Vector3.Dot(_aircraftForward, Vector3.up);
        _dotUpDown = Vector3.Dot(_aircraftUp, Vector3.down);
        
        _normalizedSpeed = AircraftNormalizer.NormalizedSpeed(aircraftController);
        _relativeVelocityDir = aircraftController.m_rigidbody.velocity.normalized;

        _normalizedOptimalDistance = flightPathNormalizer.NormalizedClosestOptimumPositionDistance(transform.position);
        _optimalDirections = flightPathNormalizer.NormalizedClosestOptimumPointDirections(transform, numOfOptimalDirections, gapBetweenOptimalDirections);
        
        _dotVelRot = Vector3.Dot(_relativeVelocityDir, _aircraftForward);
        _dotVelOpt = Vector3.Dot(_relativeVelocityDir, _optimalDirections[0]);
        _dotRotOpt = Vector3.Dot(_aircraftForward, _optimalDirections[0]);
        
        _normalizedPitchRate = NormalizerUtility.ClampNP1((float)(aircraftController.m_core.q * Mathf.Rad2Deg / 45f));
        _normalizedRollRate = NormalizerUtility.ClampNP1((float)(aircraftController.m_core.p * Mathf.Rad2Deg / 45f));
        _normalizedYawRate = NormalizerUtility.ClampNP1((float)(aircraftController.m_core.r * Mathf.Rad2Deg / 45f));
        
        _windData = AircraftNormalizer.NormalizedWind(aircraftController, maxWindSpeed, maxTurbulence);
        _windAngle = _windData[0] * 360;
        _windSpeed = _windData[1] * maxWindSpeed;
        _turbulence = _windData[2] * maxTurbulence;
        
        // AIRCRAFT GLOBAL ROTATION
        sensor.AddObservation(_aircraftForward);
        sensor.AddObservation(_dotForwardUp);
        sensor.AddObservation(_dotUpDown);
        
        // AIRCRAFT VELOCITY
        sensor.AddObservation(_normalizedSpeed);
        sensor.AddObservation(_relativeVelocityDir);
        
        // OPTIMUM POINT
        sensor.AddObservation(_normalizedOptimalDistance);
        foreach (var optimalDirection in _optimalDirections) sensor.AddObservation(optimalDirection);
        
        // Relative Directions
        sensor.AddObservation(_dotVelRot);
        sensor.AddObservation(_dotVelOpt);
        sensor.AddObservation(_dotRotOpt);
        
        // AIRCRAFT INPUTS
        sensor.AddObservation(_previousActions);
        
        // AIRCRAFT AXES RATES
        sensor.AddObservation(_normalizedPitchRate);
        sensor.AddObservation(_normalizedRollRate);
        sensor.AddObservation(_normalizedYawRate);
        
        // ATMOSPHERE
        sensor.AddObservation(_windData);
        
        observationCanvas.DisplayNormalizedData(
            _aircraftForward, _dotForwardUp, _dotUpDown,
            _relativeVelocityDir, _normalizedSpeed,
            _normalizedOptimalDistance, _optimalDirections,
            _dotVelRot, _dotVelOpt, _dotRotOpt,
            _previousActions,
            _normalizedPitchRate, _normalizedRollRate, _normalizedYawRate,
            (float)aircraftController.m_flcs.m_pitch, (float)aircraftController.m_flcs.m_roll, (float)aircraftController.m_flcs.m_yaw,
            _windAngle, _windSpeed, _turbulence
        );
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        aircraftController.m_input.SetAgentInputs(actionBuffers, manoeuvreSpeed);
        
        _aircraftForward = transform.forward;
        _aircraftUp = transform.up;
        _dotForwardUp = Vector3.Dot(_aircraftForward, Vector3.up);
        _dotUpDown = Vector3.Dot(_aircraftUp, Vector3.down);
        var illegalAircraftRotation = _dotForwardUp is > 0.4f or < -0.4f || _dotUpDown > -0.3f;

        var position = transform.position;
        var distanceToRoute = flightPathNormalizer.NormalizedClosestOptimumPositionDistance(position);
        var distanceToTarget = flightPathNormalizer.TargetDistance(position);
        
        if (IsEpisodeFailed(distanceToRoute, illegalAircraftRotation))
        {
            _episodeStarted = false;
            SetReward(-1);
            _sparseRewards--;
            Debug.Log("Sparse: " + _sparseRewards + " / Dense: " + _denseRewards + " / Optimal: " + _optimalRewards + " / Action: " + _actionPenalty + " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
            EndEpisode();
        }
        else if (AircraftArrivedExit(distanceToTarget))
        {
            _episodeStarted = false;
            SetReward(20);
            _sparseRewards++;
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Debug.Log("SUCCESSFUL / " + "Sparse: " + _sparseRewards + " / Dense: " + _denseRewards + " / Optimal: " + _optimalRewards + " / Action: " + _actionPenalty + " /// Time: " + DateTime.UtcNow.ToString("HH:mm"));
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            EndEpisode();
        }
        else
        {
            var reward = Mathf.Clamp01(1 - distanceToRoute) * 0.008f;
            AddReward(reward);
            _denseRewards += reward;
            _optimalRewards += reward;
            
            var penalty = -Mathf.Clamp01(distanceToRoute) * 0.004f;
            AddReward(penalty);
            _denseRewards += penalty;
            _optimalRewards += penalty;

            for (var i = 0; i < _previousActions.Length; i++)
            {
                var actionDifferencePenalty = -Mathf.Abs(_previousActions[i] - actionBuffers.ContinuousActions[i]) * 0.004f;
                AddReward(actionDifferencePenalty);
                _denseRewards += actionDifferencePenalty;
                _actionPenalty += actionDifferencePenalty;
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
    
    private static bool AircraftArrivedExit(float distanceToTarget)
    {
        return distanceToTarget < 30f;
    }

    private bool IsEpisodeFailed(float distanceToRoute, bool illegalAircraftRotation)
    {
        return (distanceToRoute > 0.99f || illegalAircraftRotation) && trainingMode && _episodeStarted;
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
        observationCanvas.ChangeMode(1);
        //RAISE GEAR
        if (aircraftController.gearActuator != null && aircraftController.gearActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) { aircraftController.gearActuator.DisengageActuator(); }
        else { aircraftController.m_gearState = Controller.GearState.Up; }
        yield return new WaitForSeconds(0.5f);
        _episodeStarted = true;
    }
}
