using System;
using Oyedoyin.FixedWing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
class FlightConfig : BehaviorConfig
{
    [Space(10)]
    [SerializeField] private FlightPathNormalizer flightPathNormalizer;
    
    public override void SetBehaviorComponent(Transform transform)
    {
        AddBehaviorComponent(transform);
        
        var aircraftFlightAgent = transform.AddComponent<AircraftFlightAgent>();
        Agent = aircraftFlightAgent;
        
        aircraftFlightAgent.MaxStep = maxStep;
        
        aircraftFlightAgent.trainingMode = false;
        aircraftFlightAgent.manoeuvreSpeed = manoeuvreSpeed;
        aircraftFlightAgent.windDirectionSpeed = windDirectionSpeed;
        aircraftFlightAgent.maxWindSpeed = maxWindSpeed;
        aircraftFlightAgent.maxTurbulence = maxTurbulence;
        aircraftFlightAgent.numOfOptimalDirections = numOfOptimumDirections;
        aircraftFlightAgent.gapBetweenOptimalDirections = gapBetweenOptimumDirections;
        
        aircraftFlightAgent.flightPathNormalizer = flightPathNormalizer;
        flightPathNormalizer.aircraftAgents.Clear();
        flightPathNormalizer.aircraftAgents.Add(aircraftFlightAgent);
        
        aircraftFlightAgent.observationCanvas = observationCanvas;
        aircraftFlightAgent.rewardCanvas = rewardCanvas;
        aircraftFlightAgent.aircraftController = aircraftController;
        
        AddDecisionRequester(transform);
    }
}