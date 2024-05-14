using System;
using DefaultNamespace;
using Oyedoyin.Common;
using Oyedoyin.FixedWing;
using UnityEngine;

public static class AircraftNormalizer
{
    public static float NormalizedSpeed(Controller aircraftController)
    {
        var u = aircraftController.m_core.u;
        var v = aircraftController.m_core.v;
        var speed = (float)Math.Sqrt((u * u) + (v * v)) * 1.944f;
        return NormalizerUtility.ClampNP1(speed / 150f);
    }
        
    public static float NormalizedThrust(Controller aircraftController)
    {
        return NormalizerUtility.ClampNP1(aircraftController.m_wowForce / 6500);
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