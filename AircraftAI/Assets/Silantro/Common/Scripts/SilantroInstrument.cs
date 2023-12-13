using System;
using UnityEngine;
using Oyedoyin.Common.Misc;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Oyedoyin.Common
{
    #region Component
    /// <summary>
    /// Use:	Handles the movement/rotation of control levers and handles
    /// </summary>
    public class SilantroInstrument : MonoBehaviour
    {
        public enum Type { Lever, Dial, Readout, HSI }

        public Controller m_controller;
        public Type m_type = Type.Lever;
        public Lever m_lever;
        public Dial m_dial;
        public Readout m_readout;
        public HSI m_hsi;

        public float m_roll;
        public float m_pitch;
        public float m_yaw;
        public float m_throttle;
        public float m_prop;
        public float m_mixture;

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class Lever
        {
            public enum LeverType { Stick, Throttle, Pedal, Flaps, GearIndicator, Mixture, PropellerPitch, Collective }
            public enum StickType { Joystick, Yoke, ControlColumn, }
            public enum ThrottleMode { Deflection, Sliding }
            public enum PedalType { Sliding, Hinged }
            public enum PedalMode { Individual, Combined }

            public Controller m_controller;
            public SilantroInstrument m_base;
            public LeverType m_leverType = LeverType.Stick;
            public StickType m_stickType = StickType.Joystick;
            public ThrottleMode m_throttleMode = ThrottleMode.Deflection;

            // ---------------------------------------------- Connections
            public Transform m_lever;
            public Transform m_yoke;

            // ---------------------------------------------- Limits
            // Joystick
            public float m_rollLimit = 20;
            public float m_pitchLimit = 5;
            // Base Lever
            public float m_deflectionLimit = 30;
            public float m_slidingLimit = 10;
            // Pedals
            public float m_pedalDeflectionLimit = 30;
            public float m_pedalSlidingLimit = 10;
            float _leverInput = 0;


            // ---------------------------------------------- Control Properties
            public Vector3 m_rollAxisDeflection;
            public Vector3 m_pitchAxisDeflection;
            public RotationAxis m_rotationAxisRoll = RotationAxis.X;
            public RotationDirection m_directionRoll = RotationDirection.CW;
            public RotationAxis m_rotationAxisPitch = RotationAxis.X;
            public RotationDirection m_directionPitch = RotationDirection.CW;

            // Base Lever Stuff
            public RotationAxis m_rotationAxis = RotationAxis.X;
            public RotationDirection m_direction = RotationDirection.CW;
            public MovementAxis m_movementAxis;
            public MovementDirection m_movementDirection;
            public Vector3 m_axisDeflection;
            Quaternion m_baseLeverRotation;
            Quaternion m_baseYokeRotation;
            Vector3 m_baseLeverPosition;

            // Pedals
            public PedalType m_pedalType = PedalType.Hinged;
            public PedalMode m_pedalMode = PedalMode.Combined;
            public Transform m_leftPedal;
            public Transform m_rightPedal;
            public RotationAxis m_rightRotationAxis = RotationAxis.X;
            public RotationAxis m_leftRotationAxis = RotationAxis.X;
            public RotationDirection m_rightDirection = RotationDirection.CW;
            public RotationDirection m_leftDirection = RotationDirection.CCW;
            Vector3 m_rightAxis, m_leftAxis;
            Vector3 m_baseRightPosition, m_baseLeftPosition;
            Quaternion m_baseRightRotation;
            Quaternion m_baseLeftRotation;


            /// <summary>
            /// 
            /// </summary>
            public void Initialize()
            {
                // Yaw Pedals
                if (m_leverType == LeverType.Pedal)
                {
                    if (m_leftPedal != null)
                    {
                        m_baseLeftRotation = m_leftPedal.localRotation;
                        m_baseLeftPosition = m_leftPedal.localPosition;
                        m_leftAxis = Handler.EstimateModelProperties(m_leftDirection.ToString(), m_leftRotationAxis.ToString());
                    }
                    if (m_rightPedal != null)
                    {
                        m_baseRightRotation = m_rightPedal.localRotation;
                        m_baseRightPosition = m_rightPedal.localPosition;
                        m_rightAxis = Handler.EstimateModelProperties(m_rightDirection.ToString(), m_rightRotationAxis.ToString());
                    }
                }
                // Base Levers
                if (m_lever != null)
                {
                    m_baseLeverPosition = m_controller.transform.InverseTransformPoint(m_lever.position);
                    m_baseLeverRotation = m_lever.localRotation;
                    m_axisDeflection = Handler.EstimateModelProperties(m_direction.ToString(), m_rotationAxis.ToString());
                    m_rollAxisDeflection = Handler.EstimateModelProperties(m_directionRoll.ToString(), m_rotationAxisRoll.ToString());
                    m_pitchAxisDeflection = Handler.EstimateModelProperties(m_directionPitch.ToString(), m_rotationAxisPitch.ToString());
                }
                if (m_yoke != null) { m_baseYokeRotation = m_yoke.localRotation; }
            }
            /// <summary>
            /// 
            /// </summary>
            public void Compute(float _input)
            {
                if (m_controller != null)
                {
                    // Control Stick
                    if (m_leverType == LeverType.Stick)
                    {
                        // Yoke
                        if (m_stickType == StickType.Yoke)
                        {
                            if (m_lever != null)
                            {
                                Quaternion rollEffect = Quaternion.AngleAxis(m_base.m_roll * m_rollLimit, m_rollAxisDeflection);
                                m_lever.localRotation = m_baseLeverRotation * rollEffect;

                                Vector3 m_rgt = m_lever.right;
                                if (m_movementAxis == MovementAxis.Forward) { m_rgt = m_lever.forward; }
                                if (m_movementAxis == MovementAxis.Right) { m_rgt = m_lever.right; }
                                if (m_movementAxis == MovementAxis.Up) { m_rgt = m_lever.up; }
                                if (m_movementDirection == MovementDirection.Inverted) { m_rgt *= -1; }

                                Vector3 m_worldPosition = m_controller.transform.TransformPoint(m_baseLeverPosition);
                                Vector3 m_position = m_worldPosition + (0.01f * m_base.m_pitch * m_pitchLimit * m_rgt);
                                m_lever.position = m_position;
                            }
                        }
                        // Control Column
                        if (m_stickType == StickType.ControlColumn)
                        {
                            if (m_yoke != null)
                            {
                                Quaternion rollEffect = Quaternion.AngleAxis(m_base.m_roll * m_rollLimit, m_rollAxisDeflection);
                                m_yoke.localRotation = m_baseYokeRotation * rollEffect;

                                Quaternion pitchEffect = Quaternion.AngleAxis(m_base.m_pitch * m_pitchLimit, m_pitchAxisDeflection);
                                m_lever.localRotation = m_baseLeverRotation * pitchEffect;
                            }
                        }
                        // Joystick
                        if (m_stickType == StickType.Joystick)
                        {
                            if (m_lever != null)
                            {
                                Quaternion rollEffect = Quaternion.AngleAxis(m_base.m_roll * m_rollLimit, m_rollAxisDeflection);
                                Quaternion pitchEffect = Quaternion.AngleAxis(m_base.m_pitch * m_pitchLimit, m_pitchAxisDeflection);
                                m_lever.localRotation = m_baseLeverRotation * rollEffect * pitchEffect;
                            }
                        }
                    }
                    // Rudder Pedals
                    if (m_leverType == LeverType.Pedal)
                    {
                        // ---------------------- ROTATE
                        if (m_pedalType == PedalType.Hinged)
                        {
                            float m_deflection = m_base.m_yaw * m_pedalDeflectionLimit;

                            if (m_rightPedal != null && m_leftPedal != null)
                            {
                                // ---------------------- ROTATE PEDALS
                                m_rightPedal.localRotation = m_baseRightRotation;
                                m_rightPedal.Rotate(m_rightAxis, m_deflection);
                                if (m_pedalMode == PedalMode.Combined)
                                {
                                    m_leftPedal.localRotation = m_baseLeftRotation;
                                    m_leftPedal.Rotate(m_leftAxis, m_deflection);
                                }
                                else
                                {
                                    m_leftPedal.localRotation = m_baseLeftRotation;
                                    m_leftPedal.Rotate(m_leftAxis, -m_deflection);
                                }
                            }
                        }

                        // ---------------------- MOVE
                        if (m_pedalType == PedalType.Sliding)
                        {
                            if (m_rightPedal != null && m_leftPedal != null)
                            {
                                float m_distance = m_base.m_yaw * (m_pedalSlidingLimit / 100);
                                //MOVE PEDALS
                                m_rightPedal.localPosition = m_baseRightPosition;
                                m_rightPedal.localPosition += m_rightAxis * m_distance;
                                m_leftPedal.localPosition = m_baseLeftPosition;
                                m_leftPedal.localPosition += m_leftAxis * m_distance;
                            }
                        }
                    }
                    // Throttle
                    if (m_leverType == LeverType.Throttle || m_leverType == LeverType.Mixture || m_leverType == LeverType.PropellerPitch)
                    {
                        if (m_lever != null)
                        {
                            float m_f = 0;
                            if (m_leverType == LeverType.Throttle) { m_f = m_base.m_throttle; }
                            if (m_leverType == LeverType.Mixture) { m_f = m_base.m_mixture; }
                            if (m_leverType == LeverType.PropellerPitch) { m_f = m_base.m_prop; }


                            if (m_leverType == LeverType.Throttle && m_throttleMode == ThrottleMode.Deflection)
                            {
                                float m_rt = m_f * m_deflectionLimit;
                                m_lever.localRotation = m_baseLeverRotation;
                                m_lever.Rotate(m_axisDeflection, m_rt);
                            }
                            else
                            {
                                float m_rt = m_f * m_slidingLimit;
                                Vector3 m_rgt = m_lever.right;
                                if (m_movementAxis == MovementAxis.Forward) { m_rgt = m_lever.forward; }
                                if (m_movementAxis == MovementAxis.Right) { m_rgt = m_lever.right; }
                                if (m_movementAxis == MovementAxis.Up) { m_rgt = m_lever.up; }
                                if (m_movementDirection == MovementDirection.Inverted) { m_rgt *= -1; }

                                Vector3 m_worldPosition = m_controller.transform.TransformPoint(m_baseLeverPosition);
                                Vector3 m_position = m_worldPosition + (0.01f * m_rt * m_rgt);
                                m_lever.position = m_position;
                            }

                            m_lever.localRotation = m_baseLeverRotation;
                        }
                    }
                    // Flaps
                    if (m_leverType == LeverType.Flaps)
                    {
                        if (m_lever != null)
                        {
                            float m_deflection = _input * m_deflectionLimit;
                            m_lever.localRotation = m_baseLeverRotation;
                            m_lever.Rotate(m_axisDeflection, m_deflection);
                        }
                    }
                    // Gear
                    if (m_leverType == LeverType.GearIndicator)
                    {
                        float _target = 0;

                        if (m_controller != null)
                        {
                            if (m_controller.m_gearState == Controller.GearState.Down) { _target = 0; }
                            if (m_controller.m_gearState == Controller.GearState.Up) { _target = 1; }
                            _leverInput = Mathf.MoveTowards(_leverInput, _target, m_controller._timestep * 0.5f);
                        }

                        if (m_lever != null)
                        {
                            float m_deflection = _leverInput * m_deflectionLimit;
                            m_lever.localRotation = m_baseLeverRotation;
                            m_lever.Rotate(m_axisDeflection, m_deflection);
                        }
                    }
                    // Collective
                    if (m_leverType == LeverType.Collective)
                    {
                        if (m_lever != null)
                        {
                            float m_deflection = m_controller._collectiveInput * m_deflectionLimit;
                            m_lever.localRotation = m_baseLeverRotation;
                            m_lever.Rotate(m_axisDeflection, m_deflection);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class Dial
        {
            public SilantroInstrument m_base;
            public List<Needle> m_needles;
            public double[] m_values = new double[10];

            [Serializable]
            public class Needle
            {
                public string _id = "Base";
                public Transform _needle;
                public double _currentValue;

                public RotationAxis _axis;
                public RotationDirection _direction;
                public double _lag = 0.1;

                public float _minValue = 0;
                public float _maxValue = 100;
                public float _minRotation = -180;
                public float _maxRotation = 180;

                private Vector3 _rotationAxis;
                private Quaternion _baseRotation;

                /// <summary>
                /// 
                /// </summary>
                public void _configure()
                {
                    _rotationAxis = Handler.EstimateModelProperties(_direction.ToString(), _axis.ToString());
                    if (_needle != null) { _baseRotation = _needle.localRotation; }
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="_value"></param>
                public void _compute(double _value)
                {
                    _currentValue = _value;
                    float _dialValue = Mathf.Lerp(_minRotation, _maxRotation, Mathf.InverseLerp(_minValue, _maxValue, (float)_value));

                    if (_needle != null)
                    {
                        _needle.localRotation = _baseRotation;
                        _needle.Rotate(_rotationAxis, _dialValue);
                    }
                }
            }
            /// <summary>
            /// 
            /// </summary>
            public void Initialize() { foreach (Needle needle in m_needles) { needle._configure(); } }
            /// <summary>
            /// Updates the values on the needles
            /// </summary>
            /// <param name="values"></param>
            public void UpdateGauge()
            {
                for (int i = 0; i < m_needles.Count; i++)
                {
                    m_needles[i]._compute(m_values[i]);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class Readout
        {
            public SilantroInstrument m_base;
            public enum Type { Airspeed, Altitude }
            public enum AltimeterType { FourDigit, ThreeDigit }

            public Type m_type = Type.Airspeed;
            public AltimeterType m_altimeterType = AltimeterType.ThreeDigit;

            public float digitOne;
            public float digitTwo;
            public float digitThree;
            public float digitFour;
            public float digitFive;
            float dialValue;

            public float digitOneTranslation;
            public float digitTwoTranslation;
            public float digitThreeTranslation;
            public float digitFourTranslation;

            public RectTransform digitOneContainer;
            public RectTransform digitTwoContainer;
            public RectTransform digitThreeContainer;
            public RectTransform digitFourContainer;

            public RectTransform needle;
            float needleRotation;
            float smoothRotation;
            public float maximumValue;

            public void Compute(double altitude, double speed)
            {
                if (m_type == Type.Airspeed)
                {
                    //COLLECT VALUE
                    dialValue = (float)(speed * Oyedoyin.Mathematics.Constants.ms2knots);

                    //EXTRACT DIGITS
                    digitOne = (dialValue % 10f);
                    digitTwo = Mathf.Floor((dialValue % 100.0f) / 10.0f);
                    digitThree = Mathf.Floor((dialValue % 1000.0f) / 100.0f);

                    //CALCULATE DIAL POSITIONS
                    float digitOnePosition = digitOne * -digitOneTranslation;
                    float digitTwoPosition = digitTwo * -digitTwoTranslation; if (digitOne > 9.0f) { digitTwoPosition += (digitOne - 9.0f) * -digitTwoTranslation; }
                    float digitThreePosition = digitThree * -digitThreeTranslation; if ((digitTwo * 10) > 99.0f) { digitThreePosition += ((digitTwo * 10f) - 99.0f) * -digitThreeTranslation; }

                    //SET POSITIONS
                    if (digitOneContainer != null) { digitOneContainer.localPosition = new Vector3(0, digitOnePosition, 0); }
                    if (digitTwoContainer != null) { digitTwoContainer.localPosition = new Vector3(0, digitTwoPosition, 0); }
                    if (digitThreeContainer != null) { digitThreeContainer.localPosition = new Vector3(0, digitThreePosition, 0); }
                }


                ///---------------------------------------------------------ALTIMETER
                if (m_type == Type.Altitude)
                {
                    //COLLECT VALUE
                    dialValue = (float)(altitude * Oyedoyin.Mathematics.Constants.m2ft);
                    maximumValue = 10000f;

                    //EXTRACT DIGITS
                    digitOne = ((dialValue % 100.0f) / 20.0f);//20FT Spacing
                    digitTwo = Mathf.Floor((dialValue % 1000.0f) / 100.0f);
                    digitThree = Mathf.Floor((dialValue % 10000.0f) / 1000.0f);
                    digitFour = Mathf.Floor((dialValue % 100000.0f) / 10000.0f);

                    //CALCULATE DIAL POSITIONS
                    float digitOnePosition = digitOne * -digitOneTranslation;
                    float digitTwoPosition = digitTwo * -digitTwoTranslation;
                    if ((digitOne * 20) > 90.0f) { digitTwoPosition += ((digitOne * 20f) - 90.0f) / 10.0f * -digitTwoTranslation; }
                    float digitThreePosition = digitThree * -digitThreeTranslation;
                    if ((digitTwo * 100) > 990.0f) { digitThreePosition += ((digitTwo * 100) - 990.0f) / 10.0f * -digitThreeTranslation; }
                    float digitFourPosition = 0f;
                    if (m_altimeterType == AltimeterType.FourDigit) { digitFourPosition = digitFour * -digitFourTranslation; if ((digitThree * 1000) > 9990.0f) { digitFourPosition += ((digitThree * 1000) - 9990.0f) / 10f * -digitFourTranslation; } }


                    //SET POSITIONS
                    if (digitOneContainer != null) { digitOneContainer.localPosition = new Vector3(0, digitOnePosition, 0); }
                    if (digitTwoContainer != null) { digitTwoContainer.localPosition = new Vector3(0, digitTwoPosition, 0); }
                    if (digitThreeContainer != null) { digitThreeContainer.localPosition = new Vector3(0, digitThreePosition, 0); }
                    if (m_altimeterType == AltimeterType.FourDigit) { if (digitFourContainer != null) { digitFourContainer.localPosition = new Vector3(0, digitFourPosition, 0); } }

                }


                //-----------------------------------------------------------------NEEDLE
                if (needle != null)
                {
                    needleRotation = Mathf.Lerp(0, 360, dialValue / maximumValue);
                    smoothRotation = Mathf.Lerp(smoothRotation, needleRotation, Time.deltaTime * 5);
                    needle.transform.eulerAngles = new Vector3(needle.transform.eulerAngles.x, needle.transform.eulerAngles.y, -smoothRotation);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class HSI
        {
            public SilantroInstrument m_base;
            public RectTransform pitchTape;
            public RectTransform rollAnchor;

            public float minimumPosition;
            public float movementFactor = -50;
            public float maximumPitch = 90;
            public float minimumPitch = -90;

            public float minimumRoll = -180;
            public float maximumRoll = 180;

            /// <summary>
            /// 
            /// </summary>
            public void Compute()
            {
                if (m_base != null)
                {
                    if (pitchTape != null)
                    {
                        float pitchValue = Mathf.DeltaAngle(0, -m_base.m_controller.m_rigidbody.transform.rotation.eulerAngles.x);
                        float extension = minimumPosition + movementFactor * Mathf.Clamp(pitchValue, minimumPitch, maximumPitch) / 10f;
                        pitchTape.anchoredPosition3D = new Vector3(pitchTape.anchoredPosition3D.x, extension, pitchTape.anchoredPosition3D.z);
                    }

                    if (rollAnchor != null)
                    {
                        float rollValue = Mathf.DeltaAngle(0, -m_base.m_controller.m_rigidbody.transform.eulerAngles.z);
                        float rotation = Mathf.Clamp(rollValue, minimumRoll, maximumRoll);
                        rollAnchor.localEulerAngles = new Vector3(rollAnchor.localEulerAngles.x, rollAnchor.localEulerAngles.y, rotation);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            SilantroInstrument instrument = GetComponent<SilantroInstrument>();
            if (m_type == Type.Lever)
            {
                if (instrument != null) { m_lever.m_base = instrument; }
                m_lever.m_controller = m_controller;
                m_lever.Initialize();
            }
            if (m_type == Type.Dial)
            {
                if (instrument != null) { m_dial.m_base = instrument; }
                m_dial.Initialize();
            }
            if (m_type == Type.HSI)
            {
                if (instrument != null) { m_hsi.m_base = instrument; }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Compute()
        {
            if (m_controller != null)
            {
                /// <summary>
                /// 
                /// </summary>
                if (m_type == Type.Lever) { if (m_lever.m_leverType != Lever.LeverType.Flaps) { m_lever.Compute(0); } }
                /// <summary>
                /// 
                /// </summary>
                if (m_type == Type.Dial)
                {

                }
                if (m_type == Type.HSI) { m_hsi.Compute(); }
            }
        }
    }
    #endregion

    #region Editor

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroInstrument))]

    public class InstrumentEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1, 0.4f, 0);
        SilantroInstrument instrument;
        SerializedProperty lever;
        SerializedProperty dial;
        SerializedProperty readout;
        SerializedProperty hsi;
        SerializedProperty needleList;

        private static readonly GUIContent deleteButton = new GUIContent("Remove", "Delete");
        private static readonly GUILayoutOption buttonWidth = GUILayout.Width(60f);

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            instrument = (SilantroInstrument)target;
            lever = serializedObject.FindProperty("m_lever");
            dial = serializedObject.FindProperty("m_dial");
            readout = serializedObject.FindProperty("m_readout");
            hsi = serializedObject.FindProperty("m_hsi");
            needleList = dial.FindPropertyRelative("m_needles");
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
            EditorGUILayout.HelpBox("Functionality", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_type"), new GUIContent(" "));


            // -----------------------------------------------------------------------------------------------------------------------------------------
            if (instrument.m_type == SilantroInstrument.Type.Lever)
            {
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Lever Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_leverType"), new GUIContent("Type"));

                if (instrument.m_lever.m_leverType == SilantroInstrument.Lever.LeverType.Stick)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_stickType"), new GUIContent(" "));

                    if (instrument.m_lever.m_stickType == SilantroInstrument.Lever.StickType.Joystick)
                    {
                        GUILayout.Space(5f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_lever"), new GUIContent("Stick Hinge "));

                        GUILayout.Space(10f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Roll Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rotationAxisRoll"), new GUIContent("Rotation Axis"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_directionRoll"), new GUIContent("Rotation Direction"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rollLimit"), new GUIContent("Maximum Deflection"));

                        GUILayout.Space(10f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Pitch Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rotationAxisPitch"), new GUIContent("Rotation Axis"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_directionPitch"), new GUIContent("Rotation Direction"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_pitchLimit"), new GUIContent("Maximum Deflection"));
                    }
                    if (instrument.m_lever.m_stickType == SilantroInstrument.Lever.StickType.Yoke)
                    {
                        GUILayout.Space(5f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_lever"), new GUIContent("Yoke Hinge"));

                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rotationAxisRoll"), new GUIContent("Rotation Axis"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_directionRoll"), new GUIContent("Rotation Direction"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rollLimit"), new GUIContent("Deflection Limit ( °)"));

                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Movement Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_movementAxis"), new GUIContent("Movement Axis"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_movementDirection"), new GUIContent("Movement Direction"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_pitchLimit"), new GUIContent("Movement Limit ( cm)"));
                    }
                    if (instrument.m_lever.m_stickType == SilantroInstrument.Lever.StickType.ControlColumn)
                    {
                        GUILayout.Space(5f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_lever"), new GUIContent("Column Hinge"));
                        GUILayout.Space(5f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_yoke"), new GUIContent("Yoke Hinge"));

                        GUILayout.Space(10f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Roll Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rotationAxisRoll"), new GUIContent("Rotation Axis"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_directionRoll"), new GUIContent("Rotation Direction"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rollLimit"), new GUIContent("Maximum Deflection"));

                        GUILayout.Space(10f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Pitch Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rotationAxisPitch"), new GUIContent("Rotation Axis"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_directionPitch"), new GUIContent("Rotation Direction"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_pitchLimit"), new GUIContent("Maximum Deflection"));
                    }
                }

                if (instrument.m_lever.m_leverType == SilantroInstrument.Lever.LeverType.Throttle)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_throttleMode"), new GUIContent(" "));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_lever"), new GUIContent("Hinge"));


                    if (instrument.m_lever.m_throttleMode == SilantroInstrument.Lever.ThrottleMode.Deflection)
                    {
                        GUILayout.Space(10f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rotationAxis"), new GUIContent("Rotation Axis"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_direction"), new GUIContent("Rotation Direction"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_deflectionLimit"), new GUIContent("Maximum Deflection"));
                    }

                    if (instrument.m_lever.m_throttleMode == SilantroInstrument.Lever.ThrottleMode.Sliding)
                    {
                        GUILayout.Space(10f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Sliding Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_movementAxis"), new GUIContent("Sliding Axis"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_slidingLimit"), new GUIContent("Maximum Slide Distance"));
                    }
                }

                if (instrument.m_lever.m_leverType == SilantroInstrument.Lever.LeverType.Pedal)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_pedalType"), new GUIContent(" "));

                    if (instrument.m_lever.m_pedalType == SilantroInstrument.Lever.PedalType.Hinged)
                    {
                        GUILayout.Space(10f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Right Pedal Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rightPedal"), new GUIContent("Right Pedal"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rightRotationAxis"), new GUIContent("Rotation Axis"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rightDirection"), new GUIContent("Rotation Deflection"));


                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Left Pedal Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_leftPedal"), new GUIContent("Left Pedal"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_leftRotationAxis"), new GUIContent("Rotation Axis"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_leftDirection"), new GUIContent("Rotation Deflection"));

                        GUILayout.Space(10f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_pedalMode"), new GUIContent("Clamped Together"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_pedalDeflectionLimit"), new GUIContent("Maximum Deflection"));
                    }

                    if (instrument.m_lever.m_pedalType == SilantroInstrument.Lever.PedalType.Sliding)
                    {
                        GUILayout.Space(10f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Right Pedal Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rightPedal"), new GUIContent("Right Pedal"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rightRotationAxis"), new GUIContent("Sliding Axis"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rightDirection"), new GUIContent("Sliding Deflection"));


                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Left Pedal Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_leftPedal"), new GUIContent("Left Pedal"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_leftRotationAxis"), new GUIContent("Sliding Axis"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_leftDirection"), new GUIContent("Sliding Deflection"));


                        GUILayout.Space(10f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Sliding Configuration", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_pedalSlidingLimit"), new GUIContent("Sliding Distance (cm)"));
                    }
                }

                if (instrument.m_lever.m_leverType == SilantroInstrument.Lever.LeverType.Mixture || instrument.m_lever.m_leverType == SilantroInstrument.Lever.LeverType.PropellerPitch)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_lever"), new GUIContent("Hinge"));
                    GUILayout.Space(10f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Sliding Configuration", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(2f);
                    EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_movementAxis"), new GUIContent("Sliding Axis"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_slidingLimit"), new GUIContent("Maximum Slide Distance"));
                }

                if (instrument.m_lever.m_leverType == SilantroInstrument.Lever.LeverType.Flaps ||
                    instrument.m_lever.m_leverType == SilantroInstrument.Lever.LeverType.GearIndicator ||
                   instrument.m_lever.m_leverType == SilantroInstrument.Lever.LeverType.Collective)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_lever"), new GUIContent("Hinge"));
                    GUILayout.Space(10f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Deflection Configuration", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(2f);
                    EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_rotationAxis"), new GUIContent("Rotation Axis"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_direction"), new GUIContent("Rotation Direction"));
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(lever.FindPropertyRelative("m_deflectionLimit"), new GUIContent("Maximum Deflection"));
                }
            }

            // -----------------------------------------------------------------------------------------------------------------------------------------
            if (instrument.m_type == SilantroInstrument.Type.Readout)
            {
                GUILayout.Space(5f);
                if (instrument.m_readout.m_type == SilantroInstrument.Readout.Type.Altitude)
                {
                    GUILayout.Space(3f); GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Altimeter Type", MessageType.None);
                    GUI.color = backgroundColor; GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_altimeterType"), new GUIContent(" "));
                }

                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Dial 'Per Digit' Translation", MessageType.None); GUI.color = backgroundColor;
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.PropertyField(readout.FindPropertyRelative("digitOneTranslation"), new GUIContent("Digit One"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(readout.FindPropertyRelative("digitTwoTranslation"), new GUIContent("Digit Two"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(readout.FindPropertyRelative("digitThreeTranslation"), new GUIContent("Digit Three"));

                if (instrument.m_readout.m_type == SilantroInstrument.Readout.Type.Altitude && instrument.m_readout.m_altimeterType == SilantroInstrument.Readout.AltimeterType.FourDigit)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("digitFourTranslation"), new GUIContent("Digit Four"));
                }


                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Digit Containers", MessageType.None); GUI.color = backgroundColor;
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.PropertyField(readout.FindPropertyRelative("digitOneContainer"), new GUIContent("Digit One Container"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(readout.FindPropertyRelative("digitTwoContainer"), new GUIContent("Digit Two Container"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(readout.FindPropertyRelative("digitThreeContainer"), new GUIContent("Digit Three Container"));

                if (instrument.m_readout.m_type == SilantroInstrument.Readout.Type.Altitude && instrument.m_readout.m_altimeterType == SilantroInstrument.Readout.AltimeterType.FourDigit)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("digitFourContainer"), new GUIContent("Digit Four Container"));
                }

                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Digit Display", MessageType.None); GUI.color = backgroundColor;
                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.LabelField("Current Value", instrument.m_readout.digitFour.ToString("0") + "| " + instrument.m_readout.digitThree.ToString("0") + "| " + instrument.m_readout.digitTwo.ToString("0") + "| " + instrument.m_readout.digitOne.ToString("0"));


                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Dial Face Settings", MessageType.None); GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(readout.FindPropertyRelative("needle"), new GUIContent("Needle"));

                if (instrument.m_readout.m_type == SilantroInstrument.Readout.Type.Airspeed)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(readout.FindPropertyRelative("maximumValue"), new GUIContent("Maximum Speed"));
                }
            }

            // -----------------------------------------------------------------------------------------------------------------------------------------
            if (instrument.m_type == SilantroInstrument.Type.HSI)
            {
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Anchors", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(hsi.FindPropertyRelative("pitchTape"), new GUIContent("Pitch"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(hsi.FindPropertyRelative("rollAnchor"), new GUIContent("Roll"));

                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Factors", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(hsi.FindPropertyRelative("minimumPosition"), new GUIContent("Base Position"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(hsi.FindPropertyRelative("movementFactor"), new GUIContent("Movement Factor"));

                GUILayout.Space(3f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Limits", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(hsi.FindPropertyRelative("maximumPitch"), new GUIContent("Maximum Pitch"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(hsi.FindPropertyRelative("minimumPitch"), new GUIContent("Minimum Pitch"));

                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(hsi.FindPropertyRelative("maximumRoll"), new GUIContent("Maximum Roll"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(hsi.FindPropertyRelative("minimumRoll"), new GUIContent("Minimum Roll"));
            }

            // -----------------------------------------------------------------------------------------------------------------------------------------
            if (instrument.m_type == SilantroInstrument.Type.Dial)
            {
                if (needleList != null) { EditorGUILayout.LabelField("Needle Count", needleList.arraySize.ToString()); }
                GUILayout.Space(5f);
                if (GUILayout.Button("Add Needle")) { instrument.m_dial.m_needles.Add(new SilantroInstrument.Dial.Needle()); }

                if (needleList != null)
                {
                    GUILayout.Space(2f);
                    //DISPLAY WHEEL ELEMENTS
                    for (int i = 0; i < needleList.arraySize; i++)
                    {
                        SerializedProperty reference = needleList.GetArrayElementAtIndex(i);

                        GUI.color = new Color(1, 0.8f, 0);
                        EditorGUILayout.HelpBox("Needle : " + (i + 1).ToString(), MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(reference.FindPropertyRelative("_id"), new GUIContent("ID"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(reference.FindPropertyRelative("_needle"), new GUIContent("Hinge"));


                        GUILayout.Space(3f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Rotation", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(reference.FindPropertyRelative("_axis"), new GUIContent("Axis"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(reference.FindPropertyRelative("_direction"), new GUIContent("Direction"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(reference.FindPropertyRelative("_lag"), new GUIContent("Lag"));



                        GUILayout.Space(3f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Limits", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(reference.FindPropertyRelative("_minValue"), new GUIContent("Minimum Value"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(reference.FindPropertyRelative("_maxValue"), new GUIContent("Maximum Value"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(reference.FindPropertyRelative("_minRotation"), new GUIContent("Minimum Rotation"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(reference.FindPropertyRelative("_maxRotation"), new GUIContent("Maximum Rotation"));


                        GUILayout.Space(3f);
                        if (GUILayout.Button(deleteButton, EditorStyles.miniButtonRight, buttonWidth))
                        {
                            instrument.m_dial.m_needles.RemoveAt(i);
                        }
                        GUILayout.Space(10f);
                    }
                }
            }


            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    #endregion
}
