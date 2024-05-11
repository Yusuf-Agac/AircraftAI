using System;
using Oyedoyin.FixedWing;
using Unity.MLAgents.Policies;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
class TakeOffConfig : BehaviorConfig
{
    [Space(10)]
    [SerializeField] private AirportNormalizer airportNormalizer;
    [SerializeField] private AircraftCollisionSensors sensors;
    [SerializeField] private AircraftRelativeTransformCanvas relativeTransformCanvas;
    [SerializeField] private FixedController aircraftController;
    [SerializeField] private Slider pitchSlider;
    [SerializeField] private Slider rollSlider;
    [SerializeField] private Slider throttleSlider;
    
    public override void SetBehaviorComponent(Transform transform)
    {
        AddBehaviorComponent(transform);
        
        AircraftTakeOffAgent aircraftTakeOffAgent = transform.AddComponent<AircraftTakeOffAgent>();
        Agent = aircraftTakeOffAgent;
        
        aircraftTakeOffAgent.MaxStep = maxStep;

        aircraftTakeOffAgent.trainingMode = false;
        aircraftTakeOffAgent.manoeuvreSpeed = manoeuvreSpeed;
        aircraftTakeOffAgent.maxWindSpeed = maxWindSpeed;
        aircraftTakeOffAgent.maxTurbulence = maxTurbulence;
        aircraftTakeOffAgent.numOfOptimumDirections = numOfOptimumDirections;
        aircraftTakeOffAgent.gapBetweenOptimumDirections = gapBetweenOptimumDirections;
        
        aircraftTakeOffAgent.airportNormalizer = airportNormalizer;
        airportNormalizer.aircraftAgents.Clear();
        airportNormalizer.aircraftAgents.Add(aircraftTakeOffAgent);
        
        aircraftTakeOffAgent.sensors = sensors;
        aircraftTakeOffAgent.relativeTransformCanvas = relativeTransformCanvas;
        aircraftTakeOffAgent.aircraftController = aircraftController;
        aircraftTakeOffAgent.PitchSlider = pitchSlider;
        aircraftTakeOffAgent.RollSlider = rollSlider;
        aircraftTakeOffAgent.ThrottleSlider = throttleSlider;
        
        AddDecisionRequester(transform);
    }
}