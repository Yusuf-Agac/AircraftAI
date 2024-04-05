using Oyedoyin.FixedWing;
using UnityEngine;

namespace DefaultNamespace
{
    public static class AtmosphereController
    {
        public static void SmoothlyChangeWindAndTurbulence(FixedController fixedController, float maxWindSpeed, float maxTurbulence)
        {
            var windDir = (float)fixedController.m_core.m_atmosphere.m_ψw;
            var windSpeed = (float)fixedController.m_core.m_atmosphere.m_windSpeed;
            var turbulence = (float)fixedController.m_core.m_atmosphere.m_turbulence;
            
            windDir += Random.Range(-25f, 25f);
            while(windDir < 0) windDir += 360;
            fixedController.m_core.m_atmosphere.m_ψw = (windDir % 360);
            
            windSpeed += Random.Range(-1f * maxWindSpeed, 1f * maxWindSpeed);
            fixedController.m_core.m_atmosphere.m_windSpeed = Mathf.Clamp(windSpeed, 0, maxWindSpeed);
            
            turbulence += Random.Range(-1f * maxTurbulence, 1f * maxTurbulence);
            fixedController.m_core.m_atmosphere.m_turbulence = Mathf.Clamp(turbulence, 0, maxTurbulence);
        }
    }
}