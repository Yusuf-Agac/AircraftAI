using System;
using Oyedoyin.Common;
using Oyedoyin.FixedWing;
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

        public static float[] NormalizedWind(FixedController fixedController, float maxWind, float maxTurbulence)
        {
            var normalizedWindDir = (float)fixedController.m_core.m_atmosphere.m_ψw - fixedController.transform.eulerAngles.y + 180;
            while (normalizedWindDir < 0) normalizedWindDir += 360;
            normalizedWindDir = (normalizedWindDir % 360) / 360;

            var normalizedTurbulence = (float)fixedController.m_core.m_atmosphere.m_turbulence / maxTurbulence;
            var normalizedWindSpeed = (float)fixedController.m_core.m_atmosphere.m_windSpeed / maxWind;
            
            return new[] {normalizedWindDir, normalizedWindSpeed, normalizedTurbulence};
        }
    }
}