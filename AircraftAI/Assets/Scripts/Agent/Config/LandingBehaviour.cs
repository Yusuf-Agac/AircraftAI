using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
class LandingBehaviour : AircraftBehaviour
{
    [SerializeField] private AirportNormalizer airportNormalizer;
    [SerializeField] private AircraftCollisionDetector detector;
    private AircraftLandingAgent _aircraftLandingAgent;

    public override void SetBehaviorComponent(Transform transform, BehaviourDependencies dependencies)
    {
        AddBehaviorComponent(transform);
        
        SetBehaviorProperties();
        
        _aircraftLandingAgent.airportNormalizer = airportNormalizer;
        airportNormalizer.aircraftAgents.Clear();
        airportNormalizer.aircraftAgents.Add(_aircraftLandingAgent);
        
        _aircraftLandingAgent.detector = detector;
        _aircraftLandingAgent.observationCanvas = dependencies.observationCanvas;
        _aircraftLandingAgent.rewardCanvas = dependencies.rewardCanvas;
        _aircraftLandingAgent.windArrowRenderers = dependencies.windArrows;
        _aircraftLandingAgent.windAudioSource = dependencies.windAudioSource;
        
        AddDecisionRequester(transform);
    }
    
    protected override void AddBehaviorComponent(Transform transform)
    {
        base.AddBehaviorComponent(transform);
        _aircraftLandingAgent = transform.AddComponent<AircraftLandingAgent>();
        Agent = _aircraftLandingAgent;
    }
}