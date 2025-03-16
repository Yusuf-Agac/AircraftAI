using Oyedoyin.FixedWing;
using UnityEngine;

public static class AtmosphereUtility
{
    public static void SmoothlyChangeWindAndTurbulence(FixedController fixedController, AtmosphereData atmosphereData, int decisionPeriod)
    {
        var windDirection = (float)fixedController.m_core.m_atmosphere.m_ψw;
        var windSpeed = (float)fixedController.m_core.m_atmosphere.m_windSpeed;
        var turbulence = (float)fixedController.m_core.m_atmosphere.m_turbulence;
            
        windDirection += Random.Range(-1f, 1f) * (decisionPeriod / 25f) * atmosphereData.maxWindDirectionChangeSpeed;
        while(windDirection < 0) windDirection += 360;
        fixedController.m_core.m_atmosphere.m_ψw = (windDirection % 360);
            
        windSpeed += Random.Range(-2f * atmosphereData.maxWindSpeed, 2f * atmosphereData.maxWindSpeed) * (decisionPeriod / 25f);
        fixedController.m_core.m_atmosphere.m_windSpeed = Mathf.Clamp(windSpeed, 0, atmosphereData.maxWindSpeed);
            
        turbulence += Random.Range(-2f * atmosphereData.maxTurbulence, 2f * atmosphereData.maxTurbulence) * (decisionPeriod / 25f);
        fixedController.m_core.m_atmosphere.m_turbulence = Mathf.Clamp(turbulence, 0, atmosphereData.maxTurbulence);
    }
        
    public static float[] NormalizedWind(FixedController fixedController, AtmosphereData trainingAtmosphereData)
    {
        var normalizedWindDirection = (float)fixedController.m_core.m_atmosphere.m_ψw - fixedController.transform.eulerAngles.y + 180;
        while (normalizedWindDirection < 0) normalizedWindDirection += 360;
        normalizedWindDirection = (normalizedWindDirection % 360) / 360;

        var normalizedTurbulence = (float)fixedController.m_core.m_atmosphere.m_turbulence / trainingAtmosphereData.maxTurbulence;
        var normalizedWindSpeed = (float)fixedController.m_core.m_atmosphere.m_windSpeed / trainingAtmosphereData.maxWindSpeed;
            
        return new[] {normalizedWindDirection, normalizedWindSpeed, normalizedTurbulence};
    }
}