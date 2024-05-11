using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class SubclassPicker : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SubclassPicker))]
public class SubclassPickerDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property);
    }

    IEnumerable GetClasses(Type baseType)
    {
        if (baseType.IsArray) baseType = baseType.GetElementType();
        return Assembly.GetAssembly(baseType).GetTypes().Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t));
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Type t = fieldInfo.FieldType;
        string typeName = property.managedReferenceValue?.GetType().Name ?? "Not set";

        Rect dropdownRect = position;
        dropdownRect.x += EditorGUIUtility.labelWidth + 2;
        dropdownRect.width -= EditorGUIUtility.labelWidth + 2;
        dropdownRect.height = EditorGUIUtility.singleLineHeight;
        if (EditorGUI.DropdownButton(dropdownRect, new(typeName), FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();

            // null
            menu.AddItem(new GUIContent("None"), property.managedReferenceValue == null, () =>
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            // inherited types
            foreach (Type type in GetClasses(t))
            {
                menu.AddItem(new GUIContent(type.Name), typeName == type.Name, () =>
                {
                    property.managedReferenceValue = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }
        EditorGUI.PropertyField(position, property, label, true);
    }
}
#endif