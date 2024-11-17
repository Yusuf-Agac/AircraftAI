using System;
using Oyedoyin.FixedWing;
using Unity.MLAgents;
using UnityEngine;

public abstract class AircraftAgent : Agent
{
    public FixedController aircraftController;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward.normalized * 40);
        Gizmos.color = Color.yellow;
        if(aircraftController.m_rigidbody) Gizmos.DrawLine(transform.position, transform.position + aircraftController.m_rigidbody.velocity.normalized * 40);
    }
}