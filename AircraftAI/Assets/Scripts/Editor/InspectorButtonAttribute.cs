using System;
using UnityEngine;
using UnityEditor;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class InspectorButtonAttribute : PropertyAttribute
{
    public string MethodName { get; private set; }

    public InspectorButtonAttribute(string methodName)
    {
        MethodName = methodName;
    }
}

[CustomEditor(typeof(MonoBehaviour), true)]
public class InspectorButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var type = target.GetType();
        var methods = type.GetMethods();

        foreach (var method in methods)
        {
            var attributes = method.GetCustomAttributes(typeof(InspectorButtonAttribute), true);
            foreach (var attribute in attributes)
            {
                if (attribute is InspectorButtonAttribute buttonAttribute)
                {
                    if (GUILayout.Button(buttonAttribute.MethodName))
                    {
                        method.Invoke(target, null);
                    }
                }
            }
        }
    }
}
