using Oyedoyin.FixedWing;
using UnityEngine;

public static class AtmosphereUtility
{
    public static void SmoothlyChangeWindAndTurbulence(FixedController fixedController, float maxWindSpeed, float maxTurbulence, int decisionPeriod, float windDirectionSpeed)
    {
        var windDir = (float)fixedController.m_core.m_atmosphere.m_ψw;
        var windSpeed = (float)fixedController.m_core.m_atmosphere.m_windSpeed;
        var turbulence = (float)fixedController.m_core.m_atmosphere.m_turbulence;
            
        windDir += Random.Range(-1f, 1f) * (decisionPeriod / 25f) * windDirectionSpeed;
        while(windDir < 0) windDir += 360;
        fixedController.m_core.m_atmosphere.m_ψw = (windDir % 360);
            
        windSpeed += Random.Range(-2f * maxWindSpeed, 2f * maxWindSpeed) * (decisionPeriod / 25f);
        fixedController.m_core.m_atmosphere.m_windSpeed = Mathf.Clamp(windSpeed, 0, maxWindSpeed);
            
        turbulence += Random.Range(-2f * maxTurbulence, 2f * maxTurbulence) * (decisionPeriod / 25f);
        fixedController.m_core.m_atmosphere.m_turbulence = Mathf.Clamp(turbulence, 0, maxTurbulence);
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