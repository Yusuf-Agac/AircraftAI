using System;
using Oyedoyin.FixedWing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
class LandingConfig : BehaviorConfig
{
    [Space(10)]
    [SerializeField] private AirportNormalizer airportNormalizer;
    [SerializeField] private AircraftCollisionSensors sensors;
    
    public override void SetBehaviorComponent(Transform transform)
    {
        AddBehaviorComponent(transform);
        
        var aircraftLandingAgent = transform.AddComponent<AircraftLandingAgent>();
        Agent = aircraftLandingAgent;
        
        aircraftLandingAgent.MaxStep = maxStep;

        aircraftLandingAgent.trainingMode = false;
        aircraftLandingAgent.manoeuvreSpeed = manoeuvreSpeed;
        aircraftLandingAgent.windDirectionSpeed = windDirectionSpeed;
        aircraftLandingAgent.maxWindSpeed = maxWindSpeed;
        aircraftLandingAgent.maxTurbulence = maxTurbulence;
        aircraftLandingAgent.numOfOptimalDirections = numOfOptimumDirections;
        aircraftLandingAgent.gapBetweenOptimalDirections = gapBetweenOptimumDirections;
        
        aircraftLandingAgent.airportNormalizer = airportNormalizer;
        airportNormalizer.aircraftAgents.Clear();
        airportNormalizer.aircraftAgents.Add(aircraftLandingAgent);
        
        aircraftLandingAgent.sensors = sensors;
        aircraftLandingAgent.observationCanvas = observationCanvas;
        aircraftLandingAgent.rewardCanvas = rewardCanvas;
        aircraftLandingAgent.aircraftController = aircraftController;
        aircraftLandingAgent.windArrows = windArrows;
        
        AddDecisionRequester(transform);
    }
}