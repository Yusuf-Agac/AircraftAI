#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Oyedoyin.Mathematics;



/// <summary>
/// 
/// </summary>
namespace Oyedoyin.Common.Editors
{

#if UNITY_EDITOR

    /// <summary>
    /// 
    /// </summary>
    [CustomPropertyDrawer(typeof(FComplex))]
    public class FComplexDrawer : PropertyDrawer
    {

        private const float SubLabelSpacing = 8;
        private const float BottomSpacing = 2;
        string realLabel = "Real";
        string imagLabel = "Imaginary";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="prop"></param>
        /// <param name="label"></param>
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            Rect contentPosition = EditorGUI.PrefixLabel(pos, label);
            if (contentPosition.width > 185) { realLabel = "Real"; imagLabel = "ζ"; } else { realLabel = "R"; imagLabel = "ζ"; }

            pos.height -= BottomSpacing;
            label = EditorGUI.BeginProperty(pos, label, prop);
            var contentRect = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);
            var labels = new[] { new GUIContent(realLabel), new GUIContent(imagLabel) };
            var properties = new[] { prop.FindPropertyRelative("m_real"), prop.FindPropertyRelative("m_imaginary") };
            DrawMultiplePropertyFields(contentRect, labels, properties);

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + BottomSpacing;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="subLabels"></param>
        /// <param name="props"></param>
        private static void DrawMultiplePropertyFields(Rect pos, GUIContent[] subLabels, SerializedProperty[] props)
        {
            // backup GUI settings
            var indent = EditorGUI.indentLevel;
            var labelWidth = EditorGUIUtility.labelWidth;

            // draw properties
            var propsCount = props.Length;
            var width = (pos.width - (propsCount - 1) * SubLabelSpacing) / propsCount;
            var contentPos = new Rect(pos.x, pos.y, width, pos.height);
            EditorGUI.indentLevel = 0;
            for (var i = 0; i < propsCount; i++)
            {
                EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(subLabels[i]).x;
                EditorGUI.PropertyField(contentPos, props[i], subLabels[i]);
                contentPos.x += width + SubLabelSpacing;
            }

            // restore GUI settings
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = indent;
        }
    }



    /// <summary>
    /// 
    /// </summary>
    [CustomPropertyDrawer(typeof(Gain))]
    public class GainDrawer : PropertyDrawer
    {

        private const float SubLabelSpacing = 8;
        private const float BottomSpacing = 2;
        string realLabel = "Speed";
        string imagLabel = "Factor";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="prop"></param>
        /// <param name="label"></param>
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            Rect contentPosition = EditorGUI.PrefixLabel(pos, label);
            if (contentPosition.width > 200) { realLabel = "Speed "; imagLabel = "Factor "; } else { realLabel = "kts "; imagLabel = "kp "; }

            pos.height -= BottomSpacing;
            label = EditorGUI.BeginProperty(pos, label, prop);
            var contentRect = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);
            var labels = new[] { new GUIContent(realLabel), new GUIContent(imagLabel) };
            var properties = new[] { prop.FindPropertyRelative("speed"), prop.FindPropertyRelative("factor") };
            DrawMultiplePropertyFields(contentRect, labels, properties);

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + BottomSpacing;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="subLabels"></param>
        /// <param name="props"></param>
        private static void DrawMultiplePropertyFields(Rect pos, GUIContent[] subLabels, SerializedProperty[] props)
        {
            // backup GUI settings
            var indent = EditorGUI.indentLevel;
            var labelWidth = EditorGUIUtility.labelWidth;

            // draw properties
            var propsCount = props.Length;
            var width = (pos.width - (propsCount - 1) * SubLabelSpacing) / propsCount;
            var contentPos = new Rect(pos.x, pos.y, width, pos.height);
            EditorGUI.indentLevel = 0;
            for (var i = 0; i < propsCount; i++)
            {
                EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(subLabels[i]).x;
                EditorGUI.PropertyField(contentPos, props[i], subLabels[i]);
                contentPos.x += width + SubLabelSpacing;
            }

            // restore GUI settings
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = indent;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [CustomPropertyDrawer(typeof(Vector))]
    public class VectorDrawer : PropertyDrawer
    {

        private const float SubLabelSpacing = 8;
        private const float BottomSpacing = 2;
        readonly string xLabel = "X";
        readonly string yLabel = "Y";
        readonly string zlabel = "Z";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="prop"></param>
        /// <param name="label"></param>
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            pos.height -= BottomSpacing;
            label = EditorGUI.BeginProperty(pos, label, prop);
            var contentRect = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);
            var labels = new[] { new GUIContent(xLabel), new GUIContent(yLabel), new GUIContent(zlabel) };
            var properties = new[] { prop.FindPropertyRelative("x"), prop.FindPropertyRelative("y"), prop.FindPropertyRelative("z") };
            DrawMultiplePropertyFields(contentRect, labels, properties);

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return base.GetPropertyHeight(property, label) + BottomSpacing; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="subLabels"></param>
        /// <param name="props"></param>
        private static void DrawMultiplePropertyFields(Rect pos, GUIContent[] subLabels, SerializedProperty[] props)
        {
            // backup GUI settings
            var indent = EditorGUI.indentLevel;
            var labelWidth = EditorGUIUtility.labelWidth;

            // draw properties
            var propsCount = props.Length;
            var width = (pos.width - (propsCount - 1) * SubLabelSpacing) / propsCount;
            var contentPos = new Rect(pos.x, pos.y, width, pos.height);
            EditorGUI.indentLevel = 0;
            for (var i = 0; i < propsCount; i++)
            {
                EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(subLabels[i]).x;
                EditorGUI.PropertyField(contentPos, props[i], subLabels[i]);
                contentPos.x += width + SubLabelSpacing;
            }

            // restore GUI settings
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = indent;
        }
    }

#endif


}