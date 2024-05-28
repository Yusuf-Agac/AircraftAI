using System;
using Oyedoyin.Common;
using Oyedoyin.FixedWing;
using UnityEngine;

public static class AircraftNormalizer
{
    private const float MaxAxesRate = 40f;
    internal const float MaxSpeed = 150f;
    
    public static float NormalizedSpeed(Controller aircraftController)
    {
        return NormalizerHelper.ClampNP1(Speed(aircraftController) / MaxSpeed);
    }
    
    private static float Speed(Controller aircraftController)
    {
        var u = aircraftController.m_core.u;
        var v = aircraftController.m_core.v;
        return (float)Math.Sqrt((u * u) + (v * v)) * 1.944f;
    }
        
    public static float NormalizedThrust(Controller aircraftController)
    {
        return NormalizerHelper.ClampNP1(aircraftController.m_wowForce / 6500);
    }
    
    public static Vector3 NormalizedCurrentAxes(FixedController aircraftController)
    {
        var elevator = -aircraftController.m_wings[0].m_controlDeflection;
        var aileron = -aircraftController.m_wings[3].m_controlDeflection;
        var rudder = aircraftController.m_wings[2].m_controlDeflection;
        
        var elevatorLimit = aircraftController.m_wings[0].c_positiveLimit;
        var aileronLimit = aircraftController.m_wings[3].c_positiveLimit;
        var rudderLimit = aircraftController.m_wings[2].c_positiveLimit;
        
        var aileronNormalized = NormalizerHelper.ClampNP1(aileron / aileronLimit);
        var elevatorNormalized = NormalizerHelper.ClampNP1(elevator / elevatorLimit);
        var rudderNormalized = NormalizerHelper.ClampNP1(rudder / rudderLimit);
        return new Vector3(aileronNormalized, elevatorNormalized, rudderNormalized);
    }

    public static Vector3 NormalizedTargetAxes(FixedController aircraftController)
    {
        var pitch = aircraftController.m_input._pitchInput;
        var roll = aircraftController.m_input._rollInput;
        var yaw = aircraftController.m_input._yawInput;
        
        return new Vector3(pitch, roll, yaw);
    }

    public static Vector3 NormalizeAxesRates(FixedController aircraftController)
    {
        var normalizedPitchRate = NormalizerHelper.ClampNP1((float)(aircraftController.m_core.q * Mathf.Rad2Deg / MaxAxesRate));
        var normalizedRollRate = NormalizerHelper.ClampNP1((float)(aircraftController.m_core.p * Mathf.Rad2Deg / MaxAxesRate));
        var normalizedYawRate = NormalizerHelper.ClampNP1((float)(aircraftController.m_core.r * Mathf.Rad2Deg / MaxAxesRate));
        return new Vector3(normalizedPitchRate, normalizedRollRate, normalizedYawRate);
    }
    
    public static float NormalizedCurrentThrottle(FixedController aircraftController)
    {
        return aircraftController.m_input._throttleInput;
    }
}