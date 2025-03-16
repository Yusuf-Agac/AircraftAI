using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
class FlightConfig : BehaviorConfig
{
    [Header("Configurations    Flight----------------------------------------------------------------------------------------------"), Space(10)] 
    [SerializeField] private FlightPathNormalizer flightPathNormalizer;
    
    public override void SetBehaviorComponent(Transform transform, BehaviourDependencies dependencies)
    {
        AddBehaviorComponent(transform);
        
        var aircraftFlightAgent = transform.AddComponent<AircraftFlightAgent>();
        Agent = aircraftFlightAgent;
        
        aircraftFlightAgent.MaxStep = maxStep;
        
        aircraftFlightAgent.trainingMode = false;
        aircraftFlightAgent.aircraftBehaviourConfig = aircraftBehaviourConfig;
        aircraftFlightAgent.evaluateAtmosphereData = atmosphereData;
        
        aircraftFlightAgent.flightPathNormalizer = flightPathNormalizer;
        flightPathNormalizer.aircraftAgents.Clear();
        flightPathNormalizer.aircraftAgents.Add(aircraftFlightAgent);
        
        aircraftFlightAgent.observationCanvas = dependencies.observationCanvas;
        aircraftFlightAgent.rewardCanvas = dependencies.rewardCanvas;
        aircraftFlightAgent.evaluateAtmosphereData = atmosphereData;
        
        AddDecisionRequester(transform);
    }
}