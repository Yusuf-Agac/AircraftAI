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
    
    public override void SetBehaviorComponent(Transform transform)
    {
        AddBehaviorComponent(transform);
        
        var aircraftTakeOffAgent = transform.AddComponent<AircraftTakeOffAgent>();
        Agent = aircraftTakeOffAgent;
        
        aircraftTakeOffAgent.MaxStep = maxStep;

        aircraftTakeOffAgent.trainingMode = false;
        aircraftTakeOffAgent.manoeuvreSpeed = manoeuvreSpeed;
        aircraftTakeOffAgent.windDirectionSpeed = windDirectionSpeed;
        aircraftTakeOffAgent.maxWindSpeed = maxWindSpeed;
        aircraftTakeOffAgent.maxTurbulence = maxTurbulence;
        aircraftTakeOffAgent.numOfOptimalDirections = numOfOptimumDirections;
        aircraftTakeOffAgent.gapBetweenOptimalDirections = gapBetweenOptimumDirections;
        
        aircraftTakeOffAgent.airportNormalizer = airportNormalizer;
        airportNormalizer.aircraftTakeOffAgents.Clear();
        airportNormalizer.aircraftTakeOffAgents.Add(aircraftTakeOffAgent);
        
        aircraftTakeOffAgent.sensors = sensors;
        aircraftTakeOffAgent.observationCanvas = observationCanvas;
        aircraftTakeOffAgent.rewardCanvas = rewardCanvas;
        aircraftTakeOffAgent.aircraftController = aircraftController;
        
        AddDecisionRequester(transform);
    }
}