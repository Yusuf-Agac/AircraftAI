using UnityEngine;

public partial class AircraftCollisionDetector
{
    public void OnDrawGizmos()
    {
        foreach (var sensor in sensors)
        {
            if (!Physics.Raycast(sensor.transform.position, sensor.transform.forward, out _, sensor.maxDistance * observationMultiplier, layerMask: LayerMask.GetMask("Terrain")))
                continue;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(sensor.transform.position, sensor.transform.forward * sensor.maxDistance);

            Gizmos.color = Color.green;
            var pivot = sensor.transform.position + sensor.transform.forward * sensor.maxDistance;
            Gizmos.DrawRay(pivot, sensor.transform.forward * sensor.maxDistance * observationMultiplier);
        }
    }
}