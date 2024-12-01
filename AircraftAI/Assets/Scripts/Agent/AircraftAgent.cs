using System.Collections;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public abstract class AircraftAgent : Agent
{
    public FixedController aircraftController;
    
    public bool trainingMode;
    
    [Space(10)] 
    public float windDirectionSpeed = 360;
    public float trainingMaxWindSpeed = 15;
    public float maxWindSpeed = 15;
    public float trainingMaxTurbulence = 15;
    public float maxTurbulence = 15;
    
    [Space(10)] 
    [Range(0.1f, 25f)] public float manoeuvreSpeed = 10f;
    
    [Space(10)] 
    [Range(1, 3)] public int numOfOptimalDirections = 1;
    [Range(1, 10)] public int gapBetweenOptimalDirections = 1;

    [Space(10)] 
    public ObservationCanvas observationCanvas;
    public RewardCanvas rewardCanvas;
    
    [Space(10)] 
    public Slider pitchSlider;
    public Slider rollSlider;
    public Slider yawSlider;
    
    [Space(10)] 
    [SerializeField] protected float sparseRewardMultiplier = 1f;
    [SerializeField] protected float denseRewardMultiplier = 0.001f;

    [Space(5)] 
    [SerializeField] protected float optimalDistanceRewardMultiplier = 8f;
    [SerializeField] protected float optimalDistancePenaltyMultiplier = 4f;
    [SerializeField] protected float actionDifferencePenaltyMultiplier = 4f;
    [SerializeField] protected float forwardVelocityDifferencePenaltyMultiplier = 4;
    [SerializeField] protected float optimalVelocityDifferencePenaltyMultiplier = 4;
    
    protected DecisionRequester DecisionRequester;
    protected BehaviorSelector BehaviorSelector;
    
    protected bool EpisodeStarted;

    protected float SparseRewards;
    protected float DenseRewards;
    protected float OptimalDistanceRewards;
    protected float ActionDifferenceReward;
    protected float ForwardVelocityDifferenceReward;
    protected float OptimalVelocityDifferenceReward;
    
    protected Vector3 AircraftForward;
    protected Vector3 AircraftUp;
    protected float DotForwardUp;
    protected float DotUpDown;
    
    protected float NormalizedThrust;
    [HideInInspector] public float normalizedSpeed;
    [HideInInspector] public Vector3 normalizedVelocity;
    
    protected float[] PreviousActions;
    protected Vector3 NormalizedTargetAxes;
    protected Vector3 NormalizedCurrentAxes;
    protected Vector3 NormalizedAxesRates;
    
    protected Vector3 FwdOptDifference;
    protected Vector3 VelOptDifference;
    
    protected float DotVelRot;
    protected float DotVelOpt;
    protected float DotRotOpt;

    protected float[] WindData;
    protected float WindAngle;
    protected float WindSpeed;
    protected float Turbulence;
    
    protected float NormalizedOptimalDistance;
    [HideInInspector] public Vector3[] optimalDirections;

    [SerializeField, Range(0, 5)] private int sparseWinReward = 1;
    [SerializeField, Range(0, -5)] private int sparseLoseReward = -1;
    [SerializeField, Range(0, 0.25f)] private float forwardVelocityDifferenceTolerance = 0.005f;
    [SerializeField, Range(0, 50f)] private float forwardVelocityDifferenceSensitivity = 30f;
    [SerializeField, Range(0, 0.25f)] private float optimalVelocityDifferenceTolerance = 0.12f;
    [SerializeField, Range(0, 50f)] private float optimalVelocityDifferenceSensitivity = 10f;
    [SerializeField, Range(0, 1f)]private float directionalDifferenceThreshold = 0.5f;

    protected abstract IEnumerator LazyEvaluation();
    protected abstract IEnumerator LazyEvaluationTraining();
    
    private void Start()
    {
        BehaviorSelector = GetComponent<BehaviorSelector>();
        aircraftController = GetComponent<FixedController>();
        DecisionRequester = GetComponent<DecisionRequester>();
    }

    public override void OnEpisodeBegin()
    {
        EpisodeStarted = false;
        StartCoroutine(LazyEvaluation());
        if (trainingMode)
        {
            aircraftController.m_rigidbody.isKinematic = true;
            ResetAtmosphereBounds();
            StartCoroutine(LazyEvaluationTraining());
        }
    }
    
    private void ResetAtmosphereBounds()
    {
        maxWindSpeed = Random.Range(0, trainingMaxWindSpeed);
        maxTurbulence = Random.Range(0, trainingMaxTurbulence);
    }
    
    protected void SetSparseReward(bool success)
    {
        sparseWinReward = 1;
        SetReward(sparseRewardMultiplier * (success ? sparseWinReward : sparseLoseReward));
        SparseRewards += sparseRewardMultiplier * (success ? sparseWinReward : sparseLoseReward);
    }
    
    protected void SetOptimalDistanceReward()
    {
        var subtractedDistance = Mathf.Clamp01(1 - NormalizedOptimalDistance);
        var distanceReward = subtractedDistance * denseRewardMultiplier * optimalDistanceRewardMultiplier;
        AddReward(distanceReward);
        DenseRewards += distanceReward;
        OptimalDistanceRewards += distanceReward;

        var distance = Mathf.Clamp01(NormalizedOptimalDistance);
        var distancePenalty = -distance * denseRewardMultiplier * optimalDistancePenaltyMultiplier;
        AddReward(distancePenalty);
        DenseRewards += distancePenalty;
        OptimalDistanceRewards += distancePenalty;
    }
    
    protected void SetActionDifferenceReward(ActionBuffers actionBuffers)
    {
        for (var i = 0; i < PreviousActions.Length; i++)
        {
            var actionChange = Mathf.Abs(PreviousActions[i] - actionBuffers.ContinuousActions[i]);
            var actionChangePenalty = -actionChange * denseRewardMultiplier * actionDifferencePenaltyMultiplier;
            AddReward(actionChangePenalty);
            DenseRewards += actionChangePenalty;
            ActionDifferenceReward += actionChangePenalty;
        }
    }
    
    protected void SetDirectionDifferenceReward()
    {
        if(aircraftController.m_rigidbody.velocity.magnitude < directionalDifferenceThreshold) return;
        var forwardVelocityDifference = NormalizeHelper.ClampNP1((DotVelRot - (1 - forwardVelocityDifferenceTolerance)) * forwardVelocityDifferenceSensitivity);
        var velocityDifferencePenalty = forwardVelocityDifference * denseRewardMultiplier *
                                        forwardVelocityDifferencePenaltyMultiplier;
        AddReward(velocityDifferencePenalty);
        DenseRewards += velocityDifferencePenalty;
        ForwardVelocityDifferenceReward += velocityDifferencePenalty;

        var optimalVelocityDifference = NormalizeHelper.ClampNP1((DotVelOpt - (1 - optimalVelocityDifferenceTolerance)) * optimalVelocityDifferenceSensitivity);
        var optimalVelocityDifferencePenalty = optimalVelocityDifference * denseRewardMultiplier *
                                               optimalVelocityDifferencePenaltyMultiplier;
        AddReward(optimalVelocityDifferencePenalty);
        DenseRewards += optimalVelocityDifferencePenalty;
        OptimalVelocityDifferenceReward += optimalVelocityDifferencePenalty;
    }
    
    protected void CalculateAtmosphere()
    {
        WindData = AtmosphereHelper.NormalizedWind(aircraftController, trainingMaxWindSpeed,
            trainingMaxTurbulence);
        WindAngle = WindData[0] * 360;
        WindSpeed = WindData[1] * trainingMaxWindSpeed;
        Turbulence = WindData[2] * trainingMaxTurbulence;
    }
    
    protected void CalculateAxesData()
    {
        NormalizedTargetAxes = AircraftNormalizer.NormalizedTargetAxes(aircraftController);
        NormalizedCurrentAxes = AircraftNormalizer.NormalizedCurrentAxes(aircraftController);
        NormalizedAxesRates = AircraftNormalizer.NormalizeAxesRates(aircraftController);
    }
    
    protected void CalculateDirectionSimilarities()
    {
        DotVelRot = Vector3.Dot(normalizedVelocity, AircraftForward);
        DotVelOpt = Vector3.Dot(normalizedVelocity, optimalDirections[0]);
        DotRotOpt = Vector3.Dot(AircraftForward, optimalDirections[0]);
    }
    
    protected void CalculateGlobalDirections()
    {
        AircraftForward = transform.forward;
        AircraftUp = transform.up;
        DotForwardUp = Vector3.Dot(AircraftForward, Vector3.up);
        DotUpDown = Vector3.Dot(AircraftUp, Vector3.down);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward.normalized * 40);
        Gizmos.color = Color.yellow;
        if(aircraftController.m_rigidbody) Gizmos.DrawLine(transform.position, transform.position + aircraftController.m_rigidbody.velocity.normalized * 40);
    }
}