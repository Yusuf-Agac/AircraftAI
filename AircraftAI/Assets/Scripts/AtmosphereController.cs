using Oyedoyin.FixedWing;
using UnityEngine;

namespace DefaultNamespace
{
    public static class AtmosphereController
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
    }
}