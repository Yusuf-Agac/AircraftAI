using Oyedoyin.FixedWing;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;

public class AircraftFlightAgent : Agent
{
    [Range(0.1f, 25f)] public float manoeuvreSpeed = 10f;
    public float maxWindSpeed = 5;
    public float maxTurbulence = 5;
    public int numOfOptimumDirections = 2;
    public float gapBetweenOptimumDirections = 25f;
    [Space(10)]
    public FlightPathNormalizer flightPathNormalizer;
    public FixedController aircraftController;
    [Space(10)]
    public Slider PitchSlider;
    public Slider RollSlider;
    public Slider ThrottleSlider;
    
    private float[] _previousActions = new float[3] {0, 0, 0};
    private float _sparseRewards;
    private float _denseRewards;
    private float _optimalRewards;
    private float _actionPenalty;
    private void Start () 
    {
        aircraftController = GetComponent<FixedController>();
    }
    
    public override void OnEpisodeBegin()
    {
        aircraftController.RestoreAircraft();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = PitchSlider.value;
        continuousActionsOut[1] = RollSlider.value;
        continuousActionsOut[2] = ThrottleSlider.value;
        aircraftController.m_input.SetAgentInputs(actionsOut, manoeuvreSpeed);
    }
}
