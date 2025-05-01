using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
class FlightBehaviour : AircraftBehaviour
{
    [SerializeField] private FlightPathNormalizer flightPathNormalizer;

    private AircraftFlightAgent _aircraftFlightAgent;

    public override void SetBehaviorComponent(Transform transform, BehaviourDependencies dependencies)
    {
        AddBehaviorComponent(transform);
        
        SetBehaviorProperties();
        
        _aircraftFlightAgent.flightPathNormalizer = flightPathNormalizer;
        flightPathNormalizer.aircraftAgents.Clear();
        flightPathNormalizer.aircraftAgents.Add(_aircraftFlightAgent);
        
        _aircraftFlightAgent.observationCanvas = dependencies.observationCanvas;
        _aircraftFlightAgent.rewardCanvas = dependencies.rewardCanvas;
        _aircraftFlightAgent.windArrowRenderers = dependencies.windArrows;
        _aircraftFlightAgent.windAudioSource = dependencies.windAudioSource;
        
        AddDecisionRequester(transform);
    }

    protected override void AddBehaviorComponent(Transform transform)
    {
        base.AddBehaviorComponent(transform);
        _aircraftFlightAgent = transform.AddComponent<AircraftFlightAgent>();
        Agent = _aircraftFlightAgent;
    }
}