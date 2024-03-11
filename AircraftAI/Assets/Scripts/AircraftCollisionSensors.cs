using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AircraftCollisionSensors : MonoBehaviour
{
    public List<Sensor> sensors;

    public bool CollisionSensor => sensors.Any(sensor => Physics.Raycast(sensor.transform.position, sensor.transform.forward, sensor.maxDistance, layerMask: LayerMask.GetMask("Terrain")));

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
            Gizmos.color = Physics.Raycast(sensor.transform.position, sensor.transform.forward, sensor.maxDistance, layerMask:LayerMask.GetMask("Terrain")) ? Color.red : Color.green;

            Gizmos.DrawRay(sensor.transform.position, sensor.transform.forward * sensor.maxDistance);
        }
    }
}
