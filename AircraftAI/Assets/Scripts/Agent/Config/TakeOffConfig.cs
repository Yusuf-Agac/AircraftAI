using System;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class TakeOffConfig : BehaviorConfig
{
    [Header("Configurations    Takeoff----------------------------------------------------------------------------------------------"), Space(10)] 
    [SerializeField] private AirportNormalizer airportNormalizer;
    [SerializeField] private AircraftCollisionDetector detector;
    
    public override void SetBehaviorComponent(Transform transform, BehaviourDependencies dependencies)
    {
        AddBehaviorComponent(transform);
        
        var aircraftTakeOffAgent = transform.AddComponent<AircraftTakeOffAgent>();
        Agent = aircraftTakeOffAgent;
        
        aircraftTakeOffAgent.MaxStep = maxStep;

        aircraftTakeOffAgent.trainingMode = false;
        aircraftTakeOffAgent.aircraftBehaviourConfig = aircraftBehaviourConfig;
        aircraftTakeOffAgent.evaluateAtmosphereData = atmosphereMaxData;
        aircraftTakeOffAgent.trainingAtmosphereData = atmosphereTrainData;
        
        aircraftTakeOffAgent.airportNormalizer = airportNormalizer;
        airportNormalizer.aircraftAgents.Clear();
        airportNormalizer.aircraftAgents.Add(aircraftTakeOffAgent);
        
        aircraftTakeOffAgent.detector = detector;
        aircraftTakeOffAgent.observationCanvas = dependencies.observationCanvas;
        aircraftTakeOffAgent.rewardCanvas = dependencies.rewardCanvas;
        aircraftTakeOffAgent.windArrowRenderers = dependencies.windArrows;
        aircraftTakeOffAgent.windAudioSource = dependencies.windAudioSource;
        
        AddDecisionRequester(transform);
    }
}