using System;
using System.Linq;
using UnityEngine;

public class AircraftCollisionSensors : MonoBehaviour
{
    [Header("Configurations")] 
    public float observationMultiplier = 5;
    
    [Header("Sensors")]
    public Sensor[] sensors;
    private float[] _result;

    public bool CollisionSensorCriticLevel => sensors.Any(sensor => Physics.Raycast(sensor.transform.position, sensor.transform.forward, sensor.maxDistance, layerMask: LayerMask.GetMask("Terrain")));

    private void Awake()
    {
        _result = new float[sensors.Length];
    }

    public float[] CollisionSensorsNormalizedLevels()
    {
        for (var i = 0; i < sensors.Length; i++)
        {
            var sensor = sensors[i];
            var casted = Physics.Raycast(sensor.transform.position, sensor.transform.forward, out var hit, sensor.maxDistance * observationMultiplier, layerMask: LayerMask.GetMask("Terrain"));
            _result[i] = casted ? hit.distance / (sensor.maxDistance * observationMultiplier) : 1;
        }
        return _result;
    }
    
    [Serializable]
    public class Sensor
    {
        public Transform transform;
        public float maxDistance;
    }

    public void OnDrawGizmos()
    {
        foreach (var sensor in sensors)
        {
            if (!Physics.Raycast(sensor.transform.position, sensor.transform.forward, out var hit,
                    sensor.maxDistance * observationMultiplier, layerMask: LayerMask.GetMask("Terrain")))
            {
                continue;
            }
                
            Gizmos.color = Color.red;
            Gizmos.DrawRay(sensor.transform.position, sensor.transform.forward * sensor.maxDistance);
            
            Gizmos.color = Color.green;
            var pivot = sensor.transform.position + sensor.transform.forward * sensor.maxDistance;
            Gizmos.DrawRay(pivot, sensor.transform.forward * sensor.maxDistance * observationMultiplier);
        }
    }
}
