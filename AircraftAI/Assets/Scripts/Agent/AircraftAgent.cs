using Cysharp.Threading.Tasks;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public abstract partial class AircraftAgent : Agent
{
    [SerializeField, Header("Configurations    Dependencies----------------------------------------------------------------------------------------------"), Space(10)] 
    public MeshRenderer[] windArrowRenderers;
    [Space(5)] 
    public FixedController aircraftController;
    [Space(5)]
    public AudioSource windAudioSource;
    [Space(5)] 
    public ObservationCanvas observationCanvas;
    public RewardCanvas rewardCanvas;
    [Space(5)] 
    public Slider pitchSlider;
    public Slider rollSlider;
    public Slider yawSlider;

    [FormerlySerializedAs("aircraftBehaviorConfig")] [SerializeField, Header("Configurations    General----------------------------------------------------------------------------------------------"), Space(10)]
    public AircraftBehaviorConfig aircraftBehaviourConfig;
    
    [Header("Configurations    Reward----------------------------------------------------------------------------------------------"), Space(10)] 
    [SerializeField] protected float sparseRewardMultiplier = 1f;
    [SerializeField] protected float denseRewardMultiplier = 0.001f;

    [Space(10)] 
    [SerializeField, Range(0, 5)] private int sparseWinReward = 1;
    [SerializeField, Range(0, -5)] private int sparseLoseReward = -1;
    
    [Space(10)] 
    [SerializeField] protected float optimalDistanceReward = 8f;
    [SerializeField] protected float optimalDistancePenalty = 4f;
    [SerializeField] protected float actionDifferencePenalty = 4f;
    [SerializeField] protected float forwardVelocityDifferencePenalty = 4;
    [SerializeField] protected float optimalVelocityDifferencePenalty = 4;
    
    [Space(10)] 
    [SerializeField, Range(0, 0.25f)] private float forwardVelocityDifferenceTolerance = 0.005f;
    [SerializeField, Range(0, 50f)] private float forwardVelocityDifferenceSensitivity = 30f;
    [SerializeField, Range(0, 0.25f)] private float optimalVelocityDifferenceTolerance = 0.12f;
    [SerializeField, Range(0, 50f)] private float optimalVelocityDifferenceSensitivity = 10f;
    [SerializeField, Range(0, 1f)] private float directionalDifferenceThreshold = 0.5f;
    
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
    protected float DotLocalForwardGlobalUp;
    protected float DotLocalUpGlobalDown;
    
    protected float NormalizedThrust;
    [HideInInspector] public float normalizedSpeed;
    [HideInInspector] public Vector3 normalizedVelocity;
    
    protected float[] PreviousActions;
    protected Vector3 NormalizedTargetAxes;
    protected Vector3 NormalizedCurrentAxes;
    protected Vector3 NormalizedAxesRates;
    
    protected Vector3 ForwardOptimalDifference;
    protected Vector3 VelocityOptimalDifference;
    
    protected float DotVelocityRotation;
    protected float DotVelocityOptimal;
    protected float DotRotationOptimal;

    protected float[] NormalizedWind;
    protected float WindAngle;
    protected float WindSpeed;
    protected float Turbulence;
    
    protected float NormalizedOptimalDistance;
    [HideInInspector] public Vector3[] optimalDirections;
    
    protected abstract PathNormalizer PathNormalizer { get; }

    protected abstract UniTask LazyEvaluation();
    protected abstract UniTask LazyEvaluationTraining();

    private void Start()
    {
        BehaviorSelector = GetComponent<BehaviorSelector>();
        aircraftController = GetComponent<FixedController>();
        DecisionRequester = GetComponent<DecisionRequester>();
    }

    public override void OnEpisodeBegin()
    {
        EpisodeStarted = false;
        LazyEvaluation().Forget();
        if (aircraftBehaviourConfig && aircraftBehaviourConfig.trainingMode)
        {
            aircraftController.m_rigidbody.isKinematic = true;
            ResetAtmosphereBoundsForTraining();
            LazyEvaluationTraining().Forget();
        }
    }
    
    private void ResetAtmosphereBoundsForTraining()
    {
        aircraftBehaviourConfig.evaluateAtmosphereData.maxWindSpeed = Random.Range(0, aircraftBehaviourConfig.trainingAtmosphereData.maxWindSpeed);
        aircraftBehaviourConfig.evaluateAtmosphereData.maxTurbulence = Random.Range(0, aircraftBehaviourConfig.trainingAtmosphereData.maxTurbulence);
        aircraftBehaviourConfig.evaluateAtmosphereData.maxWindDirectionChangeSpeed = Random.Range(0, aircraftBehaviourConfig.trainingAtmosphereData.maxWindDirectionChangeSpeed);
    }
    
    protected void SetSparseReward(bool success)
    {
        SetReward(sparseRewardMultiplier * (success ? sparseWinReward : sparseLoseReward));
        SparseRewards += sparseRewardMultiplier * (success ? sparseWinReward : sparseLoseReward);
    }
    
    protected void SetOptimalDistanceReward()
    {
        var subtractedDistance = Mathf.Clamp01(1 - NormalizedOptimalDistance);
        var distanceReward = subtractedDistance * denseRewardMultiplier * optimalDistanceReward;
        AddReward(distanceReward);
        DenseRewards += distanceReward;
        OptimalDistanceRewards += distanceReward;

        var distance = Mathf.Clamp01(NormalizedOptimalDistance);
        var distancePenalty = -distance * denseRewardMultiplier * optimalDistancePenalty;
        AddReward(distancePenalty);
        DenseRewards += distancePenalty;
        OptimalDistanceRewards += distancePenalty;
    }
    
    protected void SetActionDifferenceReward(ActionBuffers actionBuffers)
    {
        for (var i = 0; i < PreviousActions.Length; i++)
        {
            var actionChange = Mathf.Abs(PreviousActions[i] - actionBuffers.ContinuousActions[i]);
            var actionChangePenalty = -actionChange * denseRewardMultiplier * actionDifferencePenalty;
            AddReward(actionChangePenalty);
            DenseRewards += actionChangePenalty;
            ActionDifferenceReward += actionChangePenalty;
        }
    }
    
    protected void SetDirectionDifferenceReward()
    {
        if(aircraftController.m_rigidbody.linearVelocity.magnitude < directionalDifferenceThreshold) return;
        var forwardDifferenceExpectation = (1 - forwardVelocityDifferenceTolerance);
        var forwardVelocityDifference = NormalizeUtility.ClampNP1((DotVelocityRotation - forwardDifferenceExpectation) * forwardVelocityDifferenceSensitivity);
        var velocityDifferencePenalty = forwardVelocityDifference * denseRewardMultiplier *
                                        forwardVelocityDifferencePenalty;
        AddReward(velocityDifferencePenalty);
        DenseRewards += velocityDifferencePenalty;
        ForwardVelocityDifferenceReward += velocityDifferencePenalty;

        var optimalDifferenceExpectation = (1 - optimalVelocityDifferenceTolerance);
        var optimalVelocityDifference = NormalizeUtility.ClampNP1((DotVelocityOptimal - optimalDifferenceExpectation) * optimalVelocityDifferenceSensitivity);
        var result = optimalVelocityDifference * denseRewardMultiplier * optimalVelocityDifferencePenalty;
        AddReward(result);
        DenseRewards += result;
        OptimalVelocityDifferenceReward += result;
    }
    
    protected void CalculateAtmosphere()
    {
        NormalizedWind = AtmosphereUtility.NormalizedWind(aircraftController, aircraftBehaviourConfig.trainingAtmosphereData);
        WindAngle = NormalizedWind[0] * 360;
        WindSpeed = NormalizedWind[1] * aircraftBehaviourConfig.trainingAtmosphereData.maxWindSpeed;
        Turbulence = NormalizedWind[2] * aircraftBehaviourConfig.trainingAtmosphereData.maxTurbulence;

        UpdateWindObjects();
    }

    private void UpdateWindObjects()
    {
        if (windArrowRenderers != null)
        {
            windArrowRenderers[0].transform.localEulerAngles = new Vector3(0, WindAngle, 0);
            windArrowRenderers[1].transform.localEulerAngles = new Vector3(0, WindAngle, 0);
            foreach (var windArrow in windArrowRenderers)
            {
                var material = windArrow.material;
                material.color = Color.Lerp(Color.green, Color.red, NormalizedWind[1]);
                windArrow.material = material;
            }
        }

        if (windAudioSource)
        {
            windAudioSource.volume = Mathf.Lerp(0.1f, 1f, NormalizedWind[1]);
            windAudioSource.pitch = Mathf.Lerp(1f, 1.7f, NormalizedWind[1]);
        }
    }

    protected void CalculateAxesData()
    {
        NormalizedTargetAxes = AircraftNormalizeUtility.NormalizedTargetAxes(aircraftController);
        NormalizedCurrentAxes = AircraftNormalizeUtility.NormalizedCurrentAxes(aircraftController);
        NormalizedAxesRates = AircraftNormalizeUtility.NormalizeAxesRates(aircraftController);
    }
    
    protected void CalculateDirectionSimilarities()
    {
        DotVelocityRotation = Vector3.Dot(normalizedVelocity, AircraftForward);
        DotVelocityOptimal = Vector3.Dot(normalizedVelocity, optimalDirections[0]);
        DotRotationOptimal = Vector3.Dot(AircraftForward, optimalDirections[0]);
    }
    
    protected void CalculateDirectionsSimilarities()
    {
        AircraftForward = transform.forward;
        AircraftUp = transform.up;
        DotLocalForwardGlobalUp = Vector3.Dot(AircraftForward, Vector3.up);
        DotLocalUpGlobalDown = Vector3.Dot(AircraftUp, Vector3.down);
    }

    protected void CalculateDirectionDifferences(Vector3 optimal, Vector3 forward, Vector3 velocity)
    {
        ForwardOptimalDifference = (optimal - forward) / 2f;
        VelocityOptimalDifference = (optimal - velocity) / 2f;
    }

    protected virtual void CalculateMovementVariables()
    {
        normalizedSpeed = AircraftNormalizeUtility.NormalizedSpeed(aircraftController);
        NormalizedThrust = AircraftNormalizeUtility.NormalizedThrust(aircraftController);
        normalizedVelocity = aircraftController.m_rigidbody.linearVelocity.normalized;
    }

    protected abstract bool IsEpisodeFailed();
    protected abstract bool IsEpisodeSucceed();

    public virtual void CalculateOptimalTransforms()
    {
        NormalizedOptimalDistance = PathNormalizer.NormalizedOptimalPositionDistance(transform.position);
        optimalDirections = PathNormalizer.OptimalDirections(transform, aircraftBehaviourConfig.numOfOptimalDirections, aircraftBehaviourConfig.gapBetweenOptimalDirections);
    }
}