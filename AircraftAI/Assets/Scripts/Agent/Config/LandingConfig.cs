using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
class LandingConfig : BehaviorConfig
{
    [Header("Configurations    Landing----------------------------------------------------------------------------------------------"), Space(10)] 
    [SerializeField] private AirportNormalizer airportNormalizer;
    [SerializeField] private AircraftCollisionDetector detector;
    
    public override void SetBehaviorComponent(Transform transform, BehaviourDependencies dependencies)
    {
        AddBehaviorComponent(transform);
        
        var aircraftLandingAgent = transform.AddComponent<AircraftLandingAgent>();
        Agent = aircraftLandingAgent;
        
        aircraftLandingAgent.MaxStep = maxStep;

        aircraftLandingAgent.trainingMode = false;
        aircraftLandingAgent.aircraftBehaviourConfig = aircraftBehaviourConfig;
        aircraftLandingAgent.evaluateAtmosphereData = atmosphereMaxData;
        aircraftLandingAgent.trainingAtmosphereData = atmosphereTrainData;
        
        aircraftLandingAgent.airportNormalizer = airportNormalizer;
        airportNormalizer.aircraftAgents.Clear();
        airportNormalizer.aircraftAgents.Add(aircraftLandingAgent);
        
        aircraftLandingAgent.detector = detector;
        aircraftLandingAgent.observationCanvas = dependencies.observationCanvas;
        aircraftLandingAgent.rewardCanvas = dependencies.rewardCanvas;
        aircraftLandingAgent.windArrowRenderers = dependencies.windArrows;
        aircraftLandingAgent.windAudioSource = dependencies.windAudioSource;
        
        AddDecisionRequester(transform);
    }
}