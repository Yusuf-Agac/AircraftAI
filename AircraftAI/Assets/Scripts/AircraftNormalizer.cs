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
    
    public static Vector3 NormalizedDeflections(FixedController aircraftController)
    {
        var elevator = -aircraftController.m_wings[0].m_controlDeflection;
        var aileron = -aircraftController.m_wings[3].m_controlDeflection;
        var rudder = aircraftController.m_wings[2].m_controlDeflection;
        
        var elevatorLimit = aircraftController.m_wings[0].c_positiveLimit;
        var aileronLimit = aircraftController.m_wings[3].c_positiveLimit;
        var rudderLimit = aircraftController.m_wings[2].c_positiveLimit;
        
        var aileronNormalized = NormalizerUtility.ClampNP1(aileron / aileronLimit);
        var elevatorNormalized = NormalizerUtility.ClampNP1(elevator / elevatorLimit);
        var rudderNormalized = NormalizerUtility.ClampNP1(rudder / rudderLimit);
        return new Vector3(aileronNormalized, elevatorNormalized, rudderNormalized);
    }

    public static Vector3 NormalizedTargetDeflections(FixedController aircraftController)
    {
        var pitch = aircraftController.m_input._pitchInput;
        var roll = aircraftController.m_input._rollInput;
        var yaw = aircraftController.m_input._yawInput;
        
        return new Vector3(pitch, roll, yaw);
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