#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Oyedoyin.Common
{

    /// <summary>
    /// 
    /// </summary>
    public class SilantroMisc : MonoBehaviour
    {
        public enum Function { ImpactSound, CaseSound, SystemReset, CleanUp, Transponder, DataDisplay }
        public enum Cleanup { Active, Off }

        public Function m_function = Function.ImpactSound;
        public Cleanup m_cleanState = Cleanup.Off;
        public AudioClip[] sounds;
        private AudioSource audioOut;
        public Display m_display;

        public float soundRange = 300f;
        public float soundVolume = 0.4f;
        public int soundCount = 1;
        public float destroyTime = 5;

        // Transponder Properties
        public enum SilantroTag
        {
            Truck, Airport, Undefined, SAMBattery, Tank//Add more if you wish
        }
        [HideInInspector] public SilantroTag silantroTag = SilantroTag.Undefined;
        [HideInInspector] public Texture2D silantroTexture;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            if (m_function != Function.Transponder && m_function != Function.DataDisplay)
            {
                audioOut = gameObject.AddComponent<AudioSource>();
                audioOut.dopplerLevel = 0f;
                audioOut.spatialBlend = 1f;
                audioOut.rolloffMode = AudioRolloffMode.Custom;
                audioOut.maxDistance = soundRange;
                audioOut.volume = soundVolume;
                if (m_function == Function.ImpactSound) { audioOut.PlayOneShot(sounds[UnityEngine.Random.Range(0, sounds.Length)]); }
                if (m_cleanState == Cleanup.Active) { Destroy(gameObject, destroyTime); }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        private void OnCollisionEnter(Collision col)
        {
            if (m_function == Function.CaseSound)
            {
                if (col.collider.CompareTag("Ground"))
                {
                    if (audioOut && !audioOut.isPlaying)
                    {
                        audioOut.PlayOneShot(sounds[UnityEngine.Random.Range(0, sounds.Length)]);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void FixedUpdate()
        {
            if (m_function == Function.DataDisplay) { m_display.Compute(); }
        }

        #region Scene Autopilot

        private int m_altitudeStep;
        /// <summary>
        /// 
        /// </summary>
        public void ToggleAutopilot()
        {
            if (m_display.m_vehicle.m_sceneAutopilot.m_state == Oyedoyin.Common.SceneAutopilot.State.Inactive) { m_display.m_vehicle.m_sceneAutopilot.EnableAutopilot(); }
            else { m_display.m_vehicle.m_sceneAutopilot.DisableAutopilot(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public void IncreaseSpeed() { m_display.m_vehicle.m_sceneAutopilot.m_presetSpeed += 10f; }
        public void DecreaseSpeed() { m_display.m_vehicle.m_sceneAutopilot.m_presetSpeed -= 10f; if (m_display.m_vehicle.m_sceneAutopilot.m_presetSpeed <= 0) { m_display.m_vehicle.m_sceneAutopilot.m_presetSpeed = 0f; } }
        /// <summary>
        /// 
        /// </summary>
        public void IncreaseAltitude()
        {
            m_altitudeStep++;
            if (m_altitudeStep <= 20)
            {
                if (m_altitudeStep <= 10) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = m_altitudeStep * 10; }
                if (m_altitudeStep == 11) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 150; }
                if (m_altitudeStep == 12) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 200; }
                if (m_altitudeStep == 13) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 250; }
                if (m_altitudeStep == 14) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 300; }
                if (m_altitudeStep == 15) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 350; }
                if (m_altitudeStep == 16) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 400; }
                if (m_altitudeStep == 17) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 450; }
                if (m_altitudeStep == 18) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 500; }
                if (m_altitudeStep == 19) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 550; }
                if (m_altitudeStep == 20) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 600; }
            }
            else { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude += 100f; }
        }
        public void DecreaseAltitude()
        {
            m_altitudeStep--;
            if (m_altitudeStep <= 20)
            {
                if (m_altitudeStep <= 0) { m_altitudeStep = 0; }
                if (m_altitudeStep <= 10) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = m_altitudeStep * 10; }
                if (m_altitudeStep == 11) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 150; }
                if (m_altitudeStep == 12) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 200; }
                if (m_altitudeStep == 13) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 250; }
                if (m_altitudeStep == 14) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 300; }
                if (m_altitudeStep == 15) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 350; }
                if (m_altitudeStep == 16) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 400; }
                if (m_altitudeStep == 17) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 450; }
                if (m_altitudeStep == 18) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 500; }
                if (m_altitudeStep == 19) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 550; }
                if (m_altitudeStep == 20) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 600; }
            }
            else { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude -= 100f; }

            // Limit
            if (m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude <= 0f) { m_display.m_vehicle.m_sceneAutopilot.m_presetAltitude = 0f; }
        }
        /// <summary>
        /// 
        /// </summary>
        public void IncreaseClimb() { m_display.m_vehicle.m_sceneAutopilot.m_presetClimb += 100f; }
        public void DecreaseClimb() { m_display.m_vehicle.m_sceneAutopilot.m_presetClimb -= 100f; if (m_display.m_vehicle.m_sceneAutopilot.m_presetClimb <= 0f) { m_display.m_vehicle.m_sceneAutopilot.m_presetClimb = 0f; } }
        /// <summary>
        /// 
        /// </summary>
        public void IncreaseHeading() { m_display.m_vehicle.m_sceneAutopilot.m_presetHeading += 1f; if (m_display.m_vehicle.m_sceneAutopilot.m_presetHeading > 360f) { m_display.m_vehicle.m_sceneAutopilot.m_presetHeading = 0f; } }
        public void DecreaseHeading() { m_display.m_vehicle.m_sceneAutopilot.m_presetHeading -= 1f; if (m_display.m_vehicle.m_sceneAutopilot.m_presetHeading < 0f) { m_display.m_vehicle.m_sceneAutopilot.m_presetHeading = 360f; } }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Display
    {
        public enum ModelType { Realistic, Arcade }
        public ModelType m_type = ModelType.Realistic;

        /// <summary>
        /// 
        /// </summary>
        public enum UnitsSetup
        {
            Metric,
            Imperial,
            Custom
        }
        public UnitsSetup units = UnitsSetup.Metric;
        /// <summary>
        /// 
        /// </summary>
        public enum SpeedUnit
        {
            MeterPerSecond,
            Knots,
            FeetPerSecond,
            MilesPerHour,
            KilometerPerHour,
        }
        public SpeedUnit speedUnit = SpeedUnit.MeterPerSecond;
        /// <summary>
        /// 
        /// </summary>
        public enum AltitudeUnit
        {
            Meter,
            Feet,
            NauticalMiles,
            Kilometer
        }
        public AltitudeUnit altitudeUnit = AltitudeUnit.Meter;
        /// <summary>
        /// 
        /// </summary>
        public enum TemperatureUnit
        {
            Celsius,
            Fahrenheit
        }
        public TemperatureUnit temperatureUnit = TemperatureUnit.Celsius;
        /// <summary>
        /// 
        /// </summary>
        public enum WeightUnit
        {
            Tonne,
            Pound,
            Ounce,
            Stone,
            Kilogram
        }
        public WeightUnit weightUnit = WeightUnit.Kilogram;
        /// <summary>
        /// 
        /// </summary>
        public enum ForceUnit
        {
            Newton,
            KilogramForce,
            PoundForce
        }
        public ForceUnit forceUnit = ForceUnit.Newton;
        /// <summary>
        /// 
        /// </summary>
        public enum TorqueUnit
        {
            NewtonMeter,
            PoundForceFeet
        }
        public TorqueUnit torqueUnit = TorqueUnit.NewtonMeter;

        public Oyedoyin.Common.Controller m_vehicle;
#if SILANTRO_ROTARY
    public Oyedoyin.RotaryWing.LowFidelity.Controller m_lowfi;
#endif

        public Text speed;
        public Text altitude;
        public Text fuel;
        public Text weight;
        public Text brake;
        public Text density;
        public Text temperature;
        public Text pressure;
        public Text enginePower;
        public Text propellerPower;
        public Text gearState;
        public Text Time;
        public Text collective;
        public Text engineName;
        public Text currentWeapon;
        public Text weaponCount;
        public Text ammoCount;

        public Text flapLevel;
        public Text slatLevel;

        public Text heading;
        public Text gload;
        public Text pitchRate;
        public Text rollRate;
        public Text yawRate;
        public Text turnRate;
        public Text climb;

        public Text speedLabel;
        public Text altitudeLabel;
        public Text headingLabel;
        public Text climbLabel;
        public GameObject m_container;

        public bool displayPoints;

        /// <summary>
        /// 
        /// </summary>
        public void Compute()
        {
            if (m_type == ModelType.Realistic)
            {
                if (m_vehicle != null && m_vehicle.isActiveAndEnabled && speed != null)
                {
                    //----------------------------------------------AUTOPILOT
                    if (speedLabel) { speedLabel.text = m_vehicle.m_sceneAutopilot.m_presetSpeed.ToString(); }
                    if (altitudeLabel) { altitudeLabel.text = m_vehicle.m_sceneAutopilot.m_presetAltitude.ToString(); }
                    if (headingLabel) { headingLabel.text = m_vehicle.m_sceneAutopilot.m_presetHeading.ToString(); }
                    if (climbLabel) { climbLabel.text = m_vehicle.m_sceneAutopilot.m_presetClimb.ToString(); }

                    if (m_vehicle.m_sceneAutopilot.m_state == Oyedoyin.Common.SceneAutopilot.State.Active)
                    {
                        if (m_container != null && !m_container.activeSelf) { m_container.SetActive(true); }
                    }
                    else
                    {
                        if (m_container != null && m_container.activeSelf) { m_container.SetActive(false); }
                    }


                    if (m_vehicle != null)
                    {
                        if (engineName != null)
                        {
                            engineName.text = m_vehicle.transform.name;
                        }
                    }
                    //PARKING BRAKE
                    if (m_vehicle.m_wheels != null)
                    {
                        brake.text = m_vehicle.m_wheels.brakeState.ToString();
                        gearState.text = "Brake Lever: " + 0 + " %";
                    }


                    if (m_vehicle.m_type == Oyedoyin.Common.Controller.VehicleType.Aircraft)
                    {
#if SILANTRO_FIXED
                        Oyedoyin.FixedWing.FixedController m_aircraft = (Oyedoyin.FixedWing.FixedController)m_vehicle;
                        if (m_aircraft != null)
                        {
                            if (flapLevel != null) { flapLevel.text = "Flaps = " + (m_aircraft.m_flapDeflection).ToString("0.0") + " °"; }
                            if (slatLevel != null) { slatLevel.text = "Slat  = " + (m_aircraft.m_slatDeflection).ToString("0.0") + " °"; }
                        }
#endif
                    }
                    else
                    {
                        if (flapLevel.gameObject.activeSelf == true) { flapLevel.gameObject.SetActive(false); }
                        if (slatLevel.gameObject.activeSelf == true) { slatLevel.gameObject.SetActive(false); }
                    }

                    double gfore = m_vehicle.m_core.n;
                    if (gload != null)
                    {
                        if (m_vehicle.m_type == Oyedoyin.Common.Controller.VehicleType.Helicopter)
                        {
                            if (gfore > 4f) { gload.color = Color.red; } else if (gfore < -1f) { gload.color = Color.yellow; } else { gload.color = Color.white; }
                        }
                        if (m_vehicle.m_type == Oyedoyin.Common.Controller.VehicleType.Aircraft)
                        {
                            if (gfore > 9f) { gload.color = Color.red; } else if (gfore < -4f) { gload.color = Color.yellow; } else { gload.color = Color.white; }
                        }
                        gload.text = "G-Load = " + m_vehicle.m_core.n.ToString("0.00");
                    }
                    climb.text = "Climb = " + (m_vehicle.m_core.δz * Oyedoyin.Mathematics.Constants.toFtMin).ToString("0.0") + " ft/min";

                    if (heading != null)
                    {
                        heading.text = "Heading = " + (m_vehicle.transform.eulerAngles.y).ToString("0.0") + " °";
                    }

                    //WEIGHT SETTINGS
                    float Weight = m_vehicle.currentWeight;
                    if (weightUnit == WeightUnit.Kilogram)
                    {
                        weight.text = "Weight = " + Weight.ToString("0.0") + " kg";
                    }
                    if (weightUnit == WeightUnit.Tonne)
                    {
                        float tonneWeight = Weight * 0.001f;
                        weight.text = "Weight = " + tonneWeight.ToString("0.00") + " T";
                    }
                    if (weightUnit == WeightUnit.Pound)
                    {
                        float poundWeight = Weight * 2.20462f;
                        weight.text = "Weight = " + poundWeight.ToString("0.0") + " lb";
                    }
                    if (weightUnit == WeightUnit.Ounce)
                    {
                        float ounceWeight = Weight * 35.274f;
                        weight.text = "Weight = " + ounceWeight.ToString("0.0") + " Oz";
                    }
                    if (weightUnit == WeightUnit.Stone)
                    {
                        float stonneWeight = Weight * 0.15747f;
                        weight.text = "Weight = " + stonneWeight.ToString("0.0") + " St";
                    }
                    //FUEL
                    float Fuel = m_vehicle.fuelLevel;
                    if (weightUnit == WeightUnit.Kilogram)
                    {
                        fuel.text = "Fuel = " + Fuel.ToString("0.0") + " kg";
                    }
                    if (weightUnit == WeightUnit.Tonne)
                    {
                        float tonneWeight = Fuel * 0.001f;
                        fuel.text = "Fuel = " + tonneWeight.ToString("0.00") + " T";
                    }
                    if (weightUnit == WeightUnit.Pound)
                    {
                        float poundWeight = Fuel * 2.20462f;
                        fuel.text = "Fuel = " + poundWeight.ToString("0.0") + " lb";
                    }
                    if (weightUnit == WeightUnit.Ounce)
                    {
                        float ounceWeight = Fuel * 35.274f;
                        fuel.text = "Fuel = " + ounceWeight.ToString("0.0") + " Oz";
                    }
                    if (weightUnit == WeightUnit.Stone)
                    {
                        float stonneWeight = Fuel * 0.15747f;
                        fuel.text = "Fuel = " + stonneWeight.ToString("0.0") + " St";
                    }
                    //SPEED
                    if (m_vehicle.m_core != null)
                    {
                        double u = m_vehicle.m_core.u;
                        double v = m_vehicle.m_core.v;
                        float Speed = (float)Math.Sqrt((u * u) + (v * v));

                        if (speedUnit == SpeedUnit.Knots)
                        {
                            float speedly = Speed * 1.944f;
                            speed.text = "Airspeed = " + speedly.ToString("0.0") + " knots";
                        }
                        if (speedUnit == SpeedUnit.MeterPerSecond)
                        {
                            float speedly = Speed;
                            speed.text = "Airspeed = " + speedly.ToString("0.0") + " m/s";
                        }
                        if (speedUnit == SpeedUnit.FeetPerSecond)
                        {
                            float speedly = Speed * 3.2808f;
                            speed.text = "Airspeed = " + speedly.ToString("0.0") + " ft/s";
                        }
                        if (speedUnit == SpeedUnit.MilesPerHour)
                        {
                            float speedly = Speed * 2.237f;
                            speed.text = "Airspeed = " + speedly.ToString("0.0") + " mph";
                        }
                        if (speedUnit == SpeedUnit.KilometerPerHour)
                        {
                            float speedly = Speed * 3.6f;
                            speed.text = "Airspeed = " + speedly.ToString("0.0") + " kmh";
                        }
                    }
                    //ENGINE POWER
                    enginePower.text = "Engine Throttle = " + (m_vehicle._throttleInput * 100f).ToString("0.0") + " %";

                    if (m_vehicle.m_type == Oyedoyin.Common.Controller.VehicleType.Helicopter)
                    {
                        collective.text = "Collective = " + (m_vehicle._collectiveInput * 100f).ToString("0.0") + " %";
                    }
                    else
                    {
                        collective.text = "Engine Thrust = " + (m_vehicle.m_wowForce).ToString("0.0") + " N";
                    }


                    //ALTITUDE
                    if (m_vehicle.m_core != null)
                    {
                        float Altitude = (float)(m_vehicle.m_core.z * Oyedoyin.Mathematics.Constants.m2ft);
                        if (altitudeUnit == AltitudeUnit.Feet)
                        {
                            float distance = Altitude;
                            altitude.text = "Altitude = " + distance.ToString("0.0") + " ft";
                        }
                        if (altitudeUnit == AltitudeUnit.NauticalMiles)
                        {
                            float distance = Altitude * 0.00054f;
                            altitude.text = "Altitude = " + distance.ToString("0.0") + " NM";
                        }
                        if (altitudeUnit == AltitudeUnit.Kilometer)
                        {
                            float distance = Altitude / 3280.8f;
                            altitude.text = "Altitude = " + distance.ToString("0.0") + " km";
                        }
                        if (altitudeUnit == AltitudeUnit.Meter)
                        {
                            float distance = Altitude / 3.2808f;
                            altitude.text = "Altitude = " + distance.ToString("0.0") + " m";
                        }


                        //AMBIENT
                        pressure.text = "Pressure = " + m_vehicle.m_core.m_atmosphere.Ps.ToString("0.0") + " kpa";
                        density.text = "Air Density = " + m_vehicle.m_core.m_atmosphere.ρ.ToString("0.000") + " kg/m3";

                        float Temperature = (float)m_vehicle.m_core.m_atmosphere.T;
                        if (temperatureUnit == TemperatureUnit.Celsius)
                        {
                            temperature.text = "Temperature = " + Temperature.ToString("0.0") + " °C";
                        }
                        if (temperatureUnit == TemperatureUnit.Fahrenheit)
                        {
                            float temp = (Temperature * (9 / 5)) + 32f;
                            temperature.text = "Temperature = " + temp.ToString("0.0") + " °F";
                        }
                    }


                    if (m_vehicle.m_wheels == null)
                    {
                        if (gearState.gameObject.activeSelf == true) { gearState.gameObject.SetActive(false); }
                        if (brake.gameObject.activeSelf == true) { brake.gameObject.SetActive(false); }
                    }
                    else
                    {
                        if (gearState.gameObject.activeSelf == false) { gearState.gameObject.SetActive(true); }
                        if (brake.gameObject.activeSelf == false) { brake.gameObject.SetActive(true); }
                    }

                    if (pitchRate != null && pitchRate.gameObject.activeSelf)
                    {
                        turnRate.text = m_vehicle.m_core.ωф.ToString("0.0") + " °/s";
                        pitchRate.text = (m_vehicle.m_core.q * Mathf.Rad2Deg).ToString("0.0") + " °/s";
                        rollRate.text = (m_vehicle.m_core.p * Mathf.Rad2Deg).ToString("0.0") + " °/s";
                        yawRate.text = (m_vehicle.m_core.r * Mathf.Rad2Deg).ToString("0.0") + " °/s";
                    }

                    if (m_vehicle.m_hardpoints == Oyedoyin.Common.Controller.StoreState.Disconnected)
                    {
                        if (weaponCount.gameObject.activeSelf) { weaponCount.gameObject.SetActive(false); }
                        if (currentWeapon.gameObject.activeSelf) { currentWeapon.gameObject.SetActive(false); }
                        if (ammoCount.gameObject.activeSelf) { ammoCount.gameObject.SetActive(false); }
                    }

                    //WEAPON
                    if (m_vehicle.m_hardpoints == Oyedoyin.Common.Controller.StoreState.Connected)
                    {
                        int count = 0;
                        if (m_vehicle.m_gunState == Oyedoyin.Common.Misc.ControlState.Active) { count++; }
                        if (m_vehicle.m_rocketState == Oyedoyin.Common.Misc.ControlState.Active) { count++; }
                        if (m_vehicle.m_missileState == Oyedoyin.Common.Misc.ControlState.Active) { count++; }
                        if (m_vehicle.m_bombState == Oyedoyin.Common.Misc.ControlState.Active) { count++; }

                        //ACTIVATE
                        if (!weaponCount.gameObject.activeSelf)
                        {
                            weaponCount.gameObject.SetActive(true);
                            currentWeapon.gameObject.SetActive(true);
                            ammoCount.gameObject.SetActive(true);
                        }
                        //SET VALUES
                        weaponCount.text = "Weapon Count: " + count.ToString();
                        currentWeapon.text = "Current Weapon: " + m_vehicle.m_hardpointSelection.ToString();
                        if (m_vehicle.m_hardpointSelection == Oyedoyin.Common.Controller.Selection.Gun)
                        {
                            int ammoTotal = 0;
                            foreach (SilantroGun gun in m_vehicle.m_guns)
                            {
                                ammoTotal += gun.currentAmmo;
                            }
                            ammoCount.text = "Ammo Count: " + ammoTotal.ToString();
                        }

                        if (m_vehicle.m_hardpointSelection == Oyedoyin.Common.Controller.Selection.Missile)
                        {
                            ammoCount.text = "Ammo Count: " + m_vehicle.missiles.Count.ToString();
                        }

                        if (m_vehicle.m_hardpointSelection == Oyedoyin.Common.Controller.Selection.Rockets)
                        {
                            ammoCount.text = "Ammo Count: " + m_vehicle.rockets.Count.ToString();
                        }
                    }
                    else
                    {
                        if (weaponCount != null && weaponCount.gameObject.activeSelf)
                        {
                            weaponCount.gameObject.SetActive(false);
                            currentWeapon.gameObject.SetActive(false);
                            ammoCount.gameObject.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                if (flapLevel != null && flapLevel.gameObject.activeSelf) { flapLevel.gameObject.SetActive(false); }
                if (slatLevel != null && slatLevel.gameObject.activeSelf) { slatLevel.gameObject.SetActive(false); }
                if (collective != null && collective.gameObject.activeSelf) { collective.gameObject.SetActive(false); }

#if SILANTRO_ROTARY
            if (m_lowfi != null && m_lowfi.gameObject.activeSelf)
            {
                if (engineName != null)
                {
                    engineName.text = m_lowfi.transform.name;
                }
                if (gearState.gameObject.activeSelf == true)
                {
                    gearState.gameObject.SetActive(false);
                    brake.gameObject.SetActive(false);
                }

                if (weaponCount != null && weaponCount.gameObject.activeSelf)
                {
                    weaponCount.gameObject.SetActive(false);
                    currentWeapon.gameObject.SetActive(false);
                    ammoCount.gameObject.SetActive(false);
                }


                if (pitchRate != null && pitchRate.gameObject.activeSelf)
                {
                    turnRate.text = m_lowfi.m_turnrate.ToString("0.0") + " °/s";
                    pitchRate.text = m_lowfi.m_pitchrate.ToString("0.0") + " °/s";
                    rollRate.text = m_lowfi.m_rollrate.ToString("0.0") + " °/s";
                    yawRate.text = m_lowfi.m_yawrate.ToString("0.0") + " °/s";
                }

                if(heading != null)
                {
                    heading.text = "Heading = " + m_lowfi.transform.eulerAngles.y.ToString("0.0") + " °";
                }

                //AMBIENT
                pressure.text = "Pressure = " + m_lowfi.m_air_pressure.ToString("0.0") + " kpa";
                density.text = "Air Density = " + m_lowfi.m_air_density.ToString("0.000") + " kg/m3";
                //
                float Temperature = m_lowfi.m_air_temperature;
                if (temperatureUnit == TemperatureUnit.Celsius)
                {
                    temperature.text = "Temperature = " + Temperature.ToString("0.0") + " °C";
                }
                if (temperatureUnit == TemperatureUnit.Fahrenheit)
                {
                    float temp = (Temperature * (9 / 5)) + 32f;
                    temperature.text = "Temperature = " + temp.ToString("0.0") + " °F";
                }

                float gfore = m_lowfi.m_gforce;
                if (gload != null)
                {
                    if (gfore > 4f) { gload.color = Color.red; } else if (gfore < -1f) { gload.color = Color.yellow; } else { gload.color = Color.white; }
                    gload.text = "G-Load = " + gfore.ToString("0.00");
                }

                climb.text = "Climb = " + (m_lowfi.helicopter.velocity.y * (float)Oyedoyin.Mathematics.Constants.toFtMin).ToString("0.0") + " ft/min";

                //WEIGHT SETTINGS
                float Weight = m_lowfi.currentWeight;
                if (weightUnit == WeightUnit.Kilogram)
                {
                    weight.text = "Weight = " + Weight.ToString("0.0") + " kg";
                }
                if (weightUnit == WeightUnit.Tonne)
                {
                    float tonneWeight = Weight * 0.001f;
                    weight.text = "Weight = " + tonneWeight.ToString("0.00") + " T";
                }
                if (weightUnit == WeightUnit.Pound)
                {
                    float poundWeight = Weight * 2.20462f;
                    weight.text = "Weight = " + poundWeight.ToString("0.0") + " lb";
                }
                if (weightUnit == WeightUnit.Ounce)
                {
                    float ounceWeight = Weight * 35.274f;
                    weight.text = "Weight = " + ounceWeight.ToString("0.0") + " Oz";
                }
                if (weightUnit == WeightUnit.Stone)
                {
                    float stonneWeight = Weight * 0.15747f;
                    weight.text = "Weight = " + stonneWeight.ToString("0.0") + " St";
                }
                //FUEL
                float Fuel = m_lowfi.fuelLevel;
                if (weightUnit == WeightUnit.Kilogram)
                {
                    fuel.text = "Fuel = " + Fuel.ToString("0.0") + " kg";
                }
                if (weightUnit == WeightUnit.Tonne)
                {
                    float tonneWeight = Fuel * 0.001f;
                    fuel.text = "Fuel = " + tonneWeight.ToString("0.00") + " T";
                }
                if (weightUnit == WeightUnit.Pound)
                {
                    float poundWeight = Fuel * 2.20462f;
                    fuel.text = "Fuel = " + poundWeight.ToString("0.0") + " lb";
                }
                if (weightUnit == WeightUnit.Ounce)
                {
                    float ounceWeight = Fuel * 35.274f;
                    fuel.text = "Fuel = " + ounceWeight.ToString("0.0") + " Oz";
                }
                if (weightUnit == WeightUnit.Stone)
                {
                    float stonneWeight = Fuel * 0.15747f;
                    fuel.text = "Fuel = " + stonneWeight.ToString("0.0") + " St";
                }
                //SPEED
                float Speed = m_lowfi.vz;

                if (speedUnit == SpeedUnit.Knots)
                {
                    float speedly = Speed * 1.944f;
                    speed.text = "Airspeed = " + speedly.ToString("0.0") + " knots";
                }
                if (speedUnit == SpeedUnit.MeterPerSecond)
                {
                    float speedly = Speed;
                    speed.text = "Airspeed = " + speedly.ToString("0.0") + " m/s";
                }
                if (speedUnit == SpeedUnit.FeetPerSecond)
                {
                    float speedly = Speed * 3.2808f;
                    speed.text = "Airspeed = " + speedly.ToString("0.0") + " ft/s";
                }
                if (speedUnit == SpeedUnit.MilesPerHour)
                {
                    float speedly = Speed * 2.237f;
                    speed.text = "Airspeed = " + speedly.ToString("0.0") + " mph";
                }
                if (speedUnit == SpeedUnit.KilometerPerHour)
                {
                    float speedly = Speed * 3.6f;
                    speed.text = "Airspeed = " + speedly.ToString("0.0") + " kmh";
                }



                float Altitude = m_lowfi.transform.position.y * (float)Oyedoyin.Mathematics.Constants.m2ft;
                if (altitudeUnit == AltitudeUnit.Feet)
                {
                    float distance = Altitude;
                    altitude.text = "Altitude = " + distance.ToString("0.0") + " ft";
                }
                if (altitudeUnit == AltitudeUnit.NauticalMiles)
                {
                    float distance = Altitude * 0.00054f;
                    altitude.text = "Altitude = " + distance.ToString("0.0") + " NM";
                }
                if (altitudeUnit == AltitudeUnit.Kilometer)
                {
                    float distance = Altitude / 3280.8f;
                    altitude.text = "Altitude = " + distance.ToString("0.0") + " km";
                }
                if (altitudeUnit == AltitudeUnit.Meter)
                {
                    float distance = Altitude / 3.2808f;
                    altitude.text = "Altitude = " + distance.ToString("0.0") + " m";
                }


                enginePower.text = "Engine Throttle = " + (m_lowfi.corePower * 100f).ToString("0.0") + " %";
                collective.text = "Collective = " + (m_lowfi.m_collective_power * 100f).ToString("0.0") + " %";
            }
#endif
            }
        }
    }


    #region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(SilantroMisc))]
    [CanEditMultipleObjects]
    public class SilantroMiscEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = Color.cyan;
        SilantroMisc extension;
        SerializedProperty control;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            extension = (SilantroMisc)target;
            control = serializedObject.FindProperty("m_display");
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector();
            serializedObject.Update();

            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_function"), new GUIContent("Function"));

            if (extension.m_function != SilantroMisc.Function.Transponder && extension.m_function != SilantroMisc.Function.DataDisplay)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_cleanState"), new GUIContent("Cleanup State"));
                if (extension.m_cleanState == SilantroMisc.Cleanup.Active)
                {
                    GUILayout.Space(5f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("destroyTime"), new GUIContent("Destroy Time"));
                }
                if (extension.m_function != SilantroMisc.Function.CleanUp)
                {
                    //1. SOUND SYSTEM
                    if (extension.m_function == SilantroMisc.Function.CaseSound || extension.m_function == SilantroMisc.Function.ImpactSound)
                    {
                        GUILayout.Space(5f);
                        GUI.color = silantroColor;
                        EditorGUILayout.HelpBox("Sounds", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        GUIContent soundLabel = new GUIContent("Sound Clips");
                        SerializedProperty muzs = this.serializedObject.FindProperty("sounds");
                        EditorGUILayout.PropertyField(muzs.FindPropertyRelative("Array.size"), soundLabel);
                        GUILayout.Space(3f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Clips", MessageType.None);
                        GUI.color = backgroundColor;
                        for (int i = 0; i < muzs.arraySize; i++)
                        {
                            GUIContent label = new GUIContent("Clip " + (i + 1).ToString());
                            EditorGUILayout.PropertyField(muzs.GetArrayElementAtIndex(i), label);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(3f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Settings", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("soundRange"), new GUIContent("Range"));
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("soundVolume"), new GUIContent("Volume"));
                    }
                }
            }
            if (extension.m_function == SilantroMisc.Function.Transponder)
            {
                GUILayout.Space(3f);
                EditorGUILayout.HelpBox("Please note this functionality requires a collider be attached to the gameobject", MessageType.Info);
                GUILayout.Space(5f);
                EditorGUILayout.HelpBox("Definition", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("silantroTag"), new GUIContent("Tag"));
                GUILayout.Space(3f);
                serializedObject.FindProperty("silantroTexture").objectReferenceValue = EditorGUILayout.ObjectField("Display Icon", serializedObject.FindProperty("silantroTexture").objectReferenceValue, typeof(Texture2D), true) as Texture2D;
            }
            if (extension.m_function == SilantroMisc.Function.DataDisplay)
            {
                GUILayout.Space(10f);
                GUI.color = Color.yellow;
                EditorGUILayout.HelpBox("Controller", MessageType.None);
                GUI.color = backgroundColor;

#if SILANTRO_ROTARY
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(control.FindPropertyRelative("m_type"), new GUIContent("Mode"));
#endif
                if (extension.m_display.m_type == Display.ModelType.Arcade)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(control.FindPropertyRelative("m_lowfi"), new GUIContent(" "));
                }
                else
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(control.FindPropertyRelative("m_vehicle"), new GUIContent(" "));
                }



                GUILayout.Space(15f);
                GUI.color = Color.yellow;
                EditorGUILayout.HelpBox("Unit Display Setup", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                extension.m_display.units = (Display.UnitsSetup)EditorGUILayout.EnumPopup("Unit System", extension.m_display.units);
                GUILayout.Space(3f);
                if (extension.m_display.units == Display.UnitsSetup.Custom)
                {
                    EditorGUI.indentLevel++;
                    GUILayout.Space(3f);
                    extension.m_display.speedUnit = (Display.SpeedUnit)EditorGUILayout.EnumPopup("Speed Unit", extension.m_display.speedUnit);
                    GUILayout.Space(3f);
                    extension.m_display.altitudeUnit = (Display.AltitudeUnit)EditorGUILayout.EnumPopup("Altitude Unit", extension.m_display.altitudeUnit);
                    GUILayout.Space(3f);
                    extension.m_display.temperatureUnit = (Display.TemperatureUnit)EditorGUILayout.EnumPopup("Temperature Unit", extension.m_display.temperatureUnit);
                    GUILayout.Space(3f);
                    extension.m_display.forceUnit = (Display.ForceUnit)EditorGUILayout.EnumPopup("Force Unit", extension.m_display.forceUnit);
                    GUILayout.Space(3f);
                    extension.m_display.weightUnit = (Display.WeightUnit)EditorGUILayout.EnumPopup("Weight Unit", extension.m_display.weightUnit);
                    GUILayout.Space(3f);
                    extension.m_display.torqueUnit = (Display.TorqueUnit)EditorGUILayout.EnumPopup("Torque Unit", extension.m_display.torqueUnit);
                    EditorGUI.indentLevel--;
                }
                else if (extension.m_display.units == Display.UnitsSetup.Metric)
                {
                    extension.m_display.speedUnit = Display.SpeedUnit.MeterPerSecond;
                    extension.m_display.altitudeUnit = Display.AltitudeUnit.Meter;
                    extension.m_display.temperatureUnit = Display.TemperatureUnit.Celsius;
                    extension.m_display.forceUnit = Display.ForceUnit.Newton;
                    extension.m_display.weightUnit = Display.WeightUnit.Kilogram;
                    extension.m_display.torqueUnit = Display.TorqueUnit.NewtonMeter;
                }
                else if (extension.m_display.units == Display.UnitsSetup.Imperial)
                {
                    extension.m_display.speedUnit = Display.SpeedUnit.Knots;
                    extension.m_display.altitudeUnit = Display.AltitudeUnit.Feet;
                    extension.m_display.temperatureUnit = Display.TemperatureUnit.Fahrenheit;
                    extension.m_display.forceUnit = Display.ForceUnit.PoundForce;
                    extension.m_display.weightUnit = Display.WeightUnit.Pound;
                    extension.m_display.torqueUnit = Display.TorqueUnit.PoundForceFeet;
                }

                GUILayout.Space(5f);
                GUI.color = Color.yellow;
                EditorGUILayout.HelpBox("Output Ports", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                extension.m_display.displayPoints = EditorGUILayout.Toggle("Show", extension.m_display.displayPoints);
                if (extension.m_display.displayPoints)
                {
                    GUILayout.Space(5f);
                    extension.m_display.speed = EditorGUILayout.ObjectField("Speed Text", extension.m_display.speed, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.altitude = EditorGUILayout.ObjectField("Altitude Text", extension.m_display.altitude, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.fuel = EditorGUILayout.ObjectField("Fuel Text", extension.m_display.fuel, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.heading = EditorGUILayout.ObjectField("Heading Text", extension.m_display.heading, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.weight = EditorGUILayout.ObjectField("Weight Text", extension.m_display.weight, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.gload = EditorGUILayout.ObjectField("G-Load", extension.m_display.gload, typeof(Text), true) as Text;

                    GUILayout.Space(5f);
                    extension.m_display.engineName = EditorGUILayout.ObjectField("Engine Name Text", extension.m_display.engineName, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.enginePower = EditorGUILayout.ObjectField("Engine Power Text", extension.m_display.enginePower, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.propellerPower = EditorGUILayout.ObjectField("Propeller Power Text", extension.m_display.propellerPower, typeof(Text), true) as Text;

                    if (extension.m_display.m_vehicle != null && extension.m_display.m_vehicle.m_type == Oyedoyin.Common.Controller.VehicleType.Aircraft)
                    {
                        GUILayout.Space(3f);
                        extension.m_display.collective = EditorGUILayout.ObjectField("Engine Thrust", extension.m_display.collective, typeof(Text), true) as Text;
                    }
                    else
                    {
                        GUILayout.Space(3f);
                        extension.m_display.collective = EditorGUILayout.ObjectField("Rotor Collective", extension.m_display.collective, typeof(Text), true) as Text;
                    }


                    GUILayout.Space(5f);
                    extension.m_display.density = EditorGUILayout.ObjectField("Density Text", extension.m_display.density, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.pressure = EditorGUILayout.ObjectField("Pressure Text", extension.m_display.pressure, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.temperature = EditorGUILayout.ObjectField("Temperature Text", extension.m_display.temperature, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.Time = EditorGUILayout.ObjectField("Time Text", extension.m_display.Time, typeof(Text), true) as Text;

                    GUILayout.Space(5f);
                    extension.m_display.brake = EditorGUILayout.ObjectField("Parking Brake Text", extension.m_display.brake, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.gearState = EditorGUILayout.ObjectField("Brake Lever", extension.m_display.gearState, typeof(Text), true) as Text;

                    if (extension.m_display.m_vehicle != null && extension.m_display.m_vehicle.m_type == Oyedoyin.Common.Controller.VehicleType.Aircraft)
                    {
                        GUILayout.Space(3f);
                        extension.m_display.flapLevel = EditorGUILayout.ObjectField("Flap Text", extension.m_display.flapLevel, typeof(Text), true) as Text;
                        GUILayout.Space(3f);
                        extension.m_display.slatLevel = EditorGUILayout.ObjectField("Slat Text", extension.m_display.slatLevel, typeof(Text), true) as Text;
                    }


                    if (extension.m_display.m_vehicle != null && extension.m_display.m_vehicle.m_hardpoints == Oyedoyin.Common.Controller.StoreState.Connected)
                    {
                        GUILayout.Space(5f);
                        extension.m_display.weaponCount = EditorGUILayout.ObjectField("Weapon Count", extension.m_display.weaponCount, typeof(Text), true) as Text;
                        GUILayout.Space(3f);
                        extension.m_display.currentWeapon = EditorGUILayout.ObjectField("Current Weapon", extension.m_display.currentWeapon, typeof(Text), true) as Text;
                        GUILayout.Space(3f);
                        extension.m_display.ammoCount = EditorGUILayout.ObjectField("Ammo Count", extension.m_display.ammoCount, typeof(Text), true) as Text;
                    }





                    GUILayout.Space(5f);
                    extension.m_display.pitchRate = EditorGUILayout.ObjectField("Pitch Rate Label", extension.m_display.pitchRate, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.rollRate = EditorGUILayout.ObjectField("Roll Rate Label", extension.m_display.rollRate, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.yawRate = EditorGUILayout.ObjectField("Yaw Rate Label", extension.m_display.yawRate, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.turnRate = EditorGUILayout.ObjectField("Turn Rate Label", extension.m_display.turnRate, typeof(Text), true) as Text;

                    GUILayout.Space(5f);
                    ///extension.m_display.commandPitch = EditorGUILayout.ObjectField("Command Pitch Label", extension.m_display.commandPitch, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    //extension.m_display.commandRoll = EditorGUILayout.ObjectField("Command Roll Label", extension.m_display.commandRoll, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    //extension.m_display.commandYaw = EditorGUILayout.ObjectField("Command Yaw Label", extension.m_display.commandYaw, typeof(Text), true) as Text;



                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Autopilot", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    extension.m_display.speedLabel = EditorGUILayout.ObjectField("Speed Label", extension.m_display.speedLabel, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.altitudeLabel = EditorGUILayout.ObjectField("Altitude Label", extension.m_display.altitudeLabel, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.headingLabel = EditorGUILayout.ObjectField("Heading Label", extension.m_display.headingLabel, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.climbLabel = EditorGUILayout.ObjectField("Climb Label", extension.m_display.climbLabel, typeof(Text), true) as Text;
                    GUILayout.Space(3f);
                    extension.m_display.m_container = EditorGUILayout.ObjectField("Container", extension.m_display.m_container, typeof(GameObject), true) as GameObject;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    #endregion
}