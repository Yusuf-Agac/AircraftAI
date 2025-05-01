using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Oyedoyin.Common;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AircraftFlightAgent : AircraftAgent
{
    public FlightPathNormalizer flightPathNormalizer;

    protected override void Awake()
    {
        base.Awake();
        PreviousActions = new float[] { 0, 0, 0 };
    }

    protected override PathNormalizer PathNormalizer => flightPathNormalizer;

    protected override async UniTask LazyEvaluation()
    {
        for (var i = 0; i < PreviousActions.Length; i++) PreviousActions[i] = 0;
        await UniTask.Yield(PlayerLoopTiming.Update);
        observationCanvas.ChangeMode(1);
        rewardCanvas.ChangeMode(1);
        aircraftController.TurnOnEngines();

        await UniTask.Yield(PlayerLoopTiming.Update);
        if (aircraftController.gearActuator && aircraftController.gearActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) aircraftController.gearActuator.DisengageActuator();
        else aircraftController.m_gearState = Controller.GearState.Up;

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        EpisodeStarted = true;
    }

    protected override async UniTask LazyEvaluationTraining()
    {
        await UniTask.Yield(PlayerLoopTiming.Update);
        
        aircraftController.m_rigidbody.isKinematic = false;
        flightPathNormalizer.ResetTrainingPath();
        flightPathNormalizer.ResetAircraftTransform(transform);

        await UniTask.Yield(PlayerLoopTiming.Update);
        
        aircraftController.HotResetAircraft();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        AtmosphereUtility.SmoothlyChangeWindAndTurbulence(aircraftController, aircraftBehaviourConfig.evaluateAtmosphereData, DecisionRequester.DecisionPeriod);

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
                if (aircraftBehaviourConfig.trainingMode)
                {
                    SetSparseReward(true);
                    EndEpisode();
                }
                else if (BehaviorSelector) BehaviorSelector.SelectNextBehavior();
            }
            else if (IsEpisodeFailed())
            {
                if (aircraftBehaviourConfig.trainingMode)
                {
                    SetSparseReward(false);
                    EndEpisode();   
                }
            }
        }

        rewardCanvas.DisplayReward(SparseRewards, DenseRewards, OptimalDistanceRewards, ActionDifferenceReward, ForwardVelocityDifferenceReward, OptimalVelocityDifferenceReward);

        PreviousActions = actionBuffers.ContinuousActions.ToArray();
    }

    protected override bool IsEpisodeFailed()
    {
        var illegalAircraftRotation = DotLocalForwardGlobalUp is > 0.5f or < -0.5f || DotLocalUpGlobalDown > -0.5f;
        return (NormalizedOptimalDistance > 0.99f || illegalAircraftRotation) && aircraftBehaviourConfig.trainingMode && EpisodeStarted;
    }

    protected override bool IsEpisodeSucceed()
    {
        return flightPathNormalizer.NormalizedArriveDistance(transform.position) < 1;
    }
}