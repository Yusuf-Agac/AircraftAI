using System;
using System.Linq;
using UnityEngine;

public class AircraftCollisionSensors : MonoBehaviour
{
    [Header("Configurations")] 
    public float observationMultiplier = 5;
    
    [Header("Sensors")]
    public Sensor[] sensors;

    public bool CollisionSensorCriticLevel => sensors.Any(sensor => Physics.Raycast(sensor.transform.position, sensor.transform.forward, sensor.maxDistance, layerMask: LayerMask.GetMask("Terrain")));

    public float[] CollisionSensorsNormalizedLevels()
    {
        var result = new float[sensors.Length];
        for (var i = 0; i < sensors.Length; i++)
        {
            var sensor = sensors[i];
            var casted = Physics.Raycast(sensor.transform.position, sensor.transform.forward, out var hit, sensor.maxDistance, layerMask: LayerMask.GetMask("Terrain"));
            result[i] = casted ? hit.distance / sensor.maxDistance * observationMultiplier : 1;
        }
        return result;
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
            Gizmos.color = Color.red;
            Gizmos.DrawRay(sensor.transform.position, sensor.transform.forward * sensor.maxDistance);
            
            Gizmos.color = Color.green;
            var pivot = sensor.transform.position + sensor.transform.forward * sensor.maxDistance;
            Gizmos.DrawRay(pivot, sensor.transform.forward * sensor.maxDistance * observationMultiplier);
        }
    }
}
