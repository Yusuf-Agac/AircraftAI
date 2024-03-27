using System;
using Oyedoyin.Common;
using UnityEngine;

namespace DefaultNamespace
{
    public static class AircraftNormalizer
    {
        public static float NormalizedSpeed(Controller aircraftController)
        {
            var u = aircraftController.m_core.u;
            var v = aircraftController.m_core.v;
            var speed = (float)Math.Sqrt((u * u) + (v * v)) * 1.944f;
            return Mathf.Clamp01(speed / 110f);
        }
    }
}