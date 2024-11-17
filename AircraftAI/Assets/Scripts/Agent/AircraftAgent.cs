using System.Collections;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using UnityEngine;
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
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward.normalized * 40);
        Gizmos.color = Color.yellow;
        if(aircraftController.m_rigidbody) Gizmos.DrawLine(transform.position, transform.position + aircraftController.m_rigidbody.velocity.normalized * 40);
    }
}