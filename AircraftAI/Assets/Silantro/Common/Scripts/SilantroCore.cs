#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using Oyedoyin.Analysis;
using Oyedoyin.Mathematics;
using Oyedoyin.Common.Misc;



namespace Oyedoyin.Common
{
    #region Component
    /// <summary>
    /// Handles core aircraft data collection and processing
    /// </summary>
    /// <remarks>
    /// This component will collect the data required by the aircraft and process them. It also handles the step calculation of the aircraft
    /// center of gravity based on the position and weight of the aircraft components
    /// </remarks>
    [DisallowMultipleComponent]
    public class SilantroCore : MonoBehaviour
    {

        // ------------------------------------- Selectibles
        public enum SystemType
        {
            /// <summary>
            /// Uses the local position of the assigned empty COG gameObject as the center of gravity of the aircraft/helicopter
            /// </summary>
            Basic,
            /// <summary>
            /// Computes the aircraft/helicopter center of gravity based on the position and weights of the fuel tanks, payload and munitions
            /// </summary>
            Advanced
        }
        public enum SpeedType
        {
            /// <summary>
            /// Maximum aircraft speed is greater than Mach 1
            /// </summary>
            Supersonic,
            /// <summary>
            /// Maximum aircraft speed is less than Mach 1
            /// </summary>
            Subsonic
        }
        public enum TensorMode
        {
            /// <summary>
            /// Uses the tensor matrix calculated by Unity
            /// </summary>
            Automatic,
            /// <summary>
            /// Uses the Pitch, Roll and Yaw Inertia values supplied by the user and 
            /// </summary>
            Manual
        }
        public enum PressureBreathing { Active, Off }

        /// <summary>
        /// This component is just as the name says 😊 it’s right at the center of the system functionality and performs a set of important functions;
        //1. Collects raw data about the aircraft/helicopter behavior and performance and filters them into forms required by the components.
        //2. It computes and sets the center of gravity of the aircraft/helicopter depending on the current weight and position of the individual(COG affecting) components
        //3. It also sets the inertia values of the base rigidbody if saddled with that responsibility.
        /// </summary>
        public SystemType functionality = SystemType.Basic;
        /// <summary>
        /// Determines if the aircraft/helicopter rigidbody inertia tensor matrix (Unity only considers the principal axis) is 
        /// automatically calculated by Unity from the collider sizes and position or is set manually by the user.
        /// </summary>
        public TensorMode tensorMode = TensorMode.Automatic;
        /// <summary>
        /// NB: This is only required for aircraft models. It is uses to activate speed particle effects based on the speed of the aircraft.
        /// </summary>
        public SpeedType speedType = SpeedType.Subsonic;

        // ------------------------------------- Connections
        public Controller controller;
        /// <summary>
        /// This enables the script to examine the pre-calculated inertia tensor values and enable to user to properly
        /// determine/guess the needed tensor values (If the real life value is not available)
        /// </summary>
        public Rigidbody sampleAircraft;
        /// <summary>
        /// This is the transform mentioned in the SystemType definition. This is used to mark the center of gravity of the aircraft/helicopter when it’s empty.
        /// Further reading: http://avstop.com/ac/weightbalance/ch3.html
        /// </summary>
        public Transform emptyCenterOfMass;
        public Transform m_handler;
        public Transform currentCOM;
        public Vector3 baseCenter, centerOfMass, deviation;
        public FAtmosphere m_atmosphere;
        /// <summary>
        /// This is used to determine the objects to measure ground positions from i.e. What object(s) in your scene can be considered as ground.
        /// The position measure from them is used to calculate ground effect on the rotors and aerofoils.
        /// </summary>
        public LayerMask groundLayer;

        // ---------------------------- Inertia
        /// <summary>
        /// This is the aircraft/helicopter Rolling Moment of Inertia (kg/m2) and is the property of the aircraft/helicopter 
        /// that determines how the mass is distributed along the Z-axis which tends to resist rolling. 
        /// Higher values require larger moment values to roll the aircraft/helicopter 
        /// </summary>
        [Tooltip("Resistance to movement on the roll axis")] public float Ixx = 2000;
        [Tooltip("Resistance to movement in the pitch axis")] public float Iyy = 5000;
        [Tooltip("Resistance to movement in the yaw axis")] public float Izz = 8000;
        public Vector3 baseInertiaTensor;
        public Vector3 inertiaTensor;
        /// <summary>
        /// This is the maximum allowable angular speed of the aircraft/helicopter in degrees per second. 
        /// This helps to limit the aircraft angular instability and improve FLCS controls.
        /// </summary>
        public float maxAngularSpeed = 200f;
        /// <summary>
        /// This is a measure of the Unity angular motion drag and is used mostly on helicopter models to make them easier to control. 
        /// Increasing the angular drag will help to slow down and balance the helicopter when cyclic inputs are supplied.
        /// </summary>
        public float rotationDrag = 2f;

        // ------------------------------------- Weights
        public float emptyWeight;
        public float munitionLoad;
        public float gunLoad;
        public float componentLoad;
        public float fuelLoad;
        public float totalWeight;
        public int munitionCount;

        // ------------------------------------- Pressure Breathing
        public PressureBreathing breathing = PressureBreathing.Off;
        public AudioClip breathingLoopSound;
        public AudioClip breathingEndSound;
        public float soundVolume = 0.75f;
        public AudioSource breathingLoopSource, breathingEndSource;
        public bool breathingState = false;
        public float breathingThreshold;

        // ------------------------------------- Sonic Boom
        /// <summary>
        /// This particle effect is used in conjunction with the audio clip above to simulate transition effects at high speeds on the aircraft
        /// </summary>
        public ParticleSystem sonicCone;
        ParticleSystem.EmissionModule sonicModule;
        /// <summary>
        /// This is the audio played when the aircraft speed exceeds Mach 1 to give feedback to the user about the transition process
        /// </summary>
        public AudioClip sonicBoom;
        public AudioSource boom;
        public bool boomPlayed, breathingActive;
        /// <summary>
        /// Maximum level of emission of the sonic cone particle effect
        /// </summary>
        public float sonicEmission = 10f;
        float sonicInput;

        // ---------------------------------- Variables
        [Header("Data")]
        public double α;
        public double β;
        public double αRad, βRad;
        public double V, Vkts, Vmph, Qdyn;
        public double p, q, r, z;
        public double u, v, w;
        public double ф, θ, ψ;
        public double δp, δq, δr;
        public double δu, δv, δw, δz, δV;
        public double δф, δθ, δψ;
        public double ρ, γ, m_height, n;
        public double m_Ax, m_Ay, m_Az;
        public double ωф; // Turn Rate
        public double Rф; // Turn Radius
        public Matrix3x3 ETB, BTE;
        public Vector m_wind, m_rates, m_bodyVelocity, m_gust, m_velocity;
        protected Vector3 _vels;

        public double _rp, _rq, _rr;
        public double _ru, _rv, _rw, _rz;
        public double _rф, _rθ, _rψ;

        float _prevG;
        public float smoothGSpeed = 0.04f;
        public float smoothRateSpeed = 0.05f;
        bool initialized;

        /// <summary>
        /// Creates and configures the required transforms, sets the rigidbody inertia values, configures the atmosphere component,
        /// sets the rigidbody angular drag and speed limits
        /// </summary>
        public void Initialize()
        {
            //---------------------------- COG
            GameObject core = new GameObject("_current_cog");
            core.transform.parent = transform;
            core.transform.localPosition = Vector3.zero;
            currentCOM = core.transform;
            if (emptyCenterOfMass == null) { emptyCenterOfMass = this.transform; }
            emptyCenterOfMass.name = "_empty_cog";

            //---------------------------- Mass Properties
            baseInertiaTensor = controller.m_rigidbody.inertiaTensor;
            inertiaTensor = new Vector3(Iyy, Izz, Ixx);
            if (tensorMode == TensorMode.Manual) { controller.m_rigidbody.inertiaTensor = inertiaTensor; }

            //---------------------------- Atmosphere
            m_atmosphere.Initialize();

            //---------------------------- Configure Marker Transform
            if (m_handler == null)
            {
                GameObject _hl = new GameObject("_handler");
                _hl.transform.parent = transform;
                _hl.transform.localPosition = Vector3.zero;
                _hl.transform.localScale = Vector3.one;
                m_handler = _hl.transform;
            }

            // -----------------------------Sonic Boom
            GameObject soundPoint = new GameObject("Sources"); soundPoint.transform.parent = this.transform; soundPoint.transform.localPosition = Vector3.zero;
            if (sonicCone != null && speedType == SpeedType.Supersonic) { sonicModule = sonicCone.emission; sonicModule.rateOverTime = 0f; }
            if (sonicBoom) { Handler.SetupSoundSource(soundPoint.transform, sonicBoom, "Sound Point", 150f, false, false, out boom); boom.volume = 1f; }
            if (breathing == PressureBreathing.Active)
            {
                if (breathingLoopSound) { Handler.SetupSoundSource(this.transform, breathingLoopSound, "Loop Sound Point", 50, true, false, out breathingLoopSource); breathingLoopSource.volume = soundVolume; }
                if (breathingEndSound) { Handler.SetupSoundSource(this.transform, breathingEndSound, "End Sound Point", 50, false, false, out breathingEndSource); breathingEndSource.volume = soundVolume; }
            }

            //---------------------------- Rotation
            controller.m_rigidbody.maxAngularVelocity = maxAngularSpeed * Mathf.Deg2Rad;
            ComputeCG();
            initialized = true;
        }
        /// <summary>
        /// Receives computation command from the controllers scripts and calls the in script compute functions
        /// </summary>
        /// <param name="dt"></param>
        public void Compute(double dt)
        {
            if (controller != null && initialized)
            {
                ComputeData(dt);
                ComputeCG();
                ComputeEffects();
            }
        }
        /// <summary>
        /// Collects linear, angular speeds from the rigidbody component. Filters them and calculates the linear and angular accelerations.
        /// </summary>
        /// <param name="_timestep"></param>
        protected void ComputeData(double _timestep)
        {
            Vector m_localOrientation = Transformation.UnityToEuler(controller.m_rigidbody.transform.eulerAngles);
            ф = m_localOrientation.x;
            θ = m_localOrientation.y;
            ψ = m_localOrientation.z;
            δV = (controller.m_rigidbody.linearVelocity - _vels).magnitude / _timestep;

            Vector m_Φ = controller.m_rigidbody.angularVelocity;
            Vector m_earthVelocity = Transformation.UnityToVector(controller.m_rigidbody.linearVelocity);
            Vector m_eulerRates = new Vector(-m_Φ.z, -m_Φ.x, m_Φ.y);
            Vector m_earthGust = m_atmosphere.m_gust;
            Vector m_earthWind = m_atmosphere.m_wind;
            ETB = Transformation.EarthToBody(θ, ф, ψ);
            BTE = Matrix3x3.Transpose(ETB);

            m_wind = ETB * m_earthWind;
            m_rates = ETB * m_eulerRates;
            m_bodyVelocity = ETB * m_earthVelocity;
            m_gust = ETB * m_earthGust;
            m_velocity = (m_wind + m_gust + m_bodyVelocity);

            u = m_velocity.x + Mathf.Epsilon;
            v = m_velocity.y + Mathf.Epsilon;
            w = m_velocity.z + Mathf.Epsilon;
            p = m_rates.x + Mathf.Epsilon;
            q = m_rates.y + Mathf.Epsilon;
            r = m_rates.z + Mathf.Epsilon;
            z = m_height = controller.m_rigidbody.transform.position.y;

            δp = (p - _rp) / _timestep; δq = (q - _rq) / _timestep; δr = (r - _rr) / _timestep;
            δu = (u - _ru) / _timestep; δv = (v - _rv) / _timestep; δw = (w - _rw) / _timestep;
            δф = (ф - _rф) / _timestep; δθ = (θ - _rθ) / _timestep; δψ = (ψ - _rψ) / _timestep;
            δz = (z - _rz) / _timestep;

            V = Math.Sqrt((u * u) + (v * v) + (w * w));
            Vkts = V * 1.94384;
            Vmph = V * 2.2369311202577;
            αRad = Math.Atan(w / u);
            βRad = Math.Asin(v / V);
            α = αRad * Mathf.Rad2Deg;
            β = βRad * Mathf.Rad2Deg;
            Qdyn = 0.5f * ρ * Math.Pow(V, 2);

            γ = θ - α;
            double γRad = γ * Mathf.Deg2Rad;
            m_atmosphere.Compute(m_height, V, 1);
            ρ = m_atmosphere.ρ;

            double Vr = (V * 1.94384);
            if (Vr < 1) { Vr = 1; }
            ωф = ((1091 * Math.Tan(ф * Mathf.Deg2Rad)) / Vr);
            if (double.IsNaN(ωф) || double.IsInfinity(ωф)) { ωф = 0.0; }

            if ((Math.Abs(ф) * Mathf.Rad2Deg) > 1)
            {
                if (Vr < 1) { Vr = 1; }
                Rф = ((Vr * Vr) / (11.26 * Math.Tan(ф * Mathf.Deg2Rad))) * 0.3048;
                if (double.IsNaN(Rф) || double.IsInfinity(Rф)) { Rф = 0.0; }
            }
            else { Rф = 0; }
            if (ωф < 0.08) { Rф = 0; }

            Vector3 localVelocity = controller.m_rigidbody.transform.InverseTransformDirection(controller.m_rigidbody.linearVelocity);
            Vector3 localAngularVelocity = transform.InverseTransformDirection(controller.m_rigidbody.angularVelocity);
            float turnRadius = (Mathf.Approximately(localAngularVelocity.x, 0.0f)) ? float.MaxValue : localVelocity.z / localAngularVelocity.x;
            float turnForce = (Mathf.Approximately(turnRadius, 0.0f)) ? 0.0f : (localVelocity.z * localVelocity.z) / turnRadius;
            float baseG = turnForce / -9.81f; baseG += transform.up.y * (Physics.gravity.y / -9.81f);
            float targetG = (baseG * smoothGSpeed) + (_prevG * (1.0f - smoothGSpeed)); _prevG = targetG;
            n = (float)Math.Round(targetG, 1);

            _rp = p; _rq = q; _rr = r;
            _ru = u; _rv = v; _rw = w;
            _rф = ф; _rθ = θ; _rψ = ψ;
            _rz = z;


            if (breathing == PressureBreathing.Active && controller.m_view != null && controller.m_view.cameraState == SilantroCamera.CameraState.Interior)
            {
                if (n > 4f) { breathingThreshold += Time.deltaTime; } else { breathingThreshold -= Time.deltaTime; }
                if (breathingThreshold < 0) { breathingThreshold = 0f; }
                if (breathingThreshold > 2f) { breathingThreshold = 2f; }


                if (breathingThreshold > 1.8f) { breathingActive = true; } else { breathingActive = false; }
                if (breathingLoopSound != null && breathingEndSound != null)
                {
                    if (breathingActive && breathingLoopSource != null && !breathingLoopSource.isPlaying) { ResetSound(); breathingLoopSource.Play(); }
                    if (!breathingActive && breathingLoopSound != null && breathingLoopSource.isPlaying && !breathingEndSource.isPlaying) { ResetSound(); breathingEndSource.Play(); }
                }
            }
            _vels = controller.m_rigidbody.linearVelocity;
        }
        /// <summary>
        /// 
        /// </summary>
        protected void ResetSound()
        {
            if (breathingEndSource != null && breathingEndSource.isPlaying) { breathingEndSource.Stop(); }
            if (breathingLoopSource != null && breathingLoopSource.isPlaying) { breathingLoopSource.Stop(); }
        }
        /// <summary>
        /// Calculates the effect of the fuel tank, payload, munition and gun drum weight and position on the rigidbody center of gravity
        /// </summary>
        protected void ComputeCG()
        {
            // ---------------------------------- Collect Weights
            emptyWeight = controller.emptyWeight;
            totalWeight = emptyWeight;
            fuelLoad = 0;
            componentLoad = 0f;
            munitionCount = 0;
            munitionLoad = 0;
            gunLoad = 0;

            centerOfMass = controller.m_rigidbody.transform.TransformDirection(emptyCenterOfMass.position) * emptyWeight;

            // ---------------------------------- Fuel Effect
            if (controller.m_fuelTanks.Length > 0)
            {
                foreach (SilantroTank tank in controller.m_fuelTanks)
                {
                    if (tank != null)
                    {
                        Vector3 tankPosition = tank.transform.position; fuelLoad += tank._currentAmount; totalWeight += tank._currentAmount;
                        centerOfMass += controller.m_rigidbody.transform.TransformDirection(tankPosition) * tank._currentAmount;
                    }
                }
            }
            // ---------------------------------- Load Effect
            if (controller.m_payload.Length > 0)
            {
                foreach (SilantroPayload component in controller.m_payload)
                {
                    if (component != null)
                    {
                        Vector3 loadPosition = component.transform.position;
                        componentLoad += component.m_metricWeight;
                        totalWeight += component.m_metricWeight;
                        centerOfMass += controller.m_rigidbody.transform.TransformDirection(loadPosition) * component.m_metricWeight;
                    }
                }
            }
            // ---------------------------------- Munitions
            if (controller.m_munitions.Length > 0)
            {
                foreach (SilantroMunition munition in controller.m_munitions)
                {
                    if (munition != null)
                    {
                        Vector3 loadPosition = munition.transform.position;
                        munitionLoad += munition.munitionWeight;
                        totalWeight += munition.munitionWeight;
                        centerOfMass += controller.m_rigidbody.transform.TransformDirection(loadPosition) * munition.munitionWeight;
                    }
                }
            }
            // ---------------------------------- Guns
            if (controller.m_guns.Length > 0)
            {
                foreach (SilantroGun gun in controller.m_guns)
                {
                    if (gun != null)
                    {
                        Vector3 loadPosition = gun.transform.position;
                        gunLoad += gun.drumWeight;
                        totalWeight += gun.drumWeight;
                        centerOfMass += controller.m_rigidbody.transform.TransformDirection(loadPosition) * gun.drumWeight;
                    }
                }
            }


            // ---------------------------------- Sum and Apply
            if (totalWeight > 0) { centerOfMass /= (totalWeight); } else { centerOfMass /= emptyWeight; }
            if (functionality == SystemType.Advanced)
            {
                currentCOM.position = controller.m_rigidbody.transform.InverseTransformDirection(centerOfMass);
                controller.m_rigidbody.centerOfMass = currentCOM.localPosition;
            }
            else { controller.m_rigidbody.centerOfMass = this.transform.localPosition; }
            controller.currentWeight = totalWeight;
            controller.m_rigidbody.mass = totalWeight;
            deviation = currentCOM.localPosition - emptyCenterOfMass.localPosition;
        }
        /// <summary>
        /// 
        /// </summary>
        protected void ComputeEffects()
        {
            float machSpeed = (float)m_atmosphere.M;
            if (sonicCone != null)
            {
                if (!sonicModule.enabled) { sonicModule = sonicCone.emission; }
                if (machSpeed > 0.95f) { sonicInput = machSpeed; if (sonicInput > 1) { sonicInput = 1f; } } else { sonicInput = Mathf.Lerp(sonicInput, 0, 0.8f * Time.deltaTime); }
                sonicModule.rateOverTime = sonicInput * sonicEmission;
            }

            if (boom != null && sonicBoom != null)
            {
                if (machSpeed > 0.98f && !boomPlayed) { boom.Play(); boomPlayed = true; }
                if (machSpeed < 0.95f && boomPlayed) { boomPlayed = false; }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmos()
        {
            // Gizmos.color = Color.blue;
            // if (emptyCenterOfMass != null)
            // {
            //     Gizmos.DrawSphere(emptyCenterOfMass.position, 0.2f); 
            //     Gizmos.DrawLine(emptyCenterOfMass.position, (emptyCenterOfMass.transform.up * 3f + emptyCenterOfMass.position));
            // }
            // if (emptyCenterOfMass == null && currentCOM == null)
            // {
            //     Gizmos.DrawSphere(this.transform.position, 0.2f); 
            //     Gizmos.DrawLine(this.transform.position, (this.transform.transform.up * 3f + this.transform.position));
            // }
            //
            // Gizmos.color = Color.red;
            // if (currentCOM != null)
            // {
            //     Gizmos.DrawSphere(currentCOM.position, 0.2f); 
            //     Gizmos.DrawLine(currentCOM.position, (currentCOM.transform.up * 3f + currentCOM.position));
            // }
        }
    }
    #endregion

    #region Editor

#if UNITY_EDITOR
    [CustomEditor(typeof(SilantroCore))]
    public class SilantroCoreEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1.0f, 0.40f, 0f);
        SilantroCore core;
        Controller m_controller;
        SerializedProperty atmosphere;


        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            core = (SilantroCore)target;
            atmosphere = serializedObject.FindProperty("m_atmosphere");
            m_controller = core.transform.gameObject.GetComponentInParent<Controller>();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector();
            serializedObject.Update();

            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("COG Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("functionality"), new GUIContent("Functionality"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tensorMode"), new GUIContent("Tensor Mode"));
            if (m_controller == null || m_controller != null && m_controller.m_type == Controller.VehicleType.Aircraft)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("speedType"), new GUIContent("Mode"));
            }
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("groundLayer"), new GUIContent("Ground Layer(s)"));



            if (core.functionality == SilantroCore.SystemType.Advanced)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("emptyCenterOfMass"), new GUIContent("Empty COG"));
                float xdeviation = 0f;
                float ydeviation = 0f;
                float zdeviation = 0f;
                if (core.currentCOM != null)
                {
                    Vector3 deviation = core.emptyCenterOfMass.position - core.currentCOM.position;
                    xdeviation = deviation.x;
                    ydeviation = deviation.y;
                    zdeviation = deviation.z;
                }
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Center of Gravity deviation", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Lateral", xdeviation.ToString("0.0000") + " m");
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Vertical", ydeviation.ToString("0.0000") + " m");
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Longitudinal", zdeviation.ToString("0.0000") + " m");
            }

            if (core.tensorMode == SilantroCore.TensorMode.Manual)
            {
                GUILayout.Space(15f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Inertia Tensor Data", MessageType.None);
                GUI.color = backgroundColor;

                GUILayout.Space(3f);
                GUI.color = Color.yellow;
                EditorGUILayout.HelpBox("Unity will still use the shape of the rigidbody in calculating its performance. You will notice shaking (fluctuations" +
                    " in the angular velocity) if Unity determines that the assigned tensor values are too low...even if they are the real life values", MessageType.Info);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Ixx"), new GUIContent("Ixx"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Iyy"), new GUIContent("Iyy"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Izz"), new GUIContent("Izz"));


                if (!Application.isPlaying)
                {
                    GUILayout.Space(6f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Assign aircraft rigidbody to view default Unity values, they will be replaced with the values above once the application starts", MessageType.Info);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sampleAircraft"), new GUIContent(" "));


                    if (core.sampleAircraft != null)
                    {
                        float sampleX = core.sampleAircraft.inertiaTensor.x;
                        float sampleY = core.sampleAircraft.inertiaTensor.y;
                        float sampleZ = core.sampleAircraft.inertiaTensor.z;
                        GUILayout.Space(5f);
                        EditorGUILayout.LabelField("Pitch Inertia", sampleX.ToString("0.0") + " kg/m2");
                        GUILayout.Space(3f);
                        EditorGUILayout.LabelField("Roll Inertia", sampleZ.ToString("0.0") + " kg/m2");
                        GUILayout.Space(3f);
                        EditorGUILayout.LabelField("Yaw Inertia", sampleY.ToString("0.0") + " kg/m2");
                    }
                }
            }

            if (m_controller == null || m_controller != null && m_controller.m_type == Controller.VehicleType.Aircraft)
            {

                if (core.speedType == SilantroCore.SpeedType.Supersonic)
                {
                    GUILayout.Space(10f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Effect Configuration", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sonicBoom"), new GUIContent("Sonic Boom"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sonicCone"), new GUIContent("Sonic Cone"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sonicEmission"), new GUIContent("Maximum Emission"));
                }


                GUILayout.Space(15f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Pressure Breathing", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("breathing"), new GUIContent("State"));

                if (core.breathing == SilantroCore.PressureBreathing.Active)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("breathingLoopSound"), new GUIContent("Breathing Loop"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("breathingEndSound"), new GUIContent("Breathing End"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("soundVolume"), new GUIContent("Sound Volume"));
                }
            }

            GUILayout.Space(15f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Performance Data", MessageType.None);
            GUI.color = backgroundColor;

            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Limits", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAngularSpeed"), new GUIContent("Angular Speed Limit (°/s)"));

            if (m_controller == null || m_controller != null && m_controller.m_type == Controller.VehicleType.Helicopter)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationDrag"), new GUIContent("Angular Drag"));
            }


            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Basic", MessageType.None);
            GUI.color = backgroundColor;


            GUILayout.Space(2f);
            EditorGUILayout.LabelField("IAS", (core.V * Constants.ms2knots).ToString("0.00") + " knots");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Acceleration", (core.δu * Constants.ms2knots).ToString("0.00") + " (knots)/s");

            if (m_controller == null || m_controller != null && m_controller.m_type == Controller.VehicleType.Aircraft)
            {
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Mach", core.m_atmosphere.M.ToString("0.000"));
            }

            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Altitude", (core.z * Constants.m2ft).ToString("0.0") + " feet");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Climb Rate", (core.δz * Constants.toFtMin).ToString("0.0") + " ft/min");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Heading", core.ψ.ToString("0.0") + " °");


            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Advanced", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("G-Load", core.n.ToString("0.0"));
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Pitch Rate", (core.q * Mathf.Rad2Deg).ToString("0.00") + " °/s");
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Roll Rate", (core.p * Mathf.Rad2Deg).ToString("0.00") + " °/s");
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Yaw Rate", (core.r * Mathf.Rad2Deg).ToString("0.00") + " °/s");
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Turn Rate", core.ωф.ToString("0.00") + " °/s");


            GUILayout.Space(3f);
            EditorGUILayout.LabelField("α", core.α.ToString("0.0") + " °");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("β", core.β.ToString("0.0") + " °");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("γ", core.γ.ToString("0.0") + " °");


            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Low Pass Filter", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothGSpeed"), new GUIContent("G Smooth Speed"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothRateSpeed"), new GUIContent("Rate Smooth Speed"));

            GUILayout.Space(20f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Atmosphere", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(atmosphere.FindPropertyRelative("m_type"), new GUIContent("Type"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(atmosphere.FindPropertyRelative("m_shearMode"), new GUIContent("Shear Mode"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(atmosphere.FindPropertyRelative("m_windState"), new GUIContent("Wind Mode"));
            if (core.m_atmosphere.m_windState == FAtmosphere.State.Internal)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(atmosphere.FindPropertyRelative("m_windSpeed"), new GUIContent("Speed"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(atmosphere.FindPropertyRelative("m_ψw"), new GUIContent("Direction"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(atmosphere.FindPropertyRelative("m_turbulence"), new GUIContent("Turbulence"));
            }



            GUILayout.Space(3f);
            EditorGUILayout.LabelField("ρ", core.m_atmosphere.ρ.ToString("0.0000") + " kg/m3");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("OAT", core.m_atmosphere.T.ToString("0.00") + " °K");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Ps", core.m_atmosphere.Ps.ToString("0.00") + " Pa");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("μ", core.m_atmosphere.μ.ToString("0.00000"));
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Qc", core.m_atmosphere.qc.ToString("0.000") + " Pa");

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    #endregion
}
