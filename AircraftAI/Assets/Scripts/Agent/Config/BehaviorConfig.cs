using System;
using Oyedoyin.FixedWing;
using Unity.Barracuda;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

[Serializable]
public abstract class BehaviorConfig
{
    public string behaviorName;
    public int spaceSize;
    public ActionSpec actionSpecs;
    public NNModel model;
    [Range(1, 25)] public int decisionPeriod = 1;
    public int maxStep = 2500000;

    [SerializeField, Space(10)] protected AircraftBehaviourConfig aircraftBehaviourConfig;
    [SerializeField, Space(10)] protected AtmosphereData atmosphereMaxData;
    [SerializeField, Space(10)] protected AtmosphereData atmosphereTrainData;

    protected Agent Agent;
    protected DecisionRequester DecisionRequester;
    private BehaviorParameters _behaviorParameters;
    
    public abstract void SetBehaviorComponent(Transform transform, BehaviourDependencies dependencies);
    
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
        _behaviorParameters.BrainParameters.ActionSpec = actionSpecs;
        _behaviorParameters.Model = model;
    }
    
    public void RemoveBehaviorComponent()
    {
        if(DecisionRequester) Object.Destroy(DecisionRequester);
        if(Agent) Object.Destroy(Agent);
        if(_behaviorParameters) Object.Destroy(_behaviorParameters);
    }
}

[Serializable]
public class BehaviourDependencies
{
    public MeshRenderer[] windArrows;
    public ObservationCanvas observationCanvas;
    public RewardCanvas rewardCanvas;
    public AudioSource windAudioSource;
}