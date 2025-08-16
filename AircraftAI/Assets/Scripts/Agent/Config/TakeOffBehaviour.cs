using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class TakeOffBehaviour : AircraftBehaviour
{
    [SerializeField] private AirportNormalizer airportNormalizer;
    [SerializeField] private AircraftCollisionDetector detector;
    
    private AircraftTakeOffAgent _aircraftTakeOffAgent;

    public override void SetBehaviorComponent(Transform transform, BehaviourDependencies dependencies)
    {
        AddBehaviorComponent(transform);
        
        SetBehaviorProperties();
        
        _aircraftTakeOffAgent.airportNormalizer = airportNormalizer;
        airportNormalizer.aircraftAgents.Clear();
        airportNormalizer.aircraftAgents.Add(_aircraftTakeOffAgent);
        
        _aircraftTakeOffAgent.detector = detector;
        _aircraftTakeOffAgent.observationCanvas = dependencies.observationCanvas;
        _aircraftTakeOffAgent.rewardCanvas = dependencies.rewardCanvas;
        _aircraftTakeOffAgent.windAudioSource = dependencies.windAudioSource;
        
        AddDecisionRequester(transform);
    }
    
    protected override void AddBehaviorComponent(Transform transform)
    {
        base.AddBehaviorComponent(transform);
        _aircraftTakeOffAgent = transform.AddComponent<AircraftTakeOffAgent>();
        Agent = _aircraftTakeOffAgent;
    }
}