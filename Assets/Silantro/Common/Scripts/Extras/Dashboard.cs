using UnityEngine;

namespace Oyedoyin.Common
{
    public class Dashboard : MonoBehaviour
    {
        [Header("Vehicle")]
        public Controller m_vehicle;

        [Header("Dials")]
        public SilantroInstrument airspeed;
        public SilantroInstrument altimeter;
        public SilantroInstrument turnIndicator;
        public SilantroInstrument compass;
        public SilantroInstrument tachometer;
        public SilantroInstrument variometer;
        public SilantroInstrument fuel;


        private void FixedUpdate()
        {
            if (m_vehicle != null)
            {
                // Airspeed
                if (airspeed != null)
                {
                    airspeed.m_dial.m_values[0] = m_vehicle.m_core.Vkts;
                    airspeed.m_dial.UpdateGauge();
                }
                // Turn Indicator
                if (turnIndicator != null)
                {
                    turnIndicator.m_dial.m_values[0] = m_vehicle.m_core.ф;
                    turnIndicator.m_dial.UpdateGauge();
                }
                // Compass
                if (compass)
                {
                    compass.m_dial.m_values[0] = m_vehicle.m_rigidbody.transform.eulerAngles.y;
                    compass.m_dial.UpdateGauge();
                }
                // Climb Rate
                double m_climb = (m_vehicle.m_core.δz * 196.85) / 1000;
                if (variometer != null)
                {
                    variometer.m_dial.m_values[0] = m_climb;
                    variometer.m_dial.UpdateGauge();
                }
                // Altimeter
                double m_height = m_vehicle.m_core.m_height;
                double m_hu = (m_height % 1000.0f) / 100.0f;
                double m_th = (m_height % 10000.0f) / 1000.0f;
                double m_tt = (m_height % 100000.0f) / 10000.0f;
                if (altimeter != null)
                {
                    altimeter.m_dial.m_values[0] = m_th;
                    altimeter.m_dial.UpdateGauge();
                }
                // Fuel
                double m_fuelLevel = (m_vehicle.fuelLevel / m_vehicle.fuelCapacity) * 100;
                if (fuel != null)
                {
                    fuel.m_dial.m_values[0] = m_fuelLevel;
                    fuel.m_dial.UpdateGauge();
                }
                // Tachometer
                if (tachometer != null)
                {
                    tachometer.m_dial.m_values[0] = m_vehicle.m_powerLevel * 100;
                    tachometer.m_dial.UpdateGauge();
                }
            }
        }
    }
}