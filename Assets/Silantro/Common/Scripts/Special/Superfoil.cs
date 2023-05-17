using System;
using System.IO;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif 
using Array = Oyedoyin.Mathematics.Array;

/// <summary>
/// 
/// </summary>
public class Superfoil : MonoBehaviour
{
    public string foilName;
    public Array m_CL;
    public Array m_CD;
    public string plotFile;
    public bool plotSelected;
    private readonly char lineSeperator = '\n';
    public bool bandKeyed;

    /// <summary>
    /// 
    /// </summary>
    public double GetCL(double mach, double aoa)
    {
        return m_CL.GetValue(aoa, mach);
    }

    /// <summary>
    /// 
    /// </summary>
    public double GetCD(double mach, double aoa)
    {
       return m_CD.GetValue(aoa, mach);
    }
      
    /// <summary>
    /// 
    /// </summary>
    public void KeyData()
    {
        StreamReader shape = new StreamReader(plotFile);
        string shapeText = shape.ReadToEnd();
        // Plot rows
        string[] superPlots = shapeText.Split(lineSeperator);

        string[] headerdata = superPlots[0].Split(',');
        m_CL = new Array(headerdata.Length);
        m_CD = new Array(headerdata.Length);
        double[] header = new double[headerdata.Length];
        for(int x = 0; x < headerdata.Length; x++) { header[x] = double.Parse(headerdata[x]); }
        m_CL.m_headers = header;
        m_CD.m_headers = header;

        for (int j = 1; (j < superPlots.Length); j++)
        {
            string[] data = superPlots[j].Split(',');
            // Split lines
            int lineCount = (data.Length - 1) / 2;
            double[] clband = new double[lineCount + 1];
            double[] cdband = new double[lineCount + 1];
            double alpha = float.Parse(data[0]);
            clband[0] = alpha;
            cdband[0] = alpha;

            for (int i = 1; i < lineCount + 1; i++)
            {
                int cdpoint = i * 2;
                int clpoint = cdpoint - 1;
                float cl = float.Parse(data[clpoint]);
                float cd = float.Parse(data[cdpoint]);
                clband[i] = cl;
                cdband[i] = cd;
            }

            m_CL.FillRow(clband);
            m_CD.FillRow(cdband);
        }
        bandKeyed = true;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(Superfoil))]
public class SuperfoilEditor : Editor
{
    Superfoil foil;
    Color backgroundColor;
    Color silantroColor = new Color(1.0f, 0.40f, 0f);

    private void OnEnable()
    {
        foil = (Superfoil)target;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnInspectorGUI()
    {
        backgroundColor = GUI.backgroundColor;
        //DrawDefaultInspector();
        serializedObject.Update();


        GUILayout.Space(2f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Foil Configuration", MessageType.None);
        GUI.color = backgroundColor;

        if (foil.bandKeyed)
        {
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Mach Band Count", foil.m_CL.m_headers.Length.ToString());
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("CL Band Count", foil.m_CL.m_data.Count.ToString());
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("CD Band Count", foil.m_CD.m_data.Count.ToString());
        }
        else
        {
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Mach Band Count", "0");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("CL Band Count", "0");
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("CD Band Count", "0");
        }

        GUILayout.Space(3f);
        GUI.color = silantroColor;
        EditorGUILayout.HelpBox("Band Configuration", MessageType.None);
        GUI.color = backgroundColor;
        GUILayout.Space(15f);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("foilName"), new GUIContent("Identifier"));
        GUILayout.Space(3f);
        if (GUILayout.Button("Select CSV File"))
        {
            foil.plotFile = EditorUtility.OpenFilePanel("CL-CD Mach Plot", "", "csv");
            if (foil.plotFile.Length > 0)  { foil.plotSelected = true; }
        }
        GUILayout.Space(3f);
        EditorGUILayout.TextField(" ", foil.plotFile);

        GUILayout.Space(10f);
        if (GUILayout.Button("Create Superfoil"))
        {
            if (foil.plotSelected) { foil.gameObject.name = foil.foilName; foil.KeyData(); }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
