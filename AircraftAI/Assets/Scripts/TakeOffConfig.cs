using System;
using Oyedoyin.FixedWing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
class TakeOffConfig : BehaviorConfig
{
    [Space(10)]
    [SerializeField] private AirportNormalizer airportNormalizer;
    [SerializeField] private AircraftCollisionSensors sensors;
    [SerializeField] private ObservationCanvas observationCanvas;
    [SerializeField] private FixedController aircraftController;
    [SerializeField] private Slider pitchSlider;
    [SerializeField] private Slider rollSlider;
    [SerializeField] private Slider throttleSlider;
    
    public override void SetBehaviorComponent(Transform transform)
    {
        AddBehaviorComponent(transform);
        
        var aircraftTakeOffAgent = transform.AddComponent<AircraftTakeOffAgent>();
        Agent = aircraftTakeOffAgent;
        
        aircraftTakeOffAgent.MaxStep = maxStep;

        aircraftTakeOffAgent.trainingMode = false;
        aircraftTakeOffAgent.manoeuvreSpeed = manoeuvreSpeed;
        aircraftTakeOffAgent.maxWindSpeed = maxWindSpeed;
        aircraftTakeOffAgent.maxTurbulence = maxTurbulence;
        aircraftTakeOffAgent.numOfOptimalDirections = numOfOptimumDirections;
        aircraftTakeOffAgent.gapBetweenOptimalDirections = gapBetweenOptimumDirections;
        
        aircraftTakeOffAgent.airportNormalizer = airportNormalizer;
        airportNormalizer.aircraftAgents.Clear();
        airportNormalizer.aircraftAgents.Add(aircraftTakeOffAgent);
        
        aircraftTakeOffAgent.sensors = sensors;
        aircraftTakeOffAgent.observationCanvas = observationCanvas;
        aircraftTakeOffAgent.aircraftController = aircraftController;
        aircraftTakeOffAgent.pitchSlider = pitchSlider;
        aircraftTakeOffAgent.rollSlider = rollSlider;
        aircraftTakeOffAgent.throttleSlider = throttleSlider;
        
        AddDecisionRequester(transform);
    }
}