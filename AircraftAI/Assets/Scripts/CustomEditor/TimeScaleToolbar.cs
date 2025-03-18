using UnityEngine;
using UnityEditor;
using UnityToolbarExtender;

[InitializeOnLoad]
public class TimeScaleToolbar
{
    private static float timeScale = 1.0f;
    private static string timeScaleInput = "1.0";

    static TimeScaleToolbar() => ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);

    private static void OnToolbarGUI()
    {
        GUILayout.FlexibleSpace();
        GUILayout.Label("Time Scale", GUILayout.Width(70));

        var newInput = GUILayout.TextField(timeScaleInput, GUILayout.Width(40));

        if (newInput == timeScaleInput) return;
        
        timeScaleInput = newInput;
        if (!float.TryParse(timeScaleInput, out var parsedValue)) return;
        timeScale = Mathf.Clamp(parsedValue, 0.1f, 5f);
        Time.timeScale = timeScale;
    }
}