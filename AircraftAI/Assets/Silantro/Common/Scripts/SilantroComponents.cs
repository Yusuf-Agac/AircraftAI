using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using Oyedoyin.Common.Misc;
using Oyedoyin.Mathematics;
using System.Collections.Generic;



namespace Oyedoyin.Common.Components
{
    #region Fuel System

    [System.Serializable]
    public class SilantroFuelSystem
    {
        // ----------------------------------- Selectibles
        public enum FuelSelector { Left, Right, External, Automatic }
        public FuelSelector fuelSelector = FuelSelector.Automatic;

        // ----------------------------------- Connections
        public Controller controller;
        public SilantroTank[] fuelTanks;
        public List<SilantroTank> internalFuelTanks;
        public List<SilantroTank> externalTanks;


        public List<SilantroTank> RightTanks;
        public List<SilantroTank> LeftTanks;
        public List<SilantroTank> CentralTanks;


        public bool dumpFuel = false;
        public float fuelDumpRate = 1f;//Rate at which fuel will be dumped in kg/s
        public float actualFlowRate;

        public bool refillTank = false;
        public float refuelRate = 1f;
        public float actualrefuelRate;

        // ---------------------------------- Alert System
        AudioSource FuelAlert;
        public AudioClip fuelAlert;
        public float minimumFuelAmount = 50f;
        bool fuelAlertActivated;


        // ---------------------------------- Fuel Data
        public float RightFuelAmount;
        public float LeftFuelAmount;
        public float CenterFuelAmount;
        public float ExternalFuelAmount;
        public float engineConsumption;

        public string fuelType;
        public float timeFactor = 1;
        public float fuelFlow;
        private bool initialized;




        // ---------------------------------------------------------------------CONTROLS-------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        public void ActivateFuelDump()
        {
            if (!refillTank) { dumpFuel = !dumpFuel; }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ActivateTankRefill()
        {
            if (!dumpFuel) { refillTank = !refillTank; }
        }
        /// <summary>
        /// 
        /// </summary>
        public void StopAlertSound()
        {
            if (fuelAlertActivated) { FuelAlert.Stop(); controller.fuelLow = false; }
        }





        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            if (fuelTanks == null || fuelTanks.Length < 1) { Debug.LogError("No fuel tank is assigned to aircraft!!"); return; }
            // ------------------------- Setup Containers
            controller.fuelCapacity = 0f; controller.fuelLevel = 0f;
            externalTanks = new List<SilantroTank>();
            LeftTanks = new List<SilantroTank>();
            RightTanks = new List<SilantroTank>();
            CentralTanks = new List<SilantroTank>();
            fuelType = fuelTanks[0].fuelType.ToString();
            if (fuelAlert) { Handler.SetupSoundSource(fuelTanks[0].transform, fuelAlert, "Alert Point", 50f, true, false, out FuelAlert); FuelAlert.volume = 1f; }


            // ------------------------ Filter
            foreach (SilantroTank tank in fuelTanks)
            {
                controller.fuelCapacity += tank._actualAmount;
                controller.fuelLevel += tank._currentAmount;
                if (tank.tankType == SilantroTank.TankType.Internal) { internalFuelTanks.Add(tank); }
                if (tank.tankType == SilantroTank.TankType.External) { externalTanks.Add(tank); }
                if (tank.fuelType.ToString() != fuelType) { Debug.LogError("Fuel Type Selection not uniform"); return; }
            }
            foreach (SilantroTank tank in internalFuelTanks)
            {
                if (tank.tankPosition == SilantroTank.TankPosition.Center) { CentralTanks.Add(tank); }
                if (tank.tankPosition == SilantroTank.TankPosition.Left) { LeftTanks.Add(tank); }
                if (tank.tankPosition == SilantroTank.TankPosition.Right) { RightTanks.Add(tank); }
            }
            initialized = true;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Compute()
        {
            if (initialized)
            {
                controller.fuelLevel = 0f;
                if (fuelTanks.Length > 0) { foreach (SilantroTank tank in fuelTanks) { controller.fuelLevel += tank._currentAmount; } }
                foreach (SilantroTank tank in fuelTanks) { if (tank._currentAmount < 0) { tank._currentAmount = 0f; } }

                if (controller.fuelLevel <= minimumFuelAmount) { controller.fuelLow = true; fuelAlertActivated = true; LowFuelAction(); }
                if (controller.fuelLevel > minimumFuelAmount && fuelAlertActivated) { fuelAlertActivated = false; FuelAlert.Stop(); }

                if (dumpFuel) { DumpFuel(); }
                if (refillTank) { RefuelTank(); }
                timeFactor = controller._fixedTimestep;

                // ---------------- Actual Usage
                DepleteTanks();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void DepleteTanks()
        {
            engineConsumption = controller.fuelFlow;
            fuelFlow = engineConsumption * timeFactor;
            LeftFuelAmount = RightFuelAmount = CenterFuelAmount = ExternalFuelAmount = 0f;

            foreach (SilantroTank tank in LeftTanks) { LeftFuelAmount += tank._currentAmount; }
            foreach (SilantroTank tank in RightTanks) { RightFuelAmount += tank._currentAmount; }
            foreach (SilantroTank tank in CentralTanks) { CenterFuelAmount += tank._currentAmount; }
            foreach (SilantroTank tank in externalTanks) { ExternalFuelAmount += tank._currentAmount; }



            // -------------------------------- Use External Tanks
            if (fuelSelector == FuelSelector.External)
            {
                if (externalTanks != null && externalTanks.Count > 0)
                {
                    if (ExternalFuelAmount <= 0) { fuelSelector = FuelSelector.Automatic; }
                    float individualRate = engineConsumption / externalTanks.Count;
                    foreach (SilantroTank tank in externalTanks) { tank._currentAmount -= individualRate * timeFactor; }
                }
                else { fuelSelector = FuelSelector.Automatic; }
            }


            // -------------------------------- Use Left Tanks
            if (fuelSelector == FuelSelector.Left)
            {
                if (LeftTanks != null && LeftTanks.Count > 0)
                {
                    float individualRate = engineConsumption / LeftTanks.Count;
                    foreach (SilantroTank tank in LeftTanks) { tank._currentAmount -= individualRate * timeFactor; }
                    if (LeftFuelAmount <= 0) { fuelSelector = FuelSelector.Automatic; }
                }
                else { fuelSelector = FuelSelector.Automatic; }
            }



            // -------------------------------- Use Right Tanks
            if (fuelSelector == FuelSelector.Right)
            {
                if (RightTanks != null && RightTanks.Count > 0)
                {
                    float individualRate = engineConsumption / RightTanks.Count;
                    foreach (SilantroTank tank in RightTanks) { tank._currentAmount -= individualRate * timeFactor; }
                    if (RightFuelAmount <= 0) { fuelSelector = FuelSelector.Automatic; }
                }
                else { fuelSelector = FuelSelector.Automatic; }
            }




            // -------------------------------- Automatic
            if (fuelSelector == FuelSelector.Automatic)
            {
                //A> USE CENTRAL TANKS FIRST
                if (CentralTanks != null && CentralTanks.Count > 0 && CenterFuelAmount > 0)
                {
                    //DEPLETE
                    float individualRate = engineConsumption / CentralTanks.Count;
                    foreach (SilantroTank tank in CentralTanks)
                    {
                        tank._currentAmount -= individualRate * timeFactor;
                    }
                }
                else
                {
                    //B> USE EXTERNAL TANKS
                    if (externalTanks != null && externalTanks.Count > 0 && ExternalFuelAmount > 0)
                    {
                        //DEPLETE
                        float individualRate = engineConsumption / externalTanks.Count;
                        foreach (SilantroTank tank in externalTanks)
                        {
                            tank._currentAmount -= individualRate * timeFactor;
                        }
                    }
                    //C> USE OTHER TANKS
                    else
                    {
                        int usefulTanks = LeftTanks.Count + RightTanks.Count;
                        float individualRate = engineConsumption / usefulTanks;
                        //LEFT
                        foreach (SilantroTank tank in LeftTanks)
                        {
                            tank._currentAmount -= individualRate * timeFactor;
                        }
                        //RIGHT
                        foreach (SilantroTank tank in RightTanks)
                        {
                            tank._currentAmount -= individualRate * timeFactor;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        void LowFuelAction()
        {
            if (FuelAlert != null)
            {
                if (!FuelAlert.isPlaying) { FuelAlert.Play(); }
                else { FuelAlert.Stop(); }
            }
        }




        /// <summary>
        /// 
        /// </summary>
        public void RefuelTank()
        {
            actualrefuelRate = refuelRate * controller._timestep;
            if (internalFuelTanks != null && internalFuelTanks.Count > 0)
            {
                float indivialRate = actualrefuelRate / internalFuelTanks.Count;
                foreach (SilantroTank tank in internalFuelTanks)
                {
                    tank._currentAmount += indivialRate;
                }
            }
            //CONTROL AMOUNT
            foreach (SilantroTank tank in internalFuelTanks)
            {
                if (tank._currentAmount > tank._capacity)
                {
                    tank._currentAmount = tank._capacity;
                }
            }
            if (controller.fuelLevel >= controller.fuelCapacity)
            {
                refillTank = false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        void DumpFuel()
        {
            actualFlowRate = fuelDumpRate * controller._timestep;
            if (internalFuelTanks != null && internalFuelTanks.Count > 0)
            {
                float indivialRate = actualFlowRate / internalFuelTanks.Count;
                foreach (SilantroTank tank in internalFuelTanks)
                {
                    tank._currentAmount -= indivialRate;
                }
            }
            //CONTROL AMOUNT
            foreach (SilantroTank tank in fuelTanks)
            {
                if (tank._currentAmount <= 0)
                {
                    tank._currentAmount = 0;
                }
            }
            if (controller.fuelLevel <= 0)
            {
                dumpFuel = false;
            }
        }
    }

    #endregion

    #region Engine Core

    [System.Serializable]
    public class EngineCore
    {
        #region call functions
        /// <summary>
        /// 
        /// </summary>
        public void ShutDownEngine() { shutdown = true; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startMode"></param>
        /// <param name="fuelLevel"></param>
        /// <param name="controllerState"></param>
        public void StartEngine()
        {
            if (controller != null && controller.isControllable)
            {
                //MAKE SURE SOUND IS SET PROPERLY
                if (backIdle == null || ignitionExterior == null || shutdownExterior == null)
                {
                    Debug.Log("Engine " + engine.name + " cannot start due to incorrect Audio configuration");
                }
                else
                {
                    //MAKE SURE THERE IS FUEL TO START THE ENGINE
                    if (controller && controller.fuelLevel > 1f)
                    {
                        //ACTUAL START ENGINE
                        if (controller.m_startMode == Controller.StartMode.Cold)
                        {
                            start = true;
                        }
                        if (controller.m_startMode == Controller.StartMode.Hot)
                        {
                            //JUMP START ENGINE
                            active = true;
                            StateActive(); clutching = false; CurrentEngineState = EngineState.Active;
                        }
                    }
                    else
                    {
                        Debug.Log("Engine " + engine.name + " cannot start due to low fuel");
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Compute()
        {
            AnalyseSound();
            AnalyseCore();
            AnalyseEffects(out float _nozzleActuation);

            // -------------- NOZZLE
            float p = (float)controller._pitchInput;
            float r = (float)controller._rollInput;
            float y = (float)controller._yawInput;
            if (vectoringType != ThrustVectoring.None && nozzlePivot != null) { AnalyseNozzle(p, r, y); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startMode"></param>
        public void Initialize()
        {
            //startMode = 2 Hot
            //startMode = 1 Cold

            //----------------------------------------SET SOUND SOURCES
            GameObject soundPoint = new GameObject("_sources");
            soundPoint.transform.parent = engine;
            soundPoint.transform.localPosition = Vector3.zero;
            if (soundMode == SoundMode.Advanced)
            {
                if (frontIdle) { Handler.SetupSoundSource(soundPoint.transform, frontIdle, "_front_point", 150f, true, true, out frontSource); }
                if (sideIdle) { Handler.SetupSoundSource(soundPoint.transform, sideIdle, "_side_point", 150f, true, true, out sideSource); }
            }
            if (backIdle) { Handler.SetupSoundSource(soundPoint.transform, backIdle, "_rear_point", 150f, true, true, out backSource); }
            if (interiorIdle && interiorMode == InteriorMode.Active) { Handler.SetupSoundSource(soundPoint.transform, interiorIdle, "_interior_base_point", 80f, true, true, out interiorBase); }


            if (ignitionInterior && interiorMode == InteriorMode.Active) { Handler.SetupSoundSource(soundPoint.transform, ignitionInterior, "_interior_sound_point", 50f, false, false, out interiorSource); }
            if (ignitionExterior) { Handler.SetupSoundSource(soundPoint.transform, ignitionExterior, "_exterior_sound_point", 150f, false, false, out exteriorSource); }
            if (controller.m_startMode == Controller.StartMode.Hot) { trueCoreAcceleration = 10f; }


            // ---------------------------------------- Set up Afterburner Flame
            if (flameObject != null && flameMaterial != null)
            {
                baseDiameter = flameObject.transform.localScale.x;
                baseLength = flameObject.transform.localScale.z;
                baseFlameColor = flameMaterial.GetColor("_TintColor");

                if (controller.m_startMode == Controller.StartMode.Hot)
                { actualDiameter = targetDiameter = normalDiameter; actualLength = targetLength = normalLength; }
            }
            baseColor = Color.white;
            minimumRPM = (1 - (overspeedAllowance / 100)) * functionalRPM;
            maximumRPM = (1 + (overspeedAllowance / 100)) * functionalRPM;
            gFactor = 1f;

            if (vectoringType != ThrustVectoring.None)
            {
                if (nozzlePivot != null) { baseNormalRotation = nozzlePivot.transform.localRotation; }
                pitchAxisRotation = Handler.EstimateModelProperties(pitchDirection.ToString(), pitchAxis.ToString());
                rollAxisRotation = Handler.EstimateModelProperties(rollDirection.ToString(), rollAxis.ToString());
                yawAxisRotation = Handler.EstimateModelProperties(yawDirection.ToString(), yawAxis.ToString());
            }

            AnalyseEffects(out float _nozzleActuation);
        }
        /// <summary>
        /// 
        /// </summary>
        public void EvaluateRPMLimits()
        {
            minimumRPM = (1 - (overspeedAllowance / 100)) * functionalRPM;
            maximumRPM = (1 + (overspeedAllowance / 100)) * functionalRPM;
        }

        #endregion

        #region base Engine

        public string engineIdentifier = "Default Engine";
        public float cameraSector;
        public Transform engine;
        public Controller controller;
        public Transform intakeFan;
        public Transform nozzlePivot;

        public enum EngineNumber { N1, N2, N3, N4, N5, N6 }
        public EngineNumber engineNumber = EngineNumber.N1;
        public enum EnginePosition { Left, Right, Center }
        public EnginePosition enginePosition = EnginePosition.Center;
        public enum RotationAxis { X, Y, Z }
        public RotationAxis rotationAxis = RotationAxis.Z;
        public RotationAxis pitchAxis = RotationAxis.X;
        public RotationAxis rollAxis = RotationAxis.Y;
        public RotationAxis yawAxis = RotationAxis.Z;
        public enum RotationDirection { CW, CCW }
        public RotationDirection rotationDirection = RotationDirection.CCW;
        public RotationDirection pitchDirection = RotationDirection.CW;
        public RotationDirection rollDirection = RotationDirection.CW;
        public RotationDirection yawDirection = RotationDirection.CW;
        public enum ThrustVectoring { None, PitchOnly, TwoAxis, ThreeAxis }
        public ThrustVectoring vectoringType = ThrustVectoring.None;
        public enum FlameType { Circular, Rectangular }
        public FlameType flameType = FlameType.Circular;

        public enum PitchLabel { PitchAxis }
        public PitchLabel pitchLabel = PitchLabel.PitchAxis;
        public enum RollLabel { RollAxis }
        public RollLabel rollLabel = RollLabel.RollAxis;
        public enum YawLabel { YawAxis }
        public YawLabel yawLabel = YawLabel.YawAxis;
        public enum SoundMode { Basic, Advanced }
        public SoundMode soundMode = SoundMode.Basic;
        public enum InteriorMode { Off, Active }
        public InteriorMode interiorMode = InteriorMode.Off;

        //--------------------------------------SOUND
        public float sideVolume;
        public float frontVolume;
        public float backVolume;
        public float interiorVolume, exteriorVolume;
        public float overideExteriorVolume, overideInteriorVolume;
        public float overidePitch, basePitch;

        public AudioSource frontSource;
        public AudioSource sideSource;
        public AudioSource backSource;
        public AudioSource interiorSource;
        public AudioSource exteriorSource;
        public AudioSource interiorBase;

        public AudioClip frontIdle;
        public AudioClip sideIdle;
        public AudioClip backIdle;
        public AudioClip interiorIdle;
        public AudioClip ignitionInterior, ignitionExterior;
        public AudioClip shutdownInterior, shutdownExterior;
        public bool baseEffects = true, baseEmission;




        //----------------------------------------------------------------------
        //----------------------------------------------------------------------
        //----------------------------------------------------------------------
        //------------------------------------------------------STATE MANAGEMENT
        public enum EngineState { Off, Starting, Active }
        public EngineState CurrentEngineState;

        //--------------------------------------VARIABLES
        [Tooltip("Engine throttle up and down speed.")] [Range(0.01f, 1f)] public float baseCoreAcceleration = 0.25f;
        public float corePower, trueCoreAcceleration;
        public bool start, shutdown, clutching, active;
        public float coreRPM, factorRPM, norminalRPM, functionalRPM = 1000f;
        public float controlInput;
        public float idlePercentage = 10f, coreFactor, fuelFactor;
        [Tooltip("Percentage of RPM allowed over or under the functional RPM.")] [Range(0, 10)] public float overspeedAllowance = 3f;
        public bool afterburnerOperative, canUseAfterburner;
        public float pitchTarget;
        public float gControl, gForce, pitchFactor;

        public float totalLoad, engineLoad = 10f, engineInertia = 2, startingTorque;
        public float inputInertia;
        public float inputLoad, shaftRPM, TorqueInput;
        public bool torqueEngaged;
        public float Ωr, Ω, Ωmax;


        public float minimumRPM = 900, maximumRPM = 1000;


        public bool reverseThrustAvailable, reverseThrustEngaged;
        public float reverseThrustFactor;
        [Range(0, 60f)] public float reverseThrustPercentage = 50f;
        public float burnerFactor;
        public float gFactor = 1f;


        //----------------------------------------------------------------------
        //----------------------------------------------------------------------
        //----------------------------------------------------------------------
        //------------------------------------------------------EFFECT MANAGEMENT
        public ParticleSystem exhaustSmoke;
        public ParticleSystem.EmissionModule smokeModule;
        public ParticleSystem exhaustDistortion;
        ParticleSystem.EmissionModule distortionModule;


        // -------------------------- Emission
        public Color baseColor, finalColor;
        public float smokeEmissionLimit = 50f;
        public float distortionEmissionLimit = 20f;
        public Material burnerCoreMaterial;
        public Material burnerPipeMaterial;
        public float emissionCore;
        public float maximumNormalEmission = 1f;
        public float maximumAfterburnerEmission = 2f;

        // --------------------------- Afterburner
        public SilantroActuator m_actuator;
        public float targetDiameter, currentDiameter, baseDiameter;
        public float targetLength, currentLength, baseLength;

        public float normalDiameter = 1000f, normalLength = 1000f;
        public float wetDiameter = 1200f, wetLength = 1200f;
        [Range(0, 1f)] public float wetAlpha = 0.5f, normalAlpha = 0.01f;
        public float actualDiameter, actualLength;
        Quaternion baseNormalRotation, angleEffect;
        Vector3 pitchAxisRotation, yawAxisRotation, rollAxisRotation;
        public float maximumPitchDeflection = 15f, maximumRollDeflection = 15f, maximumYawDeflection = 15f;

        // ---------------------------- Flame
        public GameObject flameObject;
        public Material flameMaterial;
        Color baseFlameColor, currentFlameColor;

        public float currentLevel, flameAlpha, targetAlpha;
        public float alphaSpeed = 0.1f, scaleSpeed = 100f, value;
        public bool coreFlame;




        /// <summary>
        /// 
        /// </summary>
        /// <param name="m_pitch"></param>
        /// <param name="m_roll"></param>
        /// <param name="m_yaw"></param>
        public void AnalyseNozzle(float m_pitch, float m_roll, float m_yaw)
        {
            var yaw = m_yaw * maximumYawDeflection;
            var pitch = m_pitch * maximumPitchDeflection;
            var roll = m_roll * maximumRollDeflection;

            var pitchEffect = Quaternion.AngleAxis(pitch, pitchAxisRotation);
            var rollEffect = Quaternion.AngleAxis(roll, rollAxisRotation);
            var yawEffect = Quaternion.AngleAxis(yaw, yawAxisRotation);

            if (vectoringType == ThrustVectoring.PitchOnly) { angleEffect = pitchEffect; }
            else if (vectoringType == ThrustVectoring.TwoAxis) { angleEffect = pitchEffect * rollEffect; }
            else { angleEffect = yawEffect * pitchEffect * rollEffect; }
            nozzlePivot.localRotation = baseNormalRotation * angleEffect;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actuation"></param>
        private void AnalyseEffects(out float actuation)
        {
            // Collect Modules
            if (!smokeModule.enabled && exhaustSmoke != null) { smokeModule = exhaustSmoke.emission; }
            if (!distortionModule.enabled && exhaustDistortion != null) { distortionModule = exhaustDistortion.emission; }

            // ------------------------- Nozzle Driver
            if (afterburnerOperative) { actuation = coreFactor; }
            else { actuation = coreFactor * controlInput * 0.3333333333f; }
            if (!active) { actuation = 0f; }
            if (m_actuator != null) { m_actuator.targetActuationLevel = actuation; }


            // Control Amount
            if (smokeModule.enabled) { smokeModule.rateOverTime = smokeEmissionLimit * coreFactor; }
            if (distortionModule.enabled) { distortionModule.rateOverTime = distortionEmissionLimit * coreFactor; }


            // ------------------------------ Engine Emission
            if (canUseAfterburner && afterburnerOperative) { value = Mathf.Lerp(value, maximumAfterburnerEmission, 0.025f); }
            else { value = Mathf.Lerp(value, maximumNormalEmission, 0.02f); }
            finalColor = baseColor * Mathf.LinearToGammaSpace(value * coreFactor);
            if (burnerCoreMaterial != null) { burnerCoreMaterial.SetColor("_EmissionColor", finalColor); }
            if (burnerPipeMaterial != null) { burnerPipeMaterial.SetColor("_EmissionColor", finalColor); }



            // -------------------------------Afterburner Flame
            if (flameObject != null && flameMaterial != null)
            {
                if (active)
                {
                    if (canUseAfterburner && afterburnerOperative) { targetAlpha = wetAlpha * coreFactor; targetDiameter = wetDiameter; targetLength = wetLength; }
                    else { targetAlpha = normalAlpha * coreFactor; targetDiameter = normalDiameter; targetLength = normalLength; }
                }
                else { targetAlpha = 0f; targetDiameter = baseDiameter; targetLength = baseLength; }


                // --------------------------- Flame Color
                flameAlpha = Mathf.MoveTowards(flameAlpha, targetAlpha, controller._timestep * alphaSpeed);
                currentFlameColor = new Color(baseFlameColor.r, baseFlameColor.b, baseFlameColor.g, flameAlpha);

                // --------------------------- Flame Scale
                currentDiameter = baseDiameter + ((targetDiameter - baseDiameter) * coreFactor);
                currentLength = baseLength + ((targetLength - baseLength) * coreFactor);
                actualDiameter = Mathf.MoveTowards(actualDiameter, currentDiameter, controller._timestep * scaleSpeed);
                actualLength = Mathf.MoveTowards(actualLength, currentLength, controller._timestep * scaleSpeed);

                //Set
                if (flameType == FlameType.Rectangular) { flameObject.transform.localScale = new Vector3(actualDiameter, baseDiameter, actualLength * coreFactor); }
                else { flameObject.transform.localScale = new Vector3(actualDiameter, actualDiameter, actualLength); }
                flameMaterial.SetColor("_TintColor", currentFlameColor);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cameraState"></param>
        void AnalyseSound()
        {
            if (controller.m_cameraState == SilantroCamera.CameraState.Exterior)
            {
                //RESET
                interiorVolume = 0f; exteriorVolume = 1f;

                if (soundMode == SoundMode.Advanced && controller.m_view != null)
                {
                    //------------------------------------------FRONT || RIGHT
                    if (cameraSector > 0 && cameraSector < 90) { frontVolume = cameraSector / 90f; sideVolume = 1 - frontVolume; backVolume = 0f; }

                    //------------------------------------------FRONT || LEFT
                    if (cameraSector >= 90 && cameraSector < 180) { sideVolume = (cameraSector - 90) / 90f; frontVolume = 1 - sideVolume; backVolume = 0f; }

                    //------------------------------------------BACK || LEFT
                    if (cameraSector >= 180 && cameraSector < 270) { backVolume = (cameraSector - 180) / 90f; sideVolume = 1 - backVolume; frontVolume = 0f; }

                    //------------------------------------------BACK || RIGHT
                    if (cameraSector >= 270 && cameraSector < 360) { sideVolume = (cameraSector - 270) / 90f; backVolume = 1 - sideVolume; frontVolume = 0f; }
                }
                else { backVolume = 1f; }
            }
            else { backVolume = sideVolume = frontVolume = 0f; interiorVolume = 1f; exteriorVolume = 0f; }

            //-------------------PITCH
            float speedFactor = ((coreRPM + (controller.m_rigidbody.velocity.magnitude * 1.943f) + 10f) - functionalRPM * (idlePercentage / 100f)) / (functionalRPM - functionalRPM * (idlePercentage / 100f));

            basePitch = 0.25f + (0.7f * speedFactor);
            if (afterburnerOperative && canUseAfterburner) { pitchTarget = 0.5f + (1.35f * speedFactor); } else { pitchTarget = basePitch; }
            if (fuelFactor < 1) { overidePitch = pitchTarget; } else { overidePitch = fuelFactor * Mathf.Lerp(overidePitch, pitchTarget, controller._timestep); }
            basePitch *= fuelFactor; backSource.pitch = overidePitch;
            if (interiorMode == InteriorMode.Active && interiorBase != null) { interiorBase.pitch = overidePitch; }
            if (soundMode == SoundMode.Advanced) { frontSource.pitch = basePitch; sideSource.pitch = basePitch; }


            //-------------------SET VOLUMES
            backSource.volume = overideExteriorVolume * backVolume;
            if (soundMode == SoundMode.Advanced)
            {
                frontSource.volume = overideExteriorVolume * frontVolume;
                sideSource.volume = overideExteriorVolume * sideVolume;
            }
            exteriorSource.volume = exteriorVolume;
            if (interiorMode == InteriorMode.Active && interiorBase != null && interiorSource != null)
            {
                interiorSource.volume = interiorVolume;
                if (controller != null && controller.m_view != null) { interiorBase.volume = overideInteriorVolume * controller.m_view.maximumInteriorVolume; } else { interiorBase.volume = overideInteriorVolume; }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startMode"></param>
        /// <param name="cameraState"></param>
        /// <param name="fuelLevel"></param>
        /// <param name="fuelLow"></param>
        /// <param name="fuelExhausted"></param>
        /// <param name="autoThrottle"></param>
        void AnalyseCore()
        {

            // ----------------- Check Control state
            if (controller.m_startMode != Controller.StartMode.Hot)
            {
                //if (autoThrottle) { trueCoreAcceleration = 10f; }
                //else { trueCoreAcceleration = baseCoreAcceleration; }
                trueCoreAcceleration = baseCoreAcceleration;
            }


            if (active && controller.fuelLevel < 5) { ShutDownEngine(); }
            if (active && controller.isControllable == false) { ShutDownEngine(); }


            //------------------ POWER
            if (active) { if (corePower < 1f) { corePower += controller._timestep * trueCoreAcceleration; } }
            else if (corePower > 0f) { corePower -= controller._timestep * trueCoreAcceleration; }
            if (controlInput > 1) { controlInput = 1f; }
            if (corePower > 1) { corePower = 1f; }
            if (!active && corePower < 0) { corePower = 0f; }
            if (active && controller.fuelExhausted) { shutdown = true; }
            fuelFactor = 1f;

            if (active && canUseAfterburner && afterburnerOperative) { burnerFactor += controller._timestep * trueCoreAcceleration; }
            else if (burnerFactor > 0f) { burnerFactor -= controller._timestep * trueCoreAcceleration; }
            if (burnerFactor > 1) { burnerFactor = 1f; }
            if (burnerFactor < 0) { burnerFactor = 0f; }


            //------------------ FUEL
            if (active && controller.fuelLow)
            {
                float startRange = 0.2f; float endRange = 0.85f; float cycleRange = (endRange - startRange) / 2f;
                float offset = cycleRange + startRange; fuelFactor = offset + Mathf.Sin(Time.time * 3f) * cycleRange;
            }

            //------------------- STATES
            switch (CurrentEngineState) { case EngineState.Off: StateOff(); break; case EngineState.Starting: StateStart(); break; case EngineState.Active: StateActive(); break; }

            //------------------- RPM
            if (active) { factorRPM = Mathf.Lerp(factorRPM, norminalRPM, trueCoreAcceleration * controller._fixedTimestep * 2); }
            else { factorRPM = Mathf.Lerp(factorRPM, 0, trueCoreAcceleration * controller._fixedTimestep * 2f); }
            float limitRPM = (functionalRPM * (100 + overspeedAllowance)) / 100f; if (factorRPM > limitRPM) { factorRPM = limitRPM; }


            //TORQUE CONFIGURATION
            TorqueInput += startingTorque;
            totalLoad = engineLoad + inputLoad + 0.0001f;
            Ω += ((TorqueInput - totalLoad) / (inputInertia + engineInertia)) * Time.fixedDeltaTime;
            Ωmax = (2 * Mathf.PI * maximumRPM) / 60;
            Ω = Mathf.Clamp(Ω, 0, Ωmax);
            Ωr = Mathf.Max(0.0f, Ω / (2.0f * Mathf.PI));

            // engine friction 
            if (TorqueInput < totalLoad && Ωr < 1.0)
            {
                Ωr = Ωr < 0.1f ? 0.0f : (float)MathBase.Inertia(0.0, Ωr, Time.fixedDeltaTime, 0.1);
                Ω = 2.0f * Mathf.PI * Ωr;
            }
            shaftRPM = 60.0f * Ωr;
            shaftRPM = Mathf.Clamp(shaftRPM, 0, maximumRPM);

            coreRPM = factorRPM * corePower * fuelFactor * gFactor;
            coreFactor = coreRPM / functionalRPM;
            if (coreRPM < 10 && CurrentEngineState == EngineState.Active) { ShutDownEngine(); }

            if (intakeFan)
            {
                if (rotationDirection == RotationDirection.CCW)
                {
                    if (rotationAxis == RotationAxis.X) { intakeFan.Rotate(new Vector3(coreRPM * controller._timestep, 0, 0)); }
                    if (rotationAxis == RotationAxis.Y) { intakeFan.Rotate(new Vector3(0, coreRPM * controller._timestep, 0)); }
                    if (rotationAxis == RotationAxis.Z) { intakeFan.Rotate(new Vector3(0, 0, coreRPM * controller._timestep)); }
                }
                //
                if (rotationDirection == RotationDirection.CW)
                {
                    if (rotationAxis == RotationAxis.X) { intakeFan.Rotate(new Vector3(-1f * coreRPM * controller._timestep, 0, 0)); }
                    if (rotationAxis == RotationAxis.Y) { intakeFan.Rotate(new Vector3(0, -1f * coreRPM * controller._timestep, 0)); }
                    if (rotationAxis == RotationAxis.Z) { intakeFan.Rotate(new Vector3(0, 0, -1f * coreRPM * controller._timestep)); }
                }
            }

            //-------------------SOUND
            if (controller.m_cameraState == SilantroCamera.CameraState.Exterior) { overideExteriorVolume = corePower; overideInteriorVolume = 0f; }
            if (controller.m_cameraState == SilantroCamera.CameraState.Interior) { overideInteriorVolume = corePower; overideExteriorVolume = 0f; }
            if (afterburnerOperative && controlInput < 0.5f) { afterburnerOperative = false; }
            if (reverseThrustAvailable && reverseThrustEngaged && afterburnerOperative) { afterburnerOperative = false; }

            if (active && reverseThrustAvailable)
            {
                if (reverseThrustEngaged) { reverseThrustFactor += controller._timestep * baseCoreAcceleration; }
                else { reverseThrustFactor -= controller._timestep * baseCoreAcceleration; }
            }
            if (reverseThrustFactor > 1) { reverseThrustFactor = 1f; }
            if (reverseThrustFactor < 0) { reverseThrustFactor = 0f; }
        }

        #endregion

        #region core functions
        /// <summary>
        /// 
        /// </summary>
        public void StateActive()
        {
            if (exteriorSource.isPlaying) { exteriorSource.Stop(); }
            if (interiorSource != null && interiorSource.isPlaying) { interiorSource.Stop(); }

            //------------------STOP ENGINE
            if (shutdown)
            {
                exteriorSource.clip = shutdownExterior; exteriorSource.Play();
                if (interiorSource != null) { interiorSource.clip = shutdownInterior; interiorSource.Play(); }
                CurrentEngineState = EngineState.Off;
                active = false;
                engine.SendMessage("ReturnIgnition");
            }

            //------------------RUN
            if (torqueEngaged) { norminalRPM = (functionalRPM * (idlePercentage / 100f)) + (shaftRPM - (functionalRPM * (idlePercentage / 100f))) * controlInput; }
            else { norminalRPM = (functionalRPM * (idlePercentage / 100f)) + (maximumRPM - (functionalRPM * (idlePercentage / 100f))) * controlInput; }
        }
        /// <summary>
        /// 
        /// </summary>
        void StateStart()
        {
            if (clutching)
            {
                if (!exteriorSource.isPlaying)
                {
                    CurrentEngineState = EngineState.Active; clutching = false; StateActive();
                    startingTorque = 0;
                }

            }
            else { exteriorSource.Stop(); if (interiorSource != null) { interiorSource.Stop(); } CurrentEngineState = EngineState.Off; }

            //------------------RUN
            norminalRPM = functionalRPM * (idlePercentage / 100f);
        }
        /// <summary>
        /// 
        /// </summary>
        void StateOff()
        {
            if (exteriorSource.isPlaying && corePower < 0.01f) { exteriorSource.Stop(); }
            if (interiorSource != null && interiorSource.isPlaying && corePower < 0.01f) { interiorSource.Stop(); }


            //------------------START ENGINE
            if (start)
            {
                exteriorSource.clip = ignitionExterior; exteriorSource.Play();
                if (interiorSource != null) { interiorSource.clip = ignitionInterior; interiorSource.Play(); }
                CurrentEngineState = EngineState.Starting; clutching = true;
                active = true;
                engine.SendMessage("ReturnIgnition");
                startingTorque = 50;
            }


            //------------------RUN
            norminalRPM = 0f;
        }

        #endregion
    }

    #endregion

    #region Controller Helper

    [Serializable]
    public class ControllerFunctions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="craft"></param>
        /// <param name="controller"></param>
        public void RestoreFunction(Rigidbody rigidbody, Controller controller)
        {
            if (rigidbody != null && controller != null)
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.transform.position = controller.basePosition;
                rigidbody.transform.rotation = controller.baseRotation;
                controller.m_fuelSystem.ActivateTankRefill();

                //controller.TurnOffEngines();
                if (controller.gearActuator != null && controller.gearActuator.actuatorState == SilantroActuator.ActuatorState.Disengaged) { controller.gearActuator.EngageActuator(); }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public bool CheckEngineState(EngineCore core)
        {
            bool check;
            if (core.CurrentEngineState == EngineCore.EngineState.Active) { check = true; }
            else { check = false; }
            return check;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="craft"></param>
        /// <param name="controller"></param>
        public void PositionAircraftFunction(Rigidbody rigidbody, Controller controller, bool chancePosition = false)
        {
            if (rigidbody != null && controller != null)
            {
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                controller.m_fuelSystem.ActivateTankRefill();
                if (chancePosition)
                {
                    Vector3 initialPosition = rigidbody.transform.position;
                    rigidbody.transform.position = new Vector3(initialPosition.x, controller.m_startAltitude, initialPosition.z);
                }
                rigidbody.velocity = rigidbody.transform.forward * controller.m_startSpeed;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="craft"></param>
        /// <param name="controller"></param>
        public void StartAircraftFunction(Rigidbody rigidbody, Controller controller)
        {
            if (rigidbody != null && controller != null && controller.m_startMode == Controller.StartMode.Hot)
            {
                //POSITION AIRCRAFT
                controller.PositionAircraft();
                //SET ENGINE
                controller.TurnOnEngines();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator StartUpAircraft(Controller controller)
        {
            yield return new WaitForSeconds(0.002f);//JUST LAG A BIT BEHIND CONTROLLER SCRIPT
                                                    //STARTUP AIRCRAFT	
            controller.StartHotAircraft();

            //RAISE GEAR
            if (controller.gearActuator != null && controller.gearActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) { controller.gearActuator.DisengageActuator(); }
            else { controller.m_gearState = Controller.GearState.Up; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        public void InternalControlSetup(Controller controller)
        {
            if (!controller.getOutPosition)
            {
                GameObject getOutPos = new GameObject("Get Out Position");
                getOutPos.transform.SetParent(controller.transform);
                getOutPos.transform.localPosition = new Vector3(-3f, 0f, 0f);
                getOutPos.transform.localRotation = Quaternion.identity;
                controller.getOutPosition = getOutPos.transform;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        public void EnterAircraftFunction(Controller controller)
        {
            if (!controller.pilotOnBoard && controller.m_controlType == Controller.ControlType.Internal)
            {
                if (controller.canopyActuator != null)
                {
                    if (controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Disengaged) { controller.canopyActuator.EngageActuator(); }
                }
                // Start Countdown
                controller.StartCoroutine(EntryProcedure(controller));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public IEnumerator EntryProcedure(Controller controller)
        {
            if (controller.canopyActuator != null) { yield return new WaitUntil(() => controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Engaged); }
            else { yield return new WaitForSeconds(controller.m_entryTimer); }


            // Enter Check List
            if (controller.m_player != null)
            {
                // Activate Camera
                controller.m_view.ActivateExteriorCamera();
                // Make aircraft/helicopter controllable
                controller.isControllable = true;
                // Deactivate player gameobject
                controller.m_player.SetActive(false);
                // Enable interior pilot gameobject
                if (controller.m_interiorPilot != null) { controller.m_interiorPilot.SetActive(true); }
                // Enable canvas object
                if (controller.m_canvas != null && !controller.m_canvas.activeSelf)
                {
                    SilantroMisc m_display = controller.m_canvas.gameObject.GetComponent<SilantroMisc>();
                    if (m_display != null && m_display.m_function == SilantroMisc.Function.DataDisplay) { m_display.m_display.m_vehicle = controller; }
                    controller.m_canvas.SetActive(true);
                }
                // Make player object child of the aircraft/helicopter
                controller.m_player.transform.SetParent(controller.transform);
                // Reset Position
                controller.m_player.transform.localPosition = Vector3.zero;
                // Reset Rotation
                controller.m_player.transform.localRotation = Quaternion.identity;
                // Wait a little for actuator switch
                yield return new WaitForSeconds(1.5f);

                // Close doors and complete entry
                if (controller.canopyActuator != null)
                {
                    if (controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) { controller.canopyActuator.DisengageActuator(); }
                    yield return new WaitUntil(() => controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Disengaged);
                }
                else { yield return new WaitForSeconds(controller.m_entryTimer); }

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                controller.pilotOnBoard = true;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        public void ExitAircraftFunction(Controller controller)
        {
            if (controller.transform.position.y < 10 && controller.m_core.Vkts < 3)
            {
                if (controller.pilotOnBoard && controller.m_controlType == Controller.ControlType.Internal)
                {
                    if (controller.canopyActuator != null)
                    {
                        if (controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Disengaged) { controller.canopyActuator.EngageActuator(); }
                    }
                    // Start Countdown
                    controller.StartCoroutine(ExitProcedure(controller));
                }
            }
            else if (controller.transform.position.y > 10) { Debug.LogError("Cant exit aircraft above 10m"); }
            else if (controller.m_core.Vkts > 3) { Debug.LogError("Cant exit aircraft above 3kts, slow down and put parking brakes on"); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public IEnumerator ExitProcedure(Controller controller)
        {
            // Turn Off Engines
            controller.TurnOffEngines();
            // Turn Off Lights
            if (controller.m_lightState == Controller.LightState.On) { controller.m_input.TurnOffLights(); }

            if (controller.canopyActuator != null) { yield return new WaitUntil(() => controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Engaged); }
            else { yield return new WaitForSeconds(controller.m_exitTimer); }

            // Enter Check List
            if (controller.m_player != null)
            {
                // Disable interior pilot object
                if (controller.m_interiorPilot != null) { controller.m_interiorPilot.SetActive(false); }
                // Drag the player gameobject out of the aircraft/helicopter hierarchy
                controller.m_player.transform.SetParent(null);
                // Set player position to get out position
                controller.m_player.transform.position = controller.getOutPosition.position;
                // Reset player rotation
                controller.m_player.transform.rotation = controller.getOutPosition.rotation;
                controller.m_player.transform.rotation = Quaternion.Euler(0f, controller.m_player.transform.eulerAngles.y, 0f);
                // Activate player object
                controller.m_player.SetActive(true);
                // Reset aircraft/helicopter cameras
                controller.m_view.ResetCameras();
                // Disable canvas object
                if (controller.m_canvas != null && controller.m_canvas.activeSelf)
                {
                    SilantroMisc m_display = controller.m_canvas.gameObject.GetComponent<SilantroMisc>();
                    if (m_display != null && m_display.m_function == SilantroMisc.Function.DataDisplay) { m_display.m_display.m_vehicle = null; }
                    controller.m_canvas.SetActive(false);
                }
                // Wait a little for actuator switch
                yield return new WaitForSeconds(1.5f);

                // Close doors and complete entry
                if (controller.canopyActuator != null)
                {
                    if (controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Engaged) { controller.canopyActuator.DisengageActuator(); }
                    yield return new WaitUntil(() => controller.canopyActuator.actuatorState == SilantroActuator.ActuatorState.Disengaged);
                }
                else { yield return new WaitForSeconds(controller.m_exitTimer); }

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                controller.pilotOnBoard = false;
                // Make aircraft/helicopter controllable
                controller.m_player = null;
                controller.isControllable = false;
            }
        }
    }

    #endregion

    #region Rocket Motor
    [Serializable]
    public class RocketMotor
    {
        public enum BurnType { Regressive, Neutral, Progressive }
        public BurnType burnType = BurnType.Neutral;

        public float m_meanThrust = 150000;
        public Transform exitPoint;

        // ----------------------- Thrust Factor
        public AnimationCurve burnCurve;
        public float thrustFactor;
        float activeTime;
        public float engineBurnTime;
        public float fireDuration = 5f;

        // ----------------------- Effects
        public ParticleSystem exhaustSmoke;
        ParticleSystem.EmissionModule smokeModule;
        public ParticleSystem exhaustFlame;
        ParticleSystem.EmissionModule flameModule;
        public float maximumSmokeEmissionValue = 50f;
        public float maximumFlameEmissionValue = 50f;

        // ----------------------- Audio
        public AudioClip motorSound;
        AudioSource boosterSound;
        public float maximumPitch = 1.2f;

        // ----------------------- Output
        public bool active;
        public float m_thrust;

        /// <summary>
        /// 
        /// </summary>
        public void Fire()
        {
            active = true;
            engineBurnTime = 0.0f;
        }
        /// <summary>
        /// 
        /// </summary>
        private void PlotThrustCurve()
        {
            burnCurve = new AnimationCurve();

            // PROGRESSIVE BURN
            if (burnType == BurnType.Progressive)
            {
                burnCurve.AddKey(new Keyframe(0, 0));
                burnCurve.AddKey(new Keyframe(0.041055718f, 0.102848764f));
                burnCurve.AddKey(new Keyframe(0.065982405f, 0.195789207f));
                burnCurve.AddKey(new Keyframe(0.089442815f, 0.311990335f));
                burnCurve.AddKey(new Keyframe(0.11143695f, 0.421551817f));
                burnCurve.AddKey(new Keyframe(0.126099707f, 0.524493136f));
                burnCurve.AddKey(new Keyframe(0.148093842f, 0.644021395f));
                burnCurve.AddKey(new Keyframe(0.171554252f, 0.766867041f));
                burnCurve.AddKey(new Keyframe(0.212609971f, 0.85643164f));
                burnCurve.AddKey(new Keyframe(0.269794721f, 0.916042322f));
                burnCurve.AddKey(new Keyframe(0.348973607f, 0.949001861f));
                burnCurve.AddKey(new Keyframe(0.423753666f, 0.978653754f));
                burnCurve.AddKey(new Keyframe(0.498533724f, 1.008305648f));
                burnCurve.AddKey(new Keyframe(0.57771261f, 1.044587446f));
                burnCurve.AddKey(new Keyframe(0.658357771f, 1.084186631f));
                burnCurve.AddKey(new Keyframe(0.739002933f, 1.117141298f));
                burnCurve.AddKey(new Keyframe(0.799120235f, 1.140197387f));
                burnCurve.AddKey(new Keyframe(0.857771261f, 1.15329157f));
                burnCurve.AddKey(new Keyframe(0.895894428f, 1.0568194f));
                burnCurve.AddKey(new Keyframe(0.920821114f, 0.917201703f));
                burnCurve.AddKey(new Keyframe(0.939882698f, 0.797537047f));
                burnCurve.AddKey(new Keyframe(0.957478006f, 0.64465467f));
                burnCurve.AddKey(new Keyframe(0.972140762f, 0.498426555f));
                burnCurve.AddKey(new Keyframe(0.983870968f, 0.342241405f));
                burnCurve.AddKey(new Keyframe(0.992668622f, 0.202677293f));
                burnCurve.AddKey(new Keyframe(0.998533724f, 0.086378738f));
                burnCurve.AddKey(new Keyframe(1f, 0f));
            }
            // NEUTRAL BURN
            if (burnType == BurnType.Neutral)
            {
                burnCurve.AddKey(new Keyframe(0f, 0f));
                burnCurve.AddKey(new Keyframe(0.011540828f, 0.128099174f));
                burnCurve.AddKey(new Keyframe(0.021510337f, 0.289256198f));
                burnCurve.AddKey(new Keyframe(0.036367648f, 0.47107438f));
                burnCurve.AddKey(new Keyframe(0.046363904f, 0.648760331f));
                burnCurve.AddKey(new Keyframe(0.059603092f, 0.830578512f));
                burnCurve.AddKey(new Keyframe(0.076031721f, 0.983471074f));
                burnCurve.AddKey(new Keyframe(0.142508492f, 1.066115702f));
                burnCurve.AddKey(new Keyframe(0.215324026f, 1.066115702f));
                burnCurve.AddKey(new Keyframe(0.297821552f, 1.049586777f));
                burnCurve.AddKey(new Keyframe(0.381943887f, 1.037190083f));
                burnCurve.AddKey(new Keyframe(0.454739362f, 1.024793388f));
                burnCurve.AddKey(new Keyframe(0.537250261f, 1.016528926f));
                burnCurve.AddKey(new Keyframe(0.618143037f, 1.008264463f));
                burnCurve.AddKey(new Keyframe(0.697404317f, 0.991735537f));
                burnCurve.AddKey(new Keyframe(0.771817914f, 0.979338843f));
                burnCurve.AddKey(new Keyframe(0.841377143f, 0.966942149f));
                burnCurve.AddKey(new Keyframe(0.893076841f, 0.917355372f));
                burnCurve.AddKey(new Keyframe(0.92032416f, 0.756198347f));
                burnCurve.AddKey(new Keyframe(0.936251304f, 0.599173554f));
                burnCurve.AddKey(new Keyframe(0.952198508f, 0.454545455f));
                burnCurve.AddKey(new Keyframe(0.971335152f, 0.280991736f));
                burnCurve.AddKey(new Keyframe(0.987289042f, 0.140495868f));
                burnCurve.AddKey(new Keyframe(1.001651555f, 0f));
            }
            // REGRESSIVE
            if (burnType == BurnType.Regressive)
            {
                burnCurve.AddKey(new Keyframe(0.005592615f, 0.006872852f));
                burnCurve.AddKey(new Keyframe(0.027852378f, 0.113402062f));
                burnCurve.AddKey(new Keyframe(0.045896022f, 0.23024055f));
                burnCurve.AddKey(new Keyframe(0.062505415f, 0.371134021f));
                burnCurve.AddKey(new Keyframe(0.079114807f, 0.512027491f));
                burnCurve.AddKey(new Keyframe(0.094280324f, 0.683848797f));
                burnCurve.AddKey(new Keyframe(0.106663971f, 0.841924399f));
                burnCurve.AddKey(new Keyframe(0.127489484f, 0.972508591f));
                burnCurve.AddKey(new Keyframe(0.15672317f, 1.099656357f));
                burnCurve.AddKey(new Keyframe(0.190221106f, 1.182130584f));
                burnCurve.AddKey(new Keyframe(0.249059074f, 1.171821306f));
                burnCurve.AddKey(new Keyframe(0.302323679f, 1.140893471f));
                burnCurve.AddKey(new Keyframe(0.36541627f, 1.092783505f));
                burnCurve.AddKey(new Keyframe(0.436897783f, 1.054982818f));
                burnCurve.AddKey(new Keyframe(0.505582989f, 1.013745704f));
                burnCurve.AddKey(new Keyframe(0.588302675f, 0.951890034f));
                burnCurve.AddKey(new Keyframe(0.649994706f, 0.903780069f));
                burnCurve.AddKey(new Keyframe(0.727083273f, 0.862542955f));
                burnCurve.AddKey(new Keyframe(0.805582027f, 0.81443299f));
                burnCurve.AddKey(new Keyframe(0.856088827f, 0.75257732f));
                burnCurve.AddKey(new Keyframe(0.902446889f, 0.652920962f));
                burnCurve.AddKey(new Keyframe(0.930636172f, 0.525773196f));
                burnCurve.AddKey(new Keyframe(0.943404853f, 0.408934708f));
                burnCurve.AddKey(new Keyframe(0.958979468f, 0.288659794f));
                burnCurve.AddKey(new Keyframe(0.975978708f, 0.151202749f));
                burnCurve.AddKey(new Keyframe(0.999985561f, 0f));
            }
            MathBase.LinearizeCurve(burnCurve);
        }
        /// <summary>
        /// 
        /// </summary>
        public void DrawGizmos()
        {
            PlotThrustCurve();

            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                if (exitPoint != null)
                {
                    Handles.color = Color.red;
                    Handles.ArrowHandleCap(0, exitPoint.position, exitPoint.rotation * Quaternion.LookRotation(-Vector3.forward), 2f, EventType.Repaint);
                }
#endif
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            // -------------------------------------- Particles
            if (exhaustSmoke != null) { smokeModule = exhaustSmoke.emission; smokeModule.rateOverTime = 0.0f; }
            if (exhaustFlame != null) { flameModule = exhaustFlame.emission; flameModule.rateOverTime = 0.0f; }

            // -------------------------------------- Sound
            if (motorSound) { Handler.SetupSoundSource(exitPoint.transform, motorSound, "Booster Sound", 80f, true, true, out boosterSound); }

            // -------------------------------------- Curve Data
            PlotThrustCurve();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timestep"></param>
        public void Compute(float timestep)
        {
            if (active)
            {
                if (!smokeModule.enabled && exhaustSmoke != null) { smokeModule = exhaustSmoke.emission; }
                if (!flameModule.enabled && exhaustFlame != null) { flameModule = exhaustFlame.emission; }

                engineBurnTime += timestep;
                activeTime = engineBurnTime / fireDuration;
                thrustFactor = burnCurve.Evaluate(activeTime);
                m_thrust = m_meanThrust * thrustFactor;

                // Sound
                if (boosterSound != null)
                {
                    float soundVolume = maximumPitch * thrustFactor;
                    boosterSound.volume = soundVolume;
                }
                // Effects
                if (exhaustFlame) { flameModule.rateOverTime = maximumFlameEmissionValue * thrustFactor; }
                if (exhaustSmoke) { smokeModule.rateOverTime = maximumSmokeEmissionValue * thrustFactor; }
                // Burn Out
                if (engineBurnTime > fireDuration) { active = false; }
            }
        }
    }
    #endregion

    #region RocketLauncher
    [Serializable]
    public class Launcher
    {
        public Controller m_controller;
        public AudioClip fireSound;
        private AudioSource launcherSound;

        public float rateOfFire = 3f;
        public float actualFireRate;
        float fireTimer;
        public float fireVolume = 0.7f;

        /// <summary>
        /// 
        /// </summary>
        public void Initialize(Transform m_transform)
        {
            if (rateOfFire != 0) { actualFireRate = 1.0f / rateOfFire; }
            else { actualFireRate = 0.01f; }
            fireTimer = 0.0f;
            if (fireSound)
            {
                Handler.SetupSoundSource(m_transform, fireSound, "Launch Sound Point", 100f, false, false, out launcherSound);
                launcherSound.volume = fireVolume;
            }
            m_controller.CountOrdnance();
        }
        /// <summary>
        /// 
        /// </summary>
        public void FireRocket()
        {
            if (fireTimer > actualFireRate)
            {
                if (m_controller.m_hardpoints == Controller.StoreState.Connected)
                {
                    if (m_controller != null && m_controller.rockets.Count > 0)
                    {
                        fireTimer = 0f;
                        //SELECT RANDOM ROCKET
                        int index = UnityEngine.Random.Range(0, m_controller.rockets.Count);
                        if (m_controller.rockets[index] != null)
                        {
                            m_controller.rockets[index].FireRocket(m_controller.m_rigidbody.velocity);

                            //PLAY SOUND
                            if (launcherSound != null && fireSound != null && !launcherSound.isPlaying) { launcherSound.PlayOneShot(fireSound); }
                            m_controller.CountOrdnance();
                        }
                    }
                    else { Debug.Log("Rocket System Offline"); }
                }
                else { Debug.Log("Weapon System Offline"); }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void FireMissile()
        {
            if (m_controller != null && m_controller.isControllable && m_controller.m_hardpoints == Controller.StoreState.Connected)
            {
                // Count Munitions
                m_controller.CountOrdnance();

                if (m_controller.missiles.Count > 0)
                {
                    if (m_controller.m_radar != null && m_controller.m_radar.m_lockedTarget != null && m_controller.m_radar.m_lockedTarget.m_body != null)
                    {
                        Vector3 targetDirection = (m_controller.m_radar.m_lockedTarget.m_body.transform.position - m_controller.m_radar.transform.position).normalized;
                        float m_direction = Vector3.Dot(targetDirection, m_controller.m_radar.transform.forward);

                        if (m_direction < 0.6f) { Debug.Log("Missile launch canceled. Reason: Target out of view range"); }
                        else
                        {
                            int index = UnityEngine.Random.Range(0, m_controller.missiles.Count);
                            if (m_controller.missiles[index] != null && m_controller.missiles[index].m_pylon != null && !m_controller.missiles[index].m_pylon.engaged)
                            {
                                m_controller.missiles[index].m_pylon.target = m_controller.m_radar.m_lockedTarget.m_body.transform;
                                m_controller.missiles[index].m_pylon.StartLaunchSequence();
                            }

                            // Play Sound
                            if (fireSound != null) { launcherSound.PlayOneShot(fireSound); }
                            m_controller.CountOrdnance();
                        }
                    }
                    else { Debug.Log("Locked Target/Radar Unavailable"); }
                }
                else { Debug.Log("Missile System Offline"); }
            }
            else { Debug.Log("Weapon System Offline"); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="timestep"></param>
        public void Compute(float timestep)
        {
            fireTimer += timestep;
        }
    }

    #endregion

}

namespace Oyedoyin.Common.Misc
{
    #region Utils

    public enum MovementAxis { Forward, Right, Up }
    public enum MovementDirection { Normal, Inverted }
    public enum RotationAxis { X, Y, Z }
    public enum RotationDirection { CW, CCW }
    public enum WeightUnit { Kilogram, Pounds }
    public enum ControlState { Active, Off }
    public enum ControlMode { Automatic, Manual }
    public class Handler
    {

        /// <summary>
        /// Creates a new sound source for the selected component.
        /// </summary>
        /// <param name="parentObject">Object to make the sound source a child of.</param>
        /// <param name="soundClip">Audio clip to play from the sound source.</param>
        /// <param name="name">Sound source label.</param>
        /// <param name="fallOffDistance">Maximum reach distance of the sound source.</param>
        /// <param name="loopSound">Determines if the sound loops or not e.g engine Idle loops, but Ignition sound is played once.</param>
        /// <param name="playOnCreate">Only used by continous playing sound sources.</param>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void SetupSoundSource(Transform parentObject, AudioClip soundClip, string name, float fallOffDistance, bool loopSound, bool playOnCreate, out AudioSource soundSource)
        {
            //CREATE SOURCE HOLDER
            GameObject source = new GameObject(name);
            source.transform.parent = parentObject; source.transform.localPosition = Vector3.zero;
            soundSource = source.AddComponent<AudioSource>();

            //CONFIGURE PROPERTIES
            soundSource.clip = soundClip; soundSource.volume = 0f;
            soundSource.loop = loopSound; soundSource.spatialBlend = 1f; soundSource.dopplerLevel = 0f;
            soundSource.rolloffMode = AudioRolloffMode.Custom; soundSource.maxDistance = fallOffDistance;
            soundSource.playOnAwake = false;
            if (playOnCreate) { soundSource.Play(); }
        }


        public static void Rotate(double RPM, double dt, Transform mesh, RotationDirection rotorDirection, RotationAxis rotationAxis)
        {
            float dx = (float)dt;
            float coreRPM = (float)RPM;
            if (coreRPM > 0)
            {
                if (rotorDirection == RotationDirection.CW)
                {
                    if (rotationAxis == RotationAxis.X) { mesh.transform.Rotate(new Vector3(coreRPM * 5f * dx, 0, 0)); }
                    if (rotationAxis == RotationAxis.Y) { mesh.transform.Rotate(new Vector3(0, coreRPM * 5f * dx, 0)); }
                    if (rotationAxis == RotationAxis.Z) { mesh.transform.Rotate(new Vector3(0, 0, coreRPM * 5f * dx)); }
                }
                if (rotorDirection == RotationDirection.CCW)
                {
                    if (rotationAxis == RotationAxis.X) { mesh.transform.Rotate(new Vector3(-1f * coreRPM * 5f * dx, 0, 0)); }
                    if (rotationAxis == RotationAxis.Y) { mesh.transform.Rotate(new Vector3(0, -1f * coreRPM * 5f * dx, 0)); }
                    if (rotationAxis == RotationAxis.Z) { mesh.transform.Rotate(new Vector3(0, 0, -1f * coreRPM * 5f * dx)); }
                }
            }
        }


        /// <summary>
        /// Prepares the mesh for component usage by setting up the required vector rotation/movement
        /// </summary>
        /// <param name="deflectionDirection"> Mesh rotation direction either CW or CCW.</param>
        /// <param name="axis">Mesh rotation axis.</param>
        /// <returns>
        /// Rotation axis vector3
        /// </returns>
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Vector3 EstimateModelProperties(string deflectionDirection, string axis)
        {
            Vector3 defaultAxis = Vector3.zero;
            if (deflectionDirection == "CCW")
            {
                if (axis == "X") { defaultAxis = new Vector3(-1, 0, 0); }
                else if (axis == "Y") { defaultAxis = new Vector3(0, -1, 0); }
                else if (axis == "Z") { defaultAxis = new Vector3(0, 0, -1); }
            }
            else
            {
                if (axis == "X") { defaultAxis = new Vector3(1, 0, 0); }
                else if (axis == "Y") { defaultAxis = new Vector3(0, 1, 0); }
                else if (axis == "Z") { defaultAxis = new Vector3(0, 0, 1); }
            }

            //RETURN
            defaultAxis.Normalize(); return defaultAxis;
        }

        public static Vector3 EstimateModelProperties(Transform m_object, MovementAxis axis, MovementDirection direction)
        {
            Vector3 defaultAxis = Vector3.zero;
            if (axis == MovementAxis.Forward) { defaultAxis = m_object.forward; }
            if (axis == MovementAxis.Right) { defaultAxis = m_object.right; }
            if (axis == MovementAxis.Up) { defaultAxis = m_object.up; }
            if (direction == MovementDirection.Inverted) { defaultAxis *= -1; }
            return defaultAxis;
        }



        /// <summary>
        /// 
        /// </summary>
        public class Axis
        {
            public string name = String.Empty;
            public string descriptiveName = String.Empty;
            public string descriptiveNegativeName = String.Empty;
            public string negativeButton = String.Empty;
            public string positiveButton = String.Empty;
            public string altNegativeButton = String.Empty;
            public string altPositiveButton = String.Empty;
            public float gravity = 0.0f;
            public float dead = 0.001f;
            public float sensitivity = 1.0f;
            public bool snap = false;
            public bool invert = false;
            public int type = 2;
            public int axis = 0;
            public int joyNum = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void ControlAxis(Axis axis, bool duplicate)
        {
#if UNITY_EDITOR
            SerializedObject serializedObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
            SerializedProperty axesProperty = serializedObject.FindProperty("m_Axes");

            SerializedProperty axisIter = axesProperty.Copy();
            axisIter.Next(true);
            axisIter.Next(true);
            while (axisIter.Next(false))
            {
                SerializedProperty desName = axisIter.FindPropertyRelative("descriptiveName");
                SerializedProperty name = axisIter.FindPropertyRelative("m_Name");
                if (desName != null && name != null)
                {
                    string desNameString = axisIter.FindPropertyRelative("descriptiveName").stringValue;
                    string nameString = axisIter.FindPropertyRelative("m_Name").stringValue;
                    if (desNameString == axis.descriptiveName) { return; }
                    else if (nameString == axis.name && !duplicate) { return; }
                }
            }

            axesProperty.arraySize++;
            serializedObject.ApplyModifiedProperties();

            SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex(axesProperty.arraySize - 1);
            axisProperty.FindPropertyRelative("m_Name").stringValue = axis.name;
            axisProperty.FindPropertyRelative("descriptiveName").stringValue = axis.descriptiveName;
            axisProperty.FindPropertyRelative("descriptiveNegativeName").stringValue = axis.descriptiveNegativeName;
            axisProperty.FindPropertyRelative("negativeButton").stringValue = axis.negativeButton;
            axisProperty.FindPropertyRelative("positiveButton").stringValue = axis.positiveButton;
            axisProperty.FindPropertyRelative("altNegativeButton").stringValue = axis.altNegativeButton;
            axisProperty.FindPropertyRelative("altPositiveButton").stringValue = axis.altPositiveButton;
            axisProperty.FindPropertyRelative("gravity").floatValue = axis.gravity;
            axisProperty.FindPropertyRelative("dead").floatValue = axis.dead;
            axisProperty.FindPropertyRelative("sensitivity").floatValue = axis.sensitivity;
            axisProperty.FindPropertyRelative("snap").boolValue = axis.snap;
            axisProperty.FindPropertyRelative("invert").boolValue = axis.invert;
            axisProperty.FindPropertyRelative("type").intValue = axis.type;
            axisProperty.FindPropertyRelative("axis").intValue = axis.axis;
            axisProperty.FindPropertyRelative("joyNum").intValue = axis.joyNum;
            serializedObject.ApplyModifiedProperties();
#endif
        }
    }

    #endregion
}