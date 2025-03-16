using System;
using System.Linq;
using UnityEngine;

public partial class AircraftCollisionDetector : MonoBehaviour
{
    [Header("Configurations    General----------------------------------------------------------------------------------------------"), Space(10)]
    public float observationMultiplier = 5;
    public AircraftCollisionSensor[] sensors;
    
    private float[] _sensorData;
    
    private void Awake() => _sensorData = new float[sensors.Length];

    public bool IsThereBadSensorData()
    {
        return sensors.Any(sensor => Physics.Raycast(sensor.transform.position, sensor.transform.forward, sensor.maxDistance, layerMask: LayerMask.GetMask("Terrain")));
    }
    
    public float[] GetSensorData()
    {
        for (var i = 0; i < sensors.Length; i++)
        {
            var sensor = sensors[i];
            var casted = Physics.Raycast(sensor.transform.position, sensor.transform.forward, out var hit, sensor.maxDistance * observationMultiplier, layerMask: LayerMask.GetMask("Terrain"));
            _sensorData[i] = casted ? hit.distance / (sensor.maxDistance * observationMultiplier) : 1;
        }
        return _sensorData;
    }
}

[Serializable]
public class AircraftCollisionSensor
{
    public Transform transform;
    public float maxDistance;
}