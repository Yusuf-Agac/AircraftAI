using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Oyedoyin.Common
{
    #region Component

    /// <summary>
    /// 
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SilantroLever : MonoBehaviour
    {
        public enum LeverMode { RotateOnly, SlideOnly, SlideAndRotate }
        public enum LeverType { ControlStick, ControlYoke, SingleAxis, Switch }
        public enum AxisState { Normal, Inverted }
        public enum LeverAction { SelfCentering, NonCentering }
        public enum RotationAxis { X, Y, Z }

        public LeverMode m_mode = LeverMode.RotateOnly;
        public LeverType leverType = LeverType.ControlStick;
        public LeverAction leverAction = LeverAction.NonCentering;
        public RotationAxis rollAxis = RotationAxis.X;
        public RotationAxis leverAxis = RotationAxis.X;
        public RotationAxis pitchAxis = RotationAxis.X;
        public AxisState pitchAxisState = AxisState.Normal;
        public AxisState rollAxisState = AxisState.Normal;
        public AxisState leverAxisState = AxisState.Normal;

        // Connections
        public Transform m_hinge;
        private SilantroHand m_controller;

        // Properties
        public bool leverHeld;
        public float snapSpeed = 10f;
        public float maximumDeflection = 20f;
        public float maximumPitchDeflection = 20f, maximumRollDeflection = 20f;
        public float maximumMovement = 5;

        private Vector3 m_baseLeverPosition;
        private Quaternion m_baseLeverRotation;

        // Output
        public float leverOutput;
        public float pitchOutput, rollOutput;
        private Vector2 angle, value;
        private Vector2 deflectionLimit = new Vector2(30, 30);
        private Vector3 handPosition;
        private Vector3 localHandPosition;
        Vector3 m_yokeAxisRoll;

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            if (leverType == LeverType.ControlStick || leverType == LeverType.ControlYoke)
            { deflectionLimit = new Vector2(maximumRollDeflection, maximumPitchDeflection); }
            if (leverType == LeverType.SingleAxis)
            {
                if (leverAxis == RotationAxis.X) { deflectionLimit = new Vector2(maximumDeflection / 2, 0); }
                if (leverAxis == RotationAxis.Y) { deflectionLimit = new Vector2(0, maximumDeflection / 2); }
            }
            if (leverType == LeverType.Switch)
            {
                if (leverAxis == RotationAxis.X) { deflectionLimit = new Vector2(maximumDeflection / 2, 0); }
                if (leverAxis == RotationAxis.Y) { deflectionLimit = new Vector2(0, maximumDeflection / 2); }
            }

            if (m_hinge != null)
            {
                m_baseLeverPosition = transform.parent.InverseTransformPoint(m_hinge.position);
                m_baseLeverRotation = m_hinge.localRotation;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("PlayerHand"))
            {
                if (m_controller == null) { m_controller = other.GetComponent<SilantroHand>(); }
                // Input State
                if (m_controller != null)
                {
                    if (m_controller.triggerValue > 0.9f && m_controller.gripValue > 0.9f)
                    {
                        leverHeld = true;
                    }
                    else
                    {
                        leverHeld = false;
                    }
                }
                //Hand Data
                if (leverHeld) { handPosition = other.transform.position; }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("PlayerHand"))
            {
                leverHeld = false;
                m_controller = null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void AnalyseLeverState()
        {
            localHandPosition = transform.InverseTransformPoint(handPosition);

            if (leverHeld)
            {
                if (m_mode == LeverMode.SlideOnly)
                {
                    float m_limit = maximumMovement * 0.01f;
                    value.x = localHandPosition.z;
                    value.x = Mathf.Clamp(value.x, -m_limit, m_limit);
                    Vector3 m_worldPosition = transform.parent.TransformPoint(m_baseLeverPosition);
                    Vector3 m_position = m_worldPosition + (m_hinge.forward * value.x);
                    m_hinge.position = m_position;
                }
                else if (m_mode == LeverMode.RotateOnly)
                {
                    angle.x = Vector2.SignedAngle(new Vector2(localHandPosition.y, localHandPosition.z), Vector2.up);
                    angle.y = Vector2.SignedAngle(new Vector2(localHandPosition.x, localHandPosition.z), Vector2.up);
                    angle = new Vector2(Mathf.Clamp(angle.x, -deflectionLimit.x, deflectionLimit.x), Mathf.Clamp(angle.y, -deflectionLimit.y, deflectionLimit.y));
                    value = new Vector2(angle.x / (deflectionLimit.x + Mathf.Epsilon), angle.y / (deflectionLimit.y + Mathf.Epsilon));
                    m_hinge.localRotation = Quaternion.LookRotation(Vector3.SlerpUnclamped(Vector3.SlerpUnclamped(new Vector3(-1, -1, 1),
                        new Vector3(-1, 1, 1), value.x * deflectionLimit.x / 90 + .5f), Vector3.SlerpUnclamped(new Vector3(1, -1, 1),
                        new Vector3(1, 1, 1), value.x * deflectionLimit.x / 90 + .5f), value.y * deflectionLimit.y / 90 + .5f), Vector3.up);
                }
                else if (m_mode == LeverMode.SlideAndRotate && leverType == LeverType.ControlYoke)
                {
                    // Roll Axis 
                    float m_hf = 1;
                    if (m_controller != null && m_controller.m_handType == SilantroHand.HandType.Right) { m_hf = 1; }
                    if (m_controller != null && m_controller.m_handType == SilantroHand.HandType.Left) { m_hf = -1; }
                    angle.x = Vector2.SignedAngle(new Vector2(localHandPosition.x, Mathf.Abs(localHandPosition.y)), Vector2.up);
                    float rollInput = angle.x / deflectionLimit.x + Mathf.Epsilon;
                    rollInput = Mathf.Clamp(rollInput, -1, 1);
                    Quaternion rollEffect = Quaternion.AngleAxis(m_hf * rollInput * deflectionLimit.x, m_yokeAxisRoll);
                    m_hinge.localRotation = m_baseLeverRotation * rollEffect;

                    // Pitch Axis
                    float m_limit = maximumMovement * 0.01f;
                    value.x = localHandPosition.z;
                    value.x = Mathf.Clamp(value.x, -m_limit, m_limit);
                    Vector3 m_worldPosition = transform.parent.TransformPoint(m_baseLeverPosition);
                    Vector3 m_position = m_worldPosition + (m_hinge.forward * value.x);
                    m_hinge.position = m_position;
                    value.y = rollInput;
                }
            }


            //Reset Core
            if (leverAction == LeverAction.SelfCentering && !leverHeld)
            {
                if (m_mode == LeverMode.RotateOnly)
                {
                    value = Vector2.MoveTowards(value, Vector2.zero, Time.deltaTime * snapSpeed);
                    m_hinge.localRotation = Quaternion.LookRotation(Vector3.SlerpUnclamped(Vector3.SlerpUnclamped(
                        new Vector3(-1, -1, 1), new Vector3(-1, 1, 1), value.x * deflectionLimit.x / 90 + .5f),
                        Vector3.SlerpUnclamped(new Vector3(1, -1, 1), new Vector3(1, 1, 1), value.x * deflectionLimit.x / 90 + .5f),
                        value.y * deflectionLimit.y / 90 + .5f), Vector3.up);
                }

                if (m_mode == LeverMode.SlideOnly)
                {
                    value.x = Mathf.MoveTowards(value.x, 0, Time.deltaTime * snapSpeed * 0.01f);
                    Vector3 m_worldPosition = transform.parent.TransformPoint(m_baseLeverPosition);
                    Vector3 m_position = m_worldPosition + (m_hinge.forward * value.x);
                    m_hinge.position = m_position;
                }

                if (m_mode == LeverMode.SlideAndRotate && leverType == LeverType.ControlYoke)
                {
                    // Pitch Axis
                    value.x = Mathf.MoveTowards(value.x, 0, Time.deltaTime * snapSpeed * 0.01f);
                    Vector3 m_worldPosition = transform.parent.TransformPoint(m_baseLeverPosition);
                    Vector3 m_position = m_worldPosition + (m_hinge.forward * value.x);
                    m_hinge.position = m_position;

                    // Roll Axis
                    value.y = Mathf.MoveTowards(value.y, 0, Time.deltaTime * snapSpeed * 0.25f);
                    Quaternion rollEffect = Quaternion.AngleAxis(value.y * deflectionLimit.x, m_yokeAxisRoll);
                    m_hinge.localRotation = m_baseLeverRotation * rollEffect;
                }
            }

            if (m_controller != null && m_controller.triggerValue < 0.9f && m_controller.gripValue < 0.9f && leverHeld) { leverHeld = false; }
        }
        /// <summary>
        /// 
        /// </summary>
        private void AnalyseLeverInput()
        {
            if (m_mode == LeverMode.RotateOnly)
            {
                if (leverType == LeverType.ControlStick)
                {
                    if (pitchAxisState == AxisState.Normal) { pitchOutput = -value.y; } else { pitchOutput = value.y; }
                    if (rollAxisState == AxisState.Normal) { rollOutput = -value.x; } else { rollOutput = value.x; }
                }
                if (leverType == LeverType.SingleAxis)
                {
                    if (leverAxis == RotationAxis.X) { leverOutput = leverAxisState == AxisState.Inverted ? 1 - ((-value.x + 1) / 2) : (-value.x + 1) / 2; }
                    if (leverAxis == RotationAxis.Y) { leverOutput = leverAxisState == AxisState.Inverted ? 1 - ((-value.y + 1) / 2) : (-value.y + 1) / 2; }
                }
            }
            if (m_mode == LeverMode.SlideOnly)
            {
                float m_limit = maximumMovement * 0.01f;
                float m_value = value.x / m_limit;
                leverOutput = leverAxisState == AxisState.Inverted ? 1 - ((m_value + 1) / 2) : (m_value + 1) / 2;
            }
            if (m_mode == LeverMode.SlideAndRotate && leverType == LeverType.ControlYoke)
            {
                float m_limit = maximumMovement * 0.01f;
                float m_value = value.x / m_limit;
                if (pitchAxisState == AxisState.Normal) { pitchOutput = m_value; } else { pitchOutput = -m_value; }
                if (rollAxisState == AxisState.Normal) { rollOutput = value.y; } else { rollOutput = -value.y; }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Compute()
        {
            AnalyseLeverState();
            AnalyseLeverInput();
        }
    }

    #endregion

    #region Editor
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroLever))]
    public class SilantroLeverEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1, 0.4f, 0);
        SilantroLever lever;

        /// <summary>
        /// 
        /// </summary>
        void OnEnable()
        {
            lever = (SilantroLever)target;
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
            EditorGUILayout.HelpBox("State", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("leverType"), new GUIContent("Type"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_mode"), new GUIContent("Mode"));
            if (lever.leverType == SilantroLever.LeverType.ControlStick ||
               lever.leverType == SilantroLever.LeverType.ControlYoke)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("leverAction"), new GUIContent("Center Mode"));
                if (lever.leverAction == SilantroLever.LeverAction.SelfCentering)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("snapSpeed"), new GUIContent("Recenter Speed"));
                }
            }

            GUILayout.Space(10f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Rotation Hinge", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_hinge"), new GUIContent("Hinge"));

            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Rotation Config", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            if(lever.leverType == SilantroLever.LeverType.ControlStick || 
                lever.leverType == SilantroLever.LeverType.ControlYoke)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchAxis"), new GUIContent("Pitch Axis"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pitchAxisState"), new GUIContent("Pitch State"));

                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rollAxis"), new GUIContent("Roll Axis"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("rollAxisState"), new GUIContent("Roll State"));
            }
            else
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("leverAxis"), new GUIContent("Lever Axis"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("leverAxisState"), new GUIContent("Lever State"));

            }

            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Deflection Config", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            if(lever.leverType == SilantroLever.LeverType.ControlYoke)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumRollDeflection"), new GUIContent("Roll Deflection Limit"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumMovement"), new GUIContent("Pitch Movement Limit"));
            }
            else if(lever.leverType == SilantroLever.LeverType.ControlStick)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumRollDeflection"), new GUIContent("Roll Deflection Limit"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumPitchDeflection"), new GUIContent("Pitch Deflection Limit"));
            }
            else
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumDeflection"), new GUIContent("Deflection Limit"));
            }

            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Output", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            if (lever.leverType == SilantroLever.LeverType.ControlStick ||
               lever.leverType == SilantroLever.LeverType.ControlYoke)
            {
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Pitch Output", lever.pitchOutput.ToString("0.0000"));
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Roll Output", lever.rollOutput.ToString("0.0000"));
            }
            else
            {
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Lever Output", lever.leverOutput.ToString("0.0000"));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
#endregion
}

