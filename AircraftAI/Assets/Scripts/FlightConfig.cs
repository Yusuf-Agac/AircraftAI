using System;
using Oyedoyin.FixedWing;
using Unity.MLAgents.Policies;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
class FlightConfig : BehaviorConfig
{
    [Space(10)]
    [SerializeField] private FlightPathNormalizer flightPathNormalizer;
    [SerializeField] private FixedController aircraftController;
    [SerializeField] private Slider pitchSlider;
    [SerializeField] private Slider rollSlider;
    [SerializeField] private Slider throttleSlider;
    
    public override void SetBehaviorComponent(Transform transform)
    {
        AddBehaviorComponent(transform);
        
        var aircraftFlightAgent = transform.AddComponent<AircraftFlightAgent>();
        Agent = aircraftFlightAgent;
        
        aircraftFlightAgent.MaxStep = maxStep;
        
        aircraftFlightAgent.trainingMode = false;
        aircraftFlightAgent.manoeuvreSpeed = manoeuvreSpeed;
        aircraftFlightAgent.maxWindSpeed = maxWindSpeed;
        aircraftFlightAgent.maxTurbulence = maxTurbulence;
        aircraftFlightAgent.numOfOptimumDirections = numOfOptimumDirections;
        aircraftFlightAgent.gapBetweenOptimumDirections = gapBetweenOptimumDirections;
        
        aircraftFlightAgent.flightPathNormalizer = flightPathNormalizer;
        flightPathNormalizer.aircraftAgents.Clear();
        flightPathNormalizer.aircraftAgents.Add(aircraftFlightAgent);
        
        aircraftFlightAgent.aircraftController = aircraftController;
        aircraftFlightAgent.pitchSlider = pitchSlider;
        aircraftFlightAgent.rollSlider = rollSlider;
        aircraftFlightAgent.throttleSlider = throttleSlider;
        
        AddDecisionRequester(transform);
    }
}