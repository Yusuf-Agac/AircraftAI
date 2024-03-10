using System.Linq;
using DefaultNamespace;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AircraftTakeOffAgent : Agent
{
    public AirportPositionNormalizer airportPositionNormalizer;

    private FixedController _aircraftController;
        
    void Start () 
    {
        _aircraftController = GetComponent<FixedController>();
    }
    
    public override void OnEpisodeBegin()
    {
        _aircraftController.RestoreAircraft();
        //_aircraftController.TurnOnEngines();
        airportPositionNormalizer.ResetPlanePosition();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        var normalizedPos = _aircraftController.m_wheels.wheelColliders.Any(wheel => wheel.isGrounded) ? 
                airportPositionNormalizer.NormalizedAircraftSafePosition : 
                airportPositionNormalizer.NormalizedAircraftPosition;
        sensor.AddObservation(normalizedPos);
        sensor.AddObservation(airportPositionNormalizer.NormalizedAircraftExitDirection);
        
        sensor.AddObservation(_aircraftController.m_rigidbody.velocity);
        sensor.AddObservation(_aircraftController.transform.rotation.eulerAngles);
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        _aircraftController.m_input.SetAgentInputs(actionBuffers);
        if (airportPositionNormalizer.NormalizedAircraftExitDirection.magnitude < 0.1f)
        {
            SetReward(1.0f);
            EndEpisode();
        }
        else if (transform.localPosition.y < 30)
        {
            EndEpisode();
        }
    }

}
