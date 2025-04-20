using System;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public abstract class AircraftBehaviour
{
    public AircraftBehaviorConfig aircraftBehaviorConfig;
    
    protected AircraftAgent Agent;
    private DecisionRequester _decisionRequester;
    private BehaviorParameters _behaviorParameters;

    public abstract void SetBehaviorComponent(Transform transform, BehaviourDependencies dependencies);

    protected void SetBehaviorProperties()
    {
        Agent.MaxStep = aircraftBehaviorConfig.maxStep;
        Agent.aircraftBehaviourConfig = aircraftBehaviorConfig;
    }
    
    protected void AddDecisionRequester(Transform transform)
    {
        _decisionRequester = transform.AddComponent<DecisionRequester>();
        _decisionRequester.DecisionPeriod = aircraftBehaviorConfig.decisionPeriod;
    }
    
    protected virtual void AddBehaviorComponent(Transform transform)
    {
        _behaviorParameters = transform.gameObject.AddComponent<BehaviorParameters>();
        
        _behaviorParameters.BehaviorName = aircraftBehaviorConfig.behaviorName;
        _behaviorParameters.BrainParameters.VectorObservationSize = aircraftBehaviorConfig.spaceSize;
        _behaviorParameters.BrainParameters.ActionSpec = aircraftBehaviorConfig.actionSpecs;
        _behaviorParameters.Model = aircraftBehaviorConfig.model;
    }
    
    public void RemoveBehaviorComponent()
    {
        if(_decisionRequester) Object.Destroy(_decisionRequester);
        if(Agent) Object.Destroy(Agent);
        if(_behaviorParameters) Object.Destroy(_behaviorParameters);
    }
}