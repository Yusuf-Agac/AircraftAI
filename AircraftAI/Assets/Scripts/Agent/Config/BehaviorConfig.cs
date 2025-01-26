using System;
using Oyedoyin.FixedWing;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
abstract class BehaviorConfig
{
    public string behaviorName;
    public int spaceSize;
    public int continuousActions;
    public NNModel model;
    [Range(1, 25)] public int decisionPeriod = 1;
    
    [Space(10)]
    [Range(0.1f, 25f)] public float manoeuvreSpeed = 10f;
    [SerializeField] protected float windDirectionSpeed = 360;
    [SerializeField] protected float maxWindSpeed = 5;
    [SerializeField] protected float maxTurbulence = 5;
    [SerializeField] protected int numOfOptimumDirections = 2;
    [SerializeField] protected int gapBetweenOptimumDirections = 2;
    [SerializeField] protected int maxStep = 2500000;
    [Space(10)]
    [SerializeField] protected ObservationCanvas observationCanvas;
    [SerializeField] protected RewardCanvas rewardCanvas;
    [SerializeField] protected FixedController aircraftController;
    [SerializeField] protected MeshRenderer[] windArrows;
    [SerializeField] protected AudioSource windAudioSource;
    
    [Space(10)]
    protected Agent Agent;
    private BehaviorParameters _behaviorParameters;
    protected DecisionRequester DecisionRequester;
    
    public virtual void SetBehaviorComponent(Transform transform) { }
    
    protected void AddDecisionRequester(Transform transform)
    {
        DecisionRequester = transform.AddComponent<DecisionRequester>();
        DecisionRequester.DecisionPeriod = decisionPeriod;
    }
    
    protected void AddBehaviorComponent(Transform transform)
    {
        _behaviorParameters = transform.gameObject.AddComponent<BehaviorParameters>();
        
        _behaviorParameters.BehaviorName = behaviorName;
        _behaviorParameters.BrainParameters.VectorObservationSize = spaceSize;
        _behaviorParameters.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(continuousActions);
        _behaviorParameters.Model = model;
    }
    
    public void RemoveBehaviorComponent()
    {
        if(DecisionRequester != null) Object.Destroy(DecisionRequester);
        if(Agent != null) Object.Destroy(Agent);
        if(_behaviorParameters != null) Object.Destroy(_behaviorParameters);
    }
}