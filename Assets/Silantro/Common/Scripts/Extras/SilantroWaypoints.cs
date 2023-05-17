using UnityEngine;
using System.Collections.Generic;

public class SilantroWaypoints : MonoBehaviour
{
    public Transform m_aircraft;
    public Transform[] m_points;
    List<Vector3> m_aircraft_path_points;
    public double timer, count = 1;
    public bool m_display;

    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (m_points.Length > 3 && m_display)
        {
            for (int i = 0; i < m_points.Length - 1; i++)
            {
                Vector3 pointA = m_points[i].position;
                Vector3 pointB = m_points[i + 1].position;
                Gizmos.DrawLine(pointA, pointB);
                Gizmos.DrawLine(m_points[0].position, m_points[m_points.Length - 1].position);
            }
        }

        Gizmos.color = Color.cyan;
        if (m_aircraft_path_points != null && m_aircraft_path_points.Count > 3)
        {
            for (int i = 0; i < m_aircraft_path_points.Count - 1; i++)
            {
                Vector3 pointA = m_aircraft_path_points[i];
                Vector3 pointB = m_aircraft_path_points[i + 1];
                Gizmos.DrawLine(pointA, pointB);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    private void FixedUpdate()
    {
        if (m_aircraft_path_points == null) { m_aircraft_path_points = new List<Vector3>(); }
        timer += Time.fixedDeltaTime;
        if (timer > count) { m_aircraft_path_points.Add(m_aircraft.transform.position); timer = 0; }
    }
}
