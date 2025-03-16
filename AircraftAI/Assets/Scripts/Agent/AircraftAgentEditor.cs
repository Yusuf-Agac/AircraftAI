using UnityEngine;

public abstract partial class AircraftAgent
{
    private void OnDrawGizmos()
    {
        if (!aircraftController.IsEngineWorks) return;

        Gizmos.color = Color.red;
        const int rayLength = 40;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward.normalized * rayLength);
        Gizmos.color = Color.yellow;
        if (aircraftController.m_rigidbody) Gizmos.DrawLine(transform.position, transform.position + aircraftController.m_rigidbody.velocity.normalized * rayLength);
    }
}