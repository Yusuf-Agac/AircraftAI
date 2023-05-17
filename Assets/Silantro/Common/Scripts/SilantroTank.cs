using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif



namespace Oyedoyin.Common
{
    /// <summary>
    /// The fuel tanks are designed to be an independent components that can be added or removed from the model hierarchy  as the user pleases.
    /// It contains information about the current amount of fuel in a particular location within the model.
    /// The fuel tanks can either be internal or external.External tanks can have a model attached to it and can be detached from the aircraft.
    /// </summary>
    #region Component
    public class SilantroTank : MonoBehaviour
    {
        // ------------------------------------Selectibles
        public enum TankType
        {
            /// <summary>
            /// Indicates that the tank is located inside the aircraft structure and cannot be detached at runtime
            /// </summary>
            Internal,
            /// <summary>
            /// Indicates that the tank is located inside the aircraft structure and cannot be detached at runtime
            /// </summary>
            External
        }
        public enum TankPosition { Left, Right, Center }
        public enum FuelType { JetB, JetA1, JP6, JP8, AVGas100, AVGas100LL, AVGas82UL }
        public enum FuelUnit { Kilogram, Pounds, USLiquidGallon, ImperialGallon }
        public enum Mode
        {
            /// <summary>
            /// The user can input the maximum capacity of the fuel tank.
            /// </summary>
            Static,
            /// <summary>
            /// The x, y and z values of the local scale of the tank will be used to determine the dimensions of the fuel tank which will then be used the calculate the maximum volume of the tank.
            /// The fuel weight will then be calculated from the volume value and the density of the selected fuel.
            /// </summary>
            Volume
        }

        /// <summary>
        /// Determines if the fuel tank is within the aircraft structure or outside of it.
        /// This helps to check if the tank can be detached/jettisoned or not.
        /// </summary>
        public TankType tankType = TankType.Internal;
        /// <summary>
        /// This indicated the position of the aircraft relative to the zero/center point. 
        /// This helps to determine what order fuel will be used from the tanks to keep the center of gravity within limits.
        /// </summary>
        public TankPosition tankPosition = TankPosition.Center;
        public FuelType fuelType = FuelType.JetB;
        /// <summary>
        /// This allows users to input or calculate the total capacity of the tank in non-metric units which will then be converted into kilograms
        /// </summary>
        public FuelUnit fuelUnit = FuelUnit.Kilogram;
        /// <summary>
        /// This is an indication of the design mode the user wants to employ for the fuel tank.
        /// </summary>
        public Mode mode = Mode.Volume;


        public float m_length = 1;
        public float m_width = 1;
        public float m_depth = 1;
        [Range(0, 90)] public float m_rightTaper = 0;
        [Range(0, 90)] public float m_leftTaper = 0;
        public float m_volume;
        public float m_density = 840;
        public float m_mass;
        [Range(0, 100)] public float m_fillLevel = 100;

        // ------------------------------------Variables
        /// <summary>
        /// Maximum amount of fuel the tank can carry
        /// </summary>
        public float _capacity = 100;
        /// <summary>
        /// Current amount of fuel in the tank
        /// </summary>// 
        public float _currentAmount;
        /// <summary>
        /// Factor used for fuel conversion based on assigned unit
        /// </summary>// 
        public float _actualAmount;
        /// <summary>
        ///  Is the tank attached to the aircraft
        /// </summary>// 
        public bool attached = true;
        /// <summary>
        /// 
        /// </summary>
        float fuelFactor;
        public float m_topArea, m_bottomArea;

        public void Initialize() { ConvertFuel(); _currentAmount = _actualAmount; }
        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            m_length = transform.localScale.z;
            m_width = transform.localScale.x;
            m_depth = transform.localScale.y;

            Vector3 m_right = transform.right;
            Vector3 m_forward = transform.forward;
            Vector3 m_up = transform.up;
            Vector3 m_center = transform.position;

            Vector3 m_rightCenter = m_center + (m_right * m_width * 0.5f);
            Vector3 m_leftCenter = m_center - (m_right * m_width * 0.5f);
            Vector3 m_frc = m_rightCenter + (m_forward * m_length * (0.01f * (100 - m_rightTaper)) * 0.5f);
            Vector3 m_rrc = m_rightCenter - (m_forward * m_length * (0.01f * (100 - m_rightTaper)) * 0.5f);
            Vector3 m_flc = m_leftCenter + (m_forward * m_length * (0.01f * (100 - m_leftTaper)) * 0.5f);
            Vector3 m_rlc = m_leftCenter - (m_forward * m_length * (0.01f * (100 - m_leftTaper)) * 0.5f);

            Vector3 m_frt = m_frc + (m_up * m_depth * 0.5f); Vector3 m_frb = m_frc - (m_up * m_depth * 0.5f);
            Vector3 m_flt = m_flc + (m_up * m_depth * 0.5f); Vector3 m_flb = m_flc - (m_up * m_depth * 0.5f);
            Vector3 m_rrt = m_rrc + (m_up * m_depth * 0.5f); Vector3 m_rrb = m_rrc - (m_up * m_depth * 0.5f);
            Vector3 m_rlt = m_rlc + (m_up * m_depth * 0.5f); Vector3 m_rlb = m_rlc - (m_up * m_depth * 0.5f);

            Color m_tankColorA1 = new Color(1, 0.82f, 0.016f, 0.2f);
            Color m_tankColorA2 = new Color(1, 0.52f, 0.016f, 0.2f);
            Color m_tankColorA3 = new Color(1, 0.62f, 0.016f, 0.2f);
            Color m_tankColorA4 = new Color(1, 0.72f, 0.016f, 0.2f);
            Color m_tankColorB = new Color(1, 0.20f, 0.016f, 0.35f);
            Color m_tankOutline = new Color(1, 0.4f, 0, 1f);
            Vector3[] m_top = new Vector3[] { m_flt, m_frt, m_rrt, m_rlt };
            Vector3[] m_bot = new Vector3[] { m_flb, m_frb, m_rrb, m_rlb };
            Vector3[] m_bac = new Vector3[] { m_rlt, m_rrt, m_rrb, m_rlb };
            Vector3[] m_fro = new Vector3[] { m_flt, m_frt, m_frb, m_flb };
            Vector3[] m_lef = new Vector3[] { m_flt, m_rlt, m_rlb, m_flb };
            Vector3[] m_rig = new Vector3[] { m_frt, m_rrt, m_rrb, m_frb };

            m_topArea = Oyedoyin.Mathematics.MathBase.EstimatePanelSectionArea(m_flt, m_frt, m_rlt, m_rrt);
            m_bottomArea = Oyedoyin.Mathematics.MathBase.EstimatePanelSectionArea(m_flb, m_frb, m_rlb, m_rrb);

#if UNITY_EDITOR

            Handles.DrawSolidRectangleWithOutline(m_top, m_tankColorB, m_tankOutline);
            Handles.DrawSolidRectangleWithOutline(m_bot, m_tankColorB, m_tankOutline);
            Handles.DrawSolidRectangleWithOutline(m_bac, m_tankColorA1, m_tankOutline);
            Handles.DrawSolidRectangleWithOutline(m_fro, m_tankColorA2, m_tankOutline);
            Handles.DrawSolidRectangleWithOutline(m_lef, m_tankColorA3, m_tankOutline);
            Handles.DrawSolidRectangleWithOutline(m_rig, m_tankColorA4, m_tankOutline);


            //DRAW IDENTIFIER
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(this.transform.position, (this.transform.up * 2f + this.transform.position));
#endif
            ConvertFuel();
        }
        /// <summary>
        /// 
        /// </summary>
        private void ConvertFuel()
        {
            // Set Density
            if (fuelType == FuelType.JetA1)
            {
                m_density = 790f;
            }
            if (fuelType == FuelType.JetB)
            {
                m_density = 781f;
            }
            if (fuelType == FuelType.JP6)
            {
                m_density = 810f;
            }
            if (fuelType == FuelType.JP8)
            {
                m_density = 804f;
            }
            if (fuelType == FuelType.AVGas100)
            {
                m_density = 721f;
            }
            if (fuelType == FuelType.AVGas100LL)
            {
                m_density = 769f;
            }
            if (fuelType == FuelType.AVGas82UL)
            {
                m_density = 730f;
            }


            if (fuelUnit == FuelUnit.USLiquidGallon)
            {
                fuelFactor = 3.78541f;
            }
            if (fuelUnit == FuelUnit.ImperialGallon)
            {
                fuelFactor = 4.54609f;
            }
            if (fuelUnit == FuelUnit.Kilogram)
            {
                fuelFactor = 1f;
            }
            if (fuelUnit == FuelUnit.Pounds)
            {
                fuelFactor = 1 / 2.205f;
            }

            m_volume = (m_topArea + m_bottomArea) * 0.5f * m_depth;
            m_mass = m_volume * m_density * m_fillLevel * 0.01f;
            if (mode == Mode.Volume) { _capacity = m_mass; }
            _actualAmount = _capacity * fuelFactor;
        }
    }
    #endregion

    #region Editor

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroTank))]
    public class TankEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1.0f, 0.40f, 0f);
        Color fuelColor = Color.white;
        SilantroTank tank;


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        private void OnEnable() { tank = (SilantroTank)target; }

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector ();
            serializedObject.Update();


            GUILayout.Space(3f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Tank Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tankType"), new GUIContent("Type"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("tankPosition"), new GUIContent("Position"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mode"), new GUIContent("Mode"));

            if (tank.mode == SilantroTank.Mode.Volume)
            {
                GUILayout.Space(10f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Dimensions", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Length", tank.m_length.ToString("0.000") + " m");
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Width", tank.m_width.ToString("0.000") + " m");
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Depth", tank.m_depth.ToString("0.000") + " m");
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Volume", tank.m_volume.ToString("0.000") + " m3");

                GUILayout.Space(10f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Properties", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_rightTaper"), new GUIContent("Right Taper (%)"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_leftTaper"), new GUIContent("Left Taper (%)"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_fillLevel"), new GUIContent("Fill Level (%)"));
            }


            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Fuel Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);

            if (tank.fuelType == SilantroTank.FuelType.AVGas100) { fuelColor = Color.green; }
            else if (tank.fuelType == SilantroTank.FuelType.AVGas100LL) { fuelColor = Color.cyan; }
            else if (tank.fuelType == SilantroTank.FuelType.AVGas82UL) { fuelColor = Color.red; }
            else { fuelColor = Color.white; }

            GUI.color = fuelColor;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fuelType"), new GUIContent("Fuel Type"));
            GUI.color = backgroundColor;
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Density", tank.m_density.ToString("0.0") + " kg/m3");

            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fuelUnit"), new GUIContent("Fuel Unit"));

            if (tank.mode == SilantroTank.Mode.Static)
            {
                GUILayout.Space(8f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_capacity"), new GUIContent("Capacity"));
                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Actual Capacity", tank._actualAmount.ToString("0.00") + " kg");
            }
            else
            {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Capacity", tank._actualAmount.ToString("0.00") + " kg");
            }



            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Fuel Display", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Current Amount", tank._currentAmount.ToString("0.00") + " kg");


            serializedObject.ApplyModifiedProperties();
        }
    }

#endif

    #endregion
}
