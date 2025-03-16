#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ShowIfAttribute : PropertyAttribute
{
    public string ConditionField { get; private set; }

    public ShowIfAttribute(string conditionField)
    {
        ConditionField = conditionField;
    }
}

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        SerializedProperty conditionProperty = property.serializedObject.FindProperty(showIf.ConditionField);

        if (conditionProperty != null && conditionProperty.propertyType == SerializedPropertyType.Boolean)
        {
            if (conditionProperty.boolValue)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
        else
        {
            EditorGUI.HelpBox(position, $"Error: Boolean field '{showIf.ConditionField}' not found.", MessageType.Error);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;
        SerializedProperty conditionProperty = property.serializedObject.FindProperty(showIf.ConditionField);

        if (conditionProperty != null && conditionProperty.propertyType == SerializedPropertyType.Boolean)
        {
            return conditionProperty.boolValue ? EditorGUI.GetPropertyHeight(property) : 0;
        }
        return EditorGUI.GetPropertyHeight(property);
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ShowIfHeaderAttribute : PropertyAttribute
{
    public string ConditionField { get; private set; }
    public string Header { get; private set; }
    
    public ShowIfHeaderAttribute(string conditionField, string header = null)
    {
        ConditionField = conditionField;
        Header = header;
    }
}

[CustomPropertyDrawer(typeof(ShowIfHeaderAttribute))]
public class ShowIfHeaderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfHeaderAttribute showIf = (ShowIfHeaderAttribute)attribute;
        SerializedProperty conditionProperty = property.serializedObject.FindProperty(showIf.ConditionField);

        if (conditionProperty != null && conditionProperty.propertyType == SerializedPropertyType.Boolean)
        {
            if (conditionProperty.boolValue)
            {
                if (!string.IsNullOrEmpty(showIf.Header))
                {
                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField(showIf.Header, EditorStyles.boldLabel);
                }

                EditorGUI.PropertyField(position, property, label, true);
            }
        }
        else
        {
            EditorGUI.HelpBox(position, $"Error: Boolean field '{showIf.ConditionField}' not found.", MessageType.Error);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfHeaderAttribute showIf = (ShowIfHeaderAttribute)attribute;
        SerializedProperty conditionProperty = property.serializedObject.FindProperty(showIf.ConditionField);

        if (conditionProperty != null && conditionProperty.propertyType == SerializedPropertyType.Boolean)
        {
            return conditionProperty.boolValue ? EditorGUI.GetPropertyHeight(property) + 25 : 0;
        }
        return EditorGUI.GetPropertyHeight(property);
    }
}
#endif