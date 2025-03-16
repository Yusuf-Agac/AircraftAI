using System.Collections;
using System.Linq;
using Oyedoyin.Common;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AircraftFlightAgent : AircraftAgent
{
    [SerializeField, Header("Configurations    Flight Agent----------------------------------------------------------------------------------------------"), Space(10)] 
    public FlightPathNormalizer flightPathNormalizer;

    private void Awake() => PreviousActions = new float[]{0, 0, 0};

    protected override PathNormalizer PathNormalizer => flightPathNormalizer;

    protected override IEnumerator LazyEvaluation()
    {
        for (var i = 0; i < PreviousActions.Length; i++) PreviousActions[i] = 0;
        yield return null;
        observationCanvas.ChangeMode(1);
        rewardCanvas.ChangeMode(1);
        aircraftController.TurnOnEngines();

        yield return null;
        if (aircraftController.gearActuator && aircraftController.gearActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) aircraftController.gearActuator.DisengageActuator();
        else aircraftController.m_gearState = Controller.GearState.Up;

        yield return new WaitForSeconds(0.5f);
        EpisodeStarted = true;
    }

    protected override IEnumerator LazyEvaluationTraining()
    {
        yield return null;
        aircraftController.m_rigidbody.isKinematic = false;
        flightPathNormalizer.ResetTrainingPath();
        flightPathNormalizer.ResetAircraftTransform(transform);

        yield return null;
        aircraftController.HotResetAircraft();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        AtmosphereUtility.SmoothlyChangeWindAndTurbulence(aircraftController, evaluateAtmosphereData, DecisionRequester.DecisionPeriod);

        CalculateDirectionsSimilarities();
        CalculateMovementVariables();
        CalculateOptimalTransforms();
        CalculateDirectionDifferences(optimalDirections[0], AircraftForward, normalizedVelocity);
        CalculateDirectionSimilarities();
        CalculateAxesData();
        CalculateAtmosphere();

        sensor.AddObservation(AircraftForward);
        sensor.AddObservation(AircraftUp);
        sensor.AddObservation(DotLocalForwardGlobalUp);
        sensor.AddObservation(DotLocalUpGlobalDown);

        sensor.AddObservation(normalizedSpeed);
        sensor.AddObservation(NormalizedThrust);
        sensor.AddObservation(normalizedVelocity);

        sensor.AddObservation(NormalizedOptimalDistance);
        foreach (var optimalDirection in optimalDirections) sensor.AddObservation(optimalDirection);

        sensor.AddObservation(ForwardOptimalDifference);
        sensor.AddObservation(VelocityOptimalDifference);

        sensor.AddObservation(DotVelocityRotation);
        sensor.AddObservation(DotVelocityOptimal);
        sensor.AddObservation(DotRotationOptimal);

        sensor.AddObservation(PreviousActions);
        sensor.AddObservation(NormalizedTargetAxes);
        sensor.AddObservation(NormalizedCurrentAxes);
        sensor.AddObservation(NormalizedAxesRates);

        sensor.AddObservation(NormalizedWind);

        observationCanvas.DisplayNormalizedData(
            AircraftForward, AircraftUp, DotLocalForwardGlobalUp, DotLocalUpGlobalDown,
            normalizedVelocity, normalizedSpeed, NormalizedThrust,
            NormalizedOptimalDistance, optimalDirections,
            ForwardOptimalDifference, VelocityOptimalDifference,
            DotVelocityRotation, DotVelocityOptimal, DotRotationOptimal,
            PreviousActions,
            NormalizedTargetAxes,
            NormalizedCurrentAxes,
            NormalizedAxesRates,
            WindAngle, WindSpeed, NormalizedWind[1], Turbulence, NormalizedWind[2]
        );
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        aircraftController.m_input.SetAgentInputs(actionBuffers, aircraftBehaviourConfig.manoeuvreSpeed);

        CalculateDirectionsSimilarities();

        if (EpisodeStarted)
        {
            SetOptimalDistanceReward();
            SetActionDifferenceReward(actionBuffers);
            CalculateMovementVariables();
            CalculateOptimalTransforms();
            CalculateDirectionSimilarities();
            SetDirectionDifferenceReward();
            
            if (IsEpisodeSucceed())
            {
                if (trainingMode)
                {
                    SetSparseReward(true);
                    EndEpisode();
                }
                else if (BehaviorSelector) BehaviorSelector.SelectNextBehavior();
            }
            else if (IsEpisodeFailed())
            {
                if (trainingMode)
                {
                    SetSparseReward(false);
                    EndEpisode();   
                }
            }
        }

        rewardCanvas.DisplayReward(SparseRewards, DenseRewards, OptimalDistanceRewards, ActionDifferenceReward, ForwardVelocityDifferenceReward, OptimalVelocityDifferenceReward);

        PreviousActions = actionBuffers.ContinuousActions.ToArray();
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = pitchSlider.value;
        continuousActionsOut[1] = rollSlider.value;
        continuousActionsOut[2] = yawSlider.value;
        aircraftController.m_input.SetAgentInputs(actionsOut, aircraftBehaviourConfig.manoeuvreSpeed);
    }

    protected override bool IsEpisodeFailed()
    {
        var illegalAircraftRotation = DotLocalForwardGlobalUp is > 0.5f or < -0.5f || DotLocalUpGlobalDown > -0.5f;
        return (NormalizedOptimalDistance > 0.99f || illegalAircraftRotation) && trainingMode && EpisodeStarted;
    }

    protected override bool IsEpisodeSucceed()
    {
        return flightPathNormalizer.NormalizedArriveDistance(transform.position) < 1;
    }
}