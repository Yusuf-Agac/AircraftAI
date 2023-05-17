using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Oyedoyin.Common.Misc;
using System.Collections.Generic;

namespace Oyedoyin.Common
{
    #region Component
    /// <summary>
    /// Handles the movement of actuator surfaces based on the imported animation sequence	
    /// </summary>
    [HelpURL("https://youtu.be/cwLsm8w8tGg")]
    public class SilantroActuator : MonoBehaviour
    {
        public enum ActuatorState { Engaged, Disengaged }
        public ActuatorState actuatorState = ActuatorState.Disengaged;
        public enum ActuatorMode { DefaultOpen, DefaultClose }
        public ActuatorMode actuatorMode = ActuatorMode.DefaultClose;
        public enum ActuatorType { LandingGear, Canopy, SpeedBrake, SwingWings, EngineNozzle, Door, ControlSurface, Custom, LiftSystem, GunCover }
        public ActuatorType actuatorType = ActuatorType.LandingGear;


        public enum SoundType { Simple, Complex }
        public SoundType soundType = SoundType.Complex;
        public AudioClip EngageClip, DisengageClip;
        public AudioClip EngageLoopClip, EngageEndClip;
        public AudioSource EngageLoopPoint, EngageEndPoint;
        public AudioSource actuationSoundPoint;


        public Animator actuatorAnimator;
        public string animationName = "Engine Nozzle";
        public int animationLayer = 0;
        public float currentActuationLevel, targetActuationLevel, actuationSpeed = 0.2f, multiplier = 1;
        public bool invertMotion, engaged;

        public bool initialized;
        public bool evaluate;
        public List<SilantroBulb> landingBulbs;


        #region Internal Functions

        /// <summary>
        /// For testing purposes only
        /// </summary>
        private void Start() { if (evaluate) { Initialize(); } }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            if (actuatorAnimator != null)
            {
                if (actuatorMode == ActuatorMode.DefaultOpen) { actuatorState = ActuatorState.Engaged; }
                if (EngageLoopClip) { Handler.SetupSoundSource(this.transform, EngageLoopClip, "Loop Point", 150f, true, false, out EngageLoopPoint); EngageLoopPoint.volume = 1f; }
                if (EngageEndClip) { Handler.SetupSoundSource(this.transform, EngageEndClip, "Clip Point", 150f, false, false, out EngageEndPoint); EngageEndPoint.volume = 1f; }
                if (EngageClip) { Handler.SetupSoundSource(this.transform, EngageClip, "Clip Point", 150f, false, false, out actuationSoundPoint); actuationSoundPoint.volume = 1f; }
                initialized = true;
            }
            else { Debug.LogError("Animator for " + transform.name + " has not been assigned"); return; }
            if (actuatorType != ActuatorType.EngineNozzle && actuatorType != ActuatorType.ControlSurface)
            {
                if (EngageEndClip == null && EngageClip == null && EngageLoopClip == null && DisengageClip == null) { Debug.LogError("Audio Clips for " + transform.name + " has not been assigned"); return; }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        private void AnalyseSound(float target)
        {
            if (target == 0) { if (currentActuationLevel > 0.05f) { if (!EngageLoopPoint.isPlaying) { EngageLoopPoint.Play(); } } else { EngageLoopPoint.Stop(); EngageEndPoint.PlayOneShot(EngageEndClip); engaged = false; AnalyseState(0); } }
            if (target == 1) { if (currentActuationLevel < 0.95f) { if (!EngageLoopPoint.isPlaying) { EngageLoopPoint.Play(); } } else { EngageLoopPoint.Stop(); EngageEndPoint.PlayOneShot(EngageEndClip); engaged = false; AnalyseState(1); } }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="set"></param>
        private void AnalyseState(int set)
        {
            if (actuatorMode == ActuatorMode.DefaultClose)
            {
                if (set == 0) { actuatorState = ActuatorState.Disengaged; if (actuatorType == ActuatorType.LandingGear && landingBulbs != null) { foreach (SilantroBulb bulb in landingBulbs) { if (bulb.gameObject.activeSelf) { bulb.gameObject.SetActive(false); } } } }
                if (set == 1) { actuatorState = ActuatorState.Engaged; if (actuatorType == ActuatorType.LandingGear && landingBulbs != null) { foreach (SilantroBulb bulb in landingBulbs) { if (bulb.gameObject.activeSelf) { bulb.gameObject.SetActive(true); } } } }
            }
            else
            {
                if (set == 0) { actuatorState = ActuatorState.Engaged; if (actuatorType == ActuatorType.LandingGear && landingBulbs != null) { foreach (SilantroBulb bulb in landingBulbs) { if (bulb.gameObject.activeSelf) { bulb.gameObject.SetActive(true); } } } }
                if (set == 1) { actuatorState = ActuatorState.Disengaged; if (actuatorType == ActuatorType.LandingGear && landingBulbs != null) { foreach (SilantroBulb bulb in landingBulbs) { if (bulb.gameObject.activeSelf) { bulb.gameObject.SetActive(false); } } } }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Compute(float _timestep)
        {
            if (initialized)
            {
                //ADJUST CONTROL VARIABLE
                if (currentActuationLevel != targetActuationLevel) { currentActuationLevel = Mathf.MoveTowards(currentActuationLevel, targetActuationLevel, _timestep * actuationSpeed * multiplier); }

                //--------------------------------------ANIMATE
                if (actuatorAnimator != null) { actuatorAnimator.Play(animationName, animationLayer, currentActuationLevel); }

                //--------------------------------------SOUND
                if (actuatorType != ActuatorType.EngineNozzle && actuatorType != ActuatorType.ControlSurface) { if (engaged) { AnalyseSound(targetActuationLevel); } }
            }
        }

        #endregion

        #region Call Functions
        /// <summary>
        /// 
        /// </summary>
        public void EngageActuator()
        {
            if (initialized && !engaged)
            {
                if (soundType == SoundType.Complex) { if (EngageLoopPoint.isPlaying) { EngageLoopPoint.Stop(); } if (EngageEndPoint.isPlaying) { EngageEndPoint.Stop(); } }
                if (invertMotion)
                {
                    targetActuationLevel = 1; if (currentActuationLevel < 0.01f)
                    {
                        engaged = true; if (soundType == SoundType.Simple)
                        {
                            if (EngageClip)
                            {
                                actuationSoundPoint.PlayOneShot(EngageClip);
                            }
                        }
                    }
                }
                else
                {
                    targetActuationLevel = 0; if (currentActuationLevel > 0.99f)
                    {
                        engaged = true; if (soundType == SoundType.Simple) { if (EngageClip) { actuationSoundPoint.PlayOneShot(EngageClip); } }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void DisengageActuator()
        {
            if (initialized && !engaged)
            {
                if (soundType == SoundType.Complex) { if (EngageLoopPoint.isPlaying) { EngageLoopPoint.Stop(); } if (EngageEndPoint.isPlaying) { EngageEndPoint.Stop(); } }
                if (invertMotion)
                {
                    targetActuationLevel = 0; if (currentActuationLevel > 0.99f)
                    {
                        engaged = true; if (soundType == SoundType.Simple)
                        {
                            if (DisengageClip)
                            {
                                actuationSoundPoint.PlayOneShot(DisengageClip);
                            }
                        }
                    }
                }
                else
                {
                    targetActuationLevel = 1; if (currentActuationLevel < 0.01f)
                    {
                        engaged = true;
                        if (soundType == SoundType.Simple) { if (DisengageClip) { actuationSoundPoint.PlayOneShot(DisengageClip); } }
                    }
                }
                if (actuatorType == ActuatorType.LandingGear && landingBulbs != null) { foreach (SilantroBulb bulb in landingBulbs) { if (bulb.state == SilantroBulb.CurrentState.On) { bulb.SwitchOff(); } } }
            }
        }

        #endregion
    }
    #endregion

    #region Editor
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroActuator))]
    public class SilantroActuatorEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1, 0.4f, 0);
        SilantroActuator actuator;


        //------------------------------------------------------------------------
        private SerializedProperty animator;
        private SerializedProperty animationLayer;
        private SerializedProperty animationName;
        private SerializedProperty actuationSpeed;
        private SerializedProperty invertMotion;
        private SerializedProperty dragFactor;
        private SerializedProperty generatesDrag;
        private SerializedProperty invertDrag;
        private SerializedProperty soundType;


        private SerializedProperty engageClip;
        private SerializedProperty disengageClip;
        private SerializedProperty engageLoopClip;
        private SerializedProperty engageEndClip;
        private SerializedProperty type;
        private SerializedProperty Mode;

        void OnEnable()
        {
            actuator = (SilantroActuator)target;

            animator = serializedObject.FindProperty("actuatorAnimator");
            animationLayer = serializedObject.FindProperty("animationLayer");
            animationName = serializedObject.FindProperty("animationName");
            actuationSpeed = serializedObject.FindProperty("actuationSpeed");
            invertMotion = serializedObject.FindProperty("invertMotion");
            dragFactor = serializedObject.FindProperty("dragFactor");
            generatesDrag = serializedObject.FindProperty("generatesDrag");
            invertDrag = serializedObject.FindProperty("invertDrag");
            soundType = serializedObject.FindProperty("soundType");

            engageClip = serializedObject.FindProperty("EngageClip");
            disengageClip = serializedObject.FindProperty("disengageClip");
            engageLoopClip = serializedObject.FindProperty("EngageLoopClip");
            engageEndClip = serializedObject.FindProperty("EngageEndClip");
            type = serializedObject.FindProperty("actuatorType");
            Mode = serializedObject.FindProperty("actuatorMode");
        }

        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector();
            serializedObject.Update();

            if (actuator.actuatorType != SilantroActuator.ActuatorType.EngineNozzle && actuator.actuatorType != SilantroActuator.ActuatorType.ControlSurface)
            {
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("State", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                if (actuator.evaluate) { if (GUILayout.Button("Finish Evaluation")) { actuator.evaluate = false; } silantroColor = Color.red; }
                if (!actuator.evaluate) { if (GUILayout.Button("Evaluate")) { actuator.evaluate = true; } silantroColor = new Color(1, 0.4f, 0); }
                if (actuator.evaluate)
                {
                    GUILayout.Space(5f);
                    if (GUILayout.Button("Engage")) { actuator.EngageActuator(); }
                    GUILayout.Space(2f);
                    if (GUILayout.Button("DisEngage")) { actuator.DisengageActuator(); }
                }
            }


            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Type", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(type);

            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Animation Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(animator);
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(animationLayer);
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(animationName);

            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Actuation Configuration", MessageType.None);
            GUI.color = backgroundColor;
            if (actuator.actuatorType != SilantroActuator.ActuatorType.EngineNozzle && actuator.actuatorType != SilantroActuator.ActuatorType.ControlSurface)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(Mode);
            }
            GUILayout.Space(3f);
            actuationSpeed.floatValue = EditorGUILayout.Slider("Actuation Speed", actuationSpeed.floatValue, 0f, 1f);


            if (actuator.actuatorType == SilantroActuator.ActuatorType.Custom || actuator.actuatorType == SilantroActuator.ActuatorType.GunCover)
            {
                GUILayout.Space(3f);
                serializedObject.FindProperty("multiplier").floatValue = EditorGUILayout.Slider("Multiplier", serializedObject.FindProperty("multiplier").floatValue, 0f, 10f);
            }

            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Actuation level", (actuator.currentActuationLevel * 100f).ToString("0.00") + " %");
            if (actuator.actuatorType != SilantroActuator.ActuatorType.EngineNozzle && actuator.actuatorType != SilantroActuator.ActuatorType.ControlSurface)
            {
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Actuation State", actuator.actuatorState.ToString());
            }
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(invertMotion);

            if (actuator.actuatorType != SilantroActuator.ActuatorType.EngineNozzle && actuator.actuatorType != SilantroActuator.ActuatorType.ControlSurface)
            {
                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Sound Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(soundType);
                GUILayout.Space(5f);
                if (actuator.soundType == SilantroActuator.SoundType.Simple)
                {
                    EditorGUILayout.PropertyField(engageClip);
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(disengageClip);
                }
                if (actuator.soundType == SilantroActuator.SoundType.Complex)
                {
                    EditorGUILayout.PropertyField(engageLoopClip);
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(engageEndClip);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
#endregion
}