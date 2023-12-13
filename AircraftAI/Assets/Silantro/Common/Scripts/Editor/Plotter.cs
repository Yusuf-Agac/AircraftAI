using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Oyedoyin.Mathematics;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
namespace Oyedoyin.Common.Editors
{
    /// <summary>
    /// 
    /// </summary>
    public class Plotter : EditorWindow
    {
        #region Base

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Airfoil/Open", false, 100)]
        public static void ShowWindow()
        {
            GetWindow<Plotter>("Airfoil Plotter");
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Airfoil/Tutorial", false, 200)]
        public static void ShowTutorial()
        {
            Application.OpenURL("https://youtu.be/FGAZMd9tfdI");
        }

        #endregion

        #region Properties

        Color backgroundColor;
        Color silantroColor;
        public string currentTab;
        public int toolbarTab;
        public int dataToolbarTab;

        public string identifier = "NACA 00000";
        public string airfoilShapePath;
        public string airfoilPerformancePath;
        public string airfoilPrefabLocation = "Assets/Silantro/Common/Airfoils/";

        public enum FileType { TXT, AFL }
        public FileType fileType = FileType.TXT;
        public SilantroAirfoil airfoil;

        public bool editShape;
        public bool editPerformance;
        public bool editName;
        public bool shapeSelected, dataSelected;
        public string currentDataTab;

        #endregion

        void OnGUI()
        {
            silantroColor = new Color(1, 0.4f, 0);
            backgroundColor = GUI.backgroundColor;
            toolbarTab = GUILayout.Toolbar(toolbarTab, new string[] { "Create", "Edit" });

            //..................................TABS
            switch (toolbarTab)
            {
                case 0: currentTab = "Create"; break;
                case 1: currentTab = "Edit"; break;
            }


            //..................................ACTIONS
            switch (currentTab)
            {
                case "Create":
                    //.....NAME
                    GUILayout.Space(5f);
                    GUI.color = silantroColor;
                    EditorGUILayout.HelpBox("Airfoil Configuration", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(5f);
                    identifier = EditorGUILayout.TextField("Label", identifier);


                    GUILayout.Space(5f);
                    GUI.color = silantroColor;
                    EditorGUILayout.HelpBox("Save Location", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(5f);
                    airfoilPrefabLocation = EditorGUILayout.TextField(" ", airfoilPrefabLocation);

                    //.....DATA
                    GUILayout.Space(5f);
                    GUI.color = silantroColor;
                    EditorGUILayout.HelpBox("Data Plots", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Shape Plot", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(5f);
                    if (GUILayout.Button("Select Geometry File"))
                    {
                        airfoilShapePath = EditorUtility.OpenFilePanel("Foil Geometry", "", "txt");
                        if (airfoilShapePath.Length > 0)
                        {
                            shapeSelected = true;
                        }
                    }
                    GUILayout.Space(3f);
                    EditorGUILayout.TextField(" ", airfoilShapePath);

                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Base Performance", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(5f);
                    fileType = (FileType)EditorGUILayout.EnumPopup("File Type", fileType);
                    GUILayout.Space(5f);
                    if (fileType == FileType.TXT)
                    {
                        if (GUILayout.Button("Base Polar File"))
                        {
                            airfoilPerformancePath = EditorUtility.OpenFilePanel("Foil Performance", "", "txt");
                            if (airfoilPerformancePath.Length > 0)
                            {
                                shapeSelected = true;
                            }
                        }
                    }
                    if (fileType == FileType.AFL)
                    {
                        if (GUILayout.Button("Base Polar File"))
                        {
                            airfoilPerformancePath = EditorUtility.OpenFilePanel("Foil Performance", "", "afl");
                            if (airfoilPerformancePath.Length > 0)
                            {
                                dataSelected = true;
                            }
                        }
                    }
                    GUILayout.Space(3f);
                    EditorGUILayout.TextField(" ", airfoilPerformancePath);

                    if (airfoilPerformancePath == null || airfoilPerformancePath.Length <= 0 || airfoilShapePath == null || airfoilShapePath.Length <= 0)
                    {
                        GUILayout.Space(20f);
                        GUI.color = silantroColor;
                        EditorGUILayout.HelpBox("Make sure data supply points are complete..", MessageType.Error);
                        GUI.color = backgroundColor;
                    }

                    if (airfoilPerformancePath != null && airfoilPerformancePath.Length > 0 && airfoilShapePath != null && airfoilShapePath.Length > 0)
                    {
                        GUILayout.Space(20f);
                        GUI.color = silantroColor;
                        EditorGUILayout.HelpBox("Make sure selected data is valid..", MessageType.Info);
                        GUI.color = backgroundColor;
                        //
                        GUILayout.Space(10f);
                        if (GUILayout.Button("Create Wingfoil"))
                        {

                            GameObject manager = new GameObject("Airfoil Manager");
                            SilantroFoil plotter = manager.AddComponent<SilantroFoil>();
                            //SET DATA
                            plotter.identifier = identifier;
                            plotter.airfoilPrefabLocation = airfoilPrefabLocation;
                            plotter.airfoilShapePath = airfoilShapePath;
                            plotter.airfoilPerformancePath = airfoilPerformancePath;
                            if (fileType == FileType.AFL) { plotter.dataFormat = SilantroFoil.DataFormat.Afl; }
                            if (fileType == FileType.TXT) { plotter.dataFormat = SilantroFoil.DataFormat.Txt; }

                            //ACTIVATE
                            plotter.Create();

                            //RESET
                            identifier = "NACA 00000";
                            airfoilPerformancePath = null;
                            airfoilShapePath = null;
                        }
                    }

                    break;



                case "Edit":
                    //.....NAME
                    GUILayout.Space(5f);
                    silantroColor = Color.cyan;
                    GUI.color = Color.cyan;
                    EditorGUILayout.HelpBox("Selected Airfoil", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(3f);
                    airfoil = EditorGUILayout.ObjectField(" ", airfoil, typeof(SilantroAirfoil), true) as SilantroAirfoil;
                    if (airfoil == null)
                    {
                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Select a valid Airfoil..", MessageType.Error);
                        GUI.color = backgroundColor;
                    }


                    GUILayout.Space(10f);
                    editName = EditorGUILayout.Toggle("Edit Name", editName);
                    if (editName)
                    {
                        GUILayout.Space(5f);
                        GUI.color = Color.cyan;
                        EditorGUILayout.HelpBox("Airfoil Identifier", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(5f);
                        identifier = EditorGUILayout.TextField("New Identifier", identifier);
                    }

                    //.....DATA
                    GUILayout.Space(20f);
                    GUI.color = Color.cyan;
                    EditorGUILayout.HelpBox("Data Section", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Airfoil Shape", MessageType.None);
                    GUI.color = backgroundColor;
                    editShape = EditorGUILayout.Toggle("Replot Shape", editShape);
                    if (editShape)
                    {
                        GUILayout.Space(5f);
                        if (GUILayout.Button("Select Geometry File"))
                        {
                            airfoilShapePath = EditorUtility.OpenFilePanel("Foil Geometry", "", "txt");
                            if (airfoilShapePath.Length > 0)
                            {
                                shapeSelected = true;
                            }
                        }
                        GUILayout.Space(3f);
                        EditorGUILayout.TextField(" ", airfoilShapePath);
                    }

                    GUILayout.Space(15f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Airfoil Performance", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(5f);
                    editPerformance = EditorGUILayout.Toggle("Replot Polar Data", editPerformance);
                    if (editPerformance)
                    {
                        GUILayout.Space(5f);
                        fileType = (FileType)EditorGUILayout.EnumPopup("File Type", fileType);
                        GUILayout.Space(5f);
                        if (fileType == FileType.TXT)
                        {
                            if (GUILayout.Button("Select Polar File"))
                            {
                                airfoilPerformancePath = EditorUtility.OpenFilePanel("Foil Performance", "", "txt");
                                if (airfoilPerformancePath.Length > 0)
                                {
                                    dataSelected = true;
                                }
                            }
                        }
                        if (fileType == FileType.AFL)
                        {
                            if (GUILayout.Button("Select Polar File"))
                            {
                                airfoilPerformancePath = EditorUtility.OpenFilePanel("Foil Performance", "", "afl");
                                if (airfoilPerformancePath.Length > 0)
                                {
                                    dataSelected = true;
                                }
                            }
                        }
                        GUILayout.Space(3f);
                        EditorGUILayout.TextField(" ", airfoilPerformancePath);
                    }

                    GUILayout.Space(20f);
                    GUI.color = silantroColor;
                    EditorGUILayout.HelpBox("Make sure selected data is valid..", MessageType.Info);
                    GUI.color = backgroundColor;
                    //
                    if (airfoil != null)
                    {
                        GUILayout.Space(10f);
                        if (GUILayout.Button("Edit Wingfoil"))
                        {

                            GameObject manager = new GameObject("Airfoil Manager");
                            SilantroFoil plotter = manager.AddComponent<SilantroFoil>();
                            //SET DATA
                            plotter.airfoil = airfoil;
                            plotter.identifier = identifier;
                            plotter.airfoilPrefabLocation = airfoilPrefabLocation;
                            plotter.airfoilShapePath = airfoilShapePath;
                            plotter.airfoilPerformancePath = airfoilPerformancePath;
                            if (fileType == FileType.AFL)
                            {
                                plotter.dataFormat = SilantroFoil.DataFormat.Afl;
                            }
                            if (fileType == FileType.TXT)
                            {
                                plotter.dataFormat = SilantroFoil.DataFormat.Txt;
                            }
                            plotter.editName = editName;
                            plotter.editShape = editShape;
                            plotter.editPerformance = editPerformance;
                            //ACTIVATE
                            plotter.Edit();
                        }
                    }

                    break;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class SilantroFoil : MonoBehaviour
    {
        public string identifier = "NACA 00000";
        public string airfoilShapePath;
        public string airfoilPerformancePath;
        public string airfoilPrefabLocation;
        private readonly char lineSeperator = '\n';
        private readonly char fieldSeperator = '	';

        public enum DataFormat { Afl, Txt }
        public DataFormat dataFormat = DataFormat.Txt;
        //FOIL DATA
        bool reported;
        public List<Vector3> points = new List<Vector3>(); public List<float> xt = new List<float>();
        [HideInInspector] public SilantroAirfoil airfoil;

        public bool editShape;
        public bool editPerformance;
        public bool editName;

        /// <summary>
        /// 
        /// </summary>
        public void Create()
        {
            //-------------------SETUP PREFAB
            if (Directory.Exists(airfoilPrefabLocation))
            {
                airfoil = ScriptableObject.CreateInstance<SilantroAirfoil>();
                airfoil.Identifier = identifier;

                //RESET THE LISTS
                airfoil.alphas = new List<float>();
                airfoil.lift = new List<float>(); airfoil.drag = new List<float>(); airfoil.moment = new List<float>();
                airfoil.LD = new List<float>(); airfoil.AC = new List<float>(); airfoil.CP = new List<float>();
                //RESET CURVES
                airfoil.liftCurve = new AnimationCurve(); airfoil.dragCurve = new AnimationCurve();
                airfoil.momentCurve = new AnimationCurve(); airfoil.centerCurve = new AnimationCurve();
                airfoil.pressureCurve = new AnimationCurve();

                //--------------- Plot Shape
                if (airfoilShapePath != null)
                {
                    List<float> x = new List<float>();
                    List<float> y = new List<float>();
                    x = new List<float>(); y = new List<float>();
                    StreamReader shape = new StreamReader(airfoilShapePath);
                    string shapeText = shape.ReadToEnd();

                    string[] foilPlots = shapeText.Split(lineSeperator);
                    for (int j = 1; (j < foilPlots.Length - 2); j++)
                    {
                        string[] plots = foilPlots[j].Split(fieldSeperator);
                        float xj = float.Parse(plots[0], System.Globalization.NumberStyles.Any);
                        float yj = float.Parse(plots[1], System.Globalization.NumberStyles.Any);
                        x.Add(xj);
                        y.Add(yj);
                    }

                    points = new List<Vector3>();
                    Vector3 LeadingPoint = new Vector3(0, 0, 0);
                    Vector3 TrailingPoint = new Vector3(0, 0, 1);
                    if (x.Count > 0)
                    {
                        for (int j = 0; (j < x.Count); j++)
                        {
                            //BASE POINT
                            Vector3 XA = LeadingPoint - ((LeadingPoint - TrailingPoint).normalized * (x[j] * 1));
                            Vector3 liftDirection = transform.up;
                            Vector3 PointA = XA + (liftDirection * ((y[j]) * 1));
                            points.Add(PointA);
                        }
                    }
                    xt = new List<float>();
                    for (int j = 0; (j < points.Count); j++)
                    {
                        xt.Add(Vector3.Distance(points[j], points[(points.Count - j - 1)]));
                    }

                    float maximumThickness = xt.Max();
                    int thickPosition = xt.IndexOf(maximumThickness);
                    Vector3 thickPoint = points[thickPosition];
                    float thicknessLocation = Vector3.Distance(LeadingPoint, thickPoint);
                    float tc = (maximumThickness / 1) * 100f;
                    float xtc = (thicknessLocation / 1) * 100f;

                    float airfoilArea = (((xtc * 0.01f) + 3) / 6f) * (tc * 0.01f);

                    //STORE DATA
                    if (airfoil != null)
                    {
                        airfoil.x = x;
                        airfoil.y = y;
                        airfoil.xt = xt;
                        airfoil.maximumThickness = maximumThickness;
                        airfoil.thicknessLocation = thicknessLocation;
                        airfoil.tc = tc;
                        airfoil.xtc = xtc;
                        airfoil.airfoilArea = airfoilArea;
                    }
                }



                //--------------- Plot Performance
                if (airfoilPerformancePath != null)
                {
                    if (dataFormat == DataFormat.Txt)
                    {
                        StreamReader shape = new StreamReader(airfoilPerformancePath);
                        string shapeText = shape.ReadToEnd();
                        string[] foilPlots = shapeText.Split(lineSeperator);
                        for (int j = 0; (j < 2); j++)
                        {
                            //COLLECT REYNOLDS NUMBER
                            string[] dataPlot = foilPlots[1].Split(';');
                            string[] Reyds = dataPlot[1].Split(null);
                            if (Reyds[3] != null)
                            {
                                airfoil.ReynoldsNumber = (Reyds[3]);
                            }
                        }

                        string[] superPlots = shapeText.Split(lineSeperator);
                        for (int j = 5; (j < superPlots.Length - 3); j++)
                        {
                            string[] data = superPlots[j].Split(fieldSeperator);
                            float alpha = float.Parse(data[0]);
                            float cl = float.Parse(data[1]); float cd = float.Parse(data[2]);
                            float cm = float.Parse(data[3]); float ld = float.Parse(data[8]);
                            float ac = float.Parse(data[9]);
                            float cp = float.Parse(data[10]);
                            //STORE LISTS
                            airfoil.alphas.Add(alpha); airfoil.lift.Add(cl); airfoil.drag.Add(cd);
                            airfoil.moment.Add(cm); airfoil.LD.Add(ld); airfoil.AC.Add(ac); airfoil.CP.Add(cp);
                            //STORE CURVES
                            airfoil.liftCurve.AddKey(alpha, cl);
                            airfoil.dragCurve.AddKey(alpha, cd);
                            airfoil.momentCurve.AddKey(alpha, cm);
                            airfoil.centerCurve.AddKey(alpha, ac);
                            airfoil.pressureCurve.AddKey(alpha, cp);
                        }
                        airfoil.detailed = true;
                    }

                    if (dataFormat == DataFormat.Afl)
                    {
                        StreamReader shape = new StreamReader(airfoilPerformancePath);
                        string shapeText = shape.ReadToEnd();
                        string[] superPlots = shapeText.Split(lineSeperator);
                        for (int j = 5; (j < superPlots.Length - 2); j++)
                        {
                            string[] data = superPlots[j].Split(null);
                            float alpha = float.Parse(data[1]);
                            float cl = float.Parse(data[2]);
                            float cd = float.Parse(data[3]);
                            float cm = float.Parse(data[4]);
                            airfoil.LD.Add((cl / cd));
                            //STORE
                            airfoil.alphas.Add(alpha); airfoil.lift.Add(cl); airfoil.drag.Add(cd); airfoil.moment.Add(cm);
                            //STORE CURVES
                            airfoil.liftCurve.AddKey(alpha, cl);
                            airfoil.dragCurve.AddKey(alpha, cd);
                            airfoil.momentCurve.AddKey(alpha, cm);
                        }
                    }
                    //
                    airfoil.maxCd = airfoil.drag.Max();
                    float maxCl = airfoil.lift.Max();
                    int indexOfStall = airfoil.lift.IndexOf(maxCl);
                    airfoil.stallAngle = airfoil.alphas[indexOfStall];
                    airfoil.maxClCd = airfoil.LD.Max();
                    float CenterSum = 0f;
                    for (int i = 0; i < airfoil.AC.Count; i++)
                    {
                        float currentAC = airfoil.AC[i];
                        if (currentAC > 0.27f) { currentAC = 0.27f; }
                        if (currentAC < 0.23f) { currentAC = 0.23f; }
                        CenterSum += currentAC;
                    }
                    airfoil.aerodynamicCenter = (CenterSum / airfoil.AC.Count) * 100f;
                }

                MathBase.SmoothCurve(airfoil.liftCurve);
                MathBase.SmoothCurve(airfoil.dragCurve);
                MathBase.SmoothCurve(airfoil.momentCurve);

                //SAVE DATABASE
                AssetDatabase.CreateAsset(airfoil, airfoilPrefabLocation + identifier + ".asset");
                AssetDatabase.SaveAssets();

                if (!reported && this.gameObject != null)
                {
                    reported = true;
                    Debug.Log("Airfoil : " + identifier + " Created Successfully at: " + airfoilPrefabLocation);
                    DestroyImmediate(this.gameObject);
                }
            }

            if (!Directory.Exists(airfoilPrefabLocation))
            {
                reported = true;
                Debug.LogError("Provided save location does not exist!");
                DestroyImmediate(this.gameObject);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Edit()
        {
            if (editName && identifier != "NACA 00000")
            {
                airfoil.Identifier = identifier;
            }
            //RESET THE LISTS
            airfoil.alphas = new List<float>();
            airfoil.lift = new List<float>(); airfoil.drag = new List<float>(); airfoil.moment = new List<float>();
            airfoil.LD = new List<float>(); airfoil.AC = new List<float>(); airfoil.CP = new List<float>();

            //PLOT SHAPE
            if (airfoilShapePath != null && editShape)
            {
                List<float> x = new List<float>();
                List<float> y = new List<float>();
                x = new List<float>(); y = new List<float>();
                StreamReader shape = new StreamReader(airfoilShapePath);
                string shapeText = shape.ReadToEnd();

                string[] foilPlots = shapeText.Split(lineSeperator);
                for (int j = 1; (j < foilPlots.Length - 2); j++)
                {
                    string[] plots = foilPlots[j].Split(fieldSeperator);
                    float xj = float.Parse(plots[0], System.Globalization.NumberStyles.Any);
                    float yj = float.Parse(plots[1], System.Globalization.NumberStyles.Any);
                    x.Add(xj);
                    y.Add(yj);
                }

                points = new List<Vector3>();
                Vector3 LeadingPoint = new Vector3(0, 0, 0);
                Vector3 TrailingPoint = new Vector3(0, 0, 1);
                if (x.Count > 0)
                {
                    for (int j = 0; (j < x.Count); j++)
                    {
                        //BASE POINT
                        Vector3 XA = LeadingPoint - ((LeadingPoint - TrailingPoint).normalized * (x[j] * 1));
                        Vector3 liftDirection = transform.up;
                        Vector3 PointA = XA + (liftDirection * ((y[j]) * 1));
                        points.Add(PointA);
                    }
                }
                xt = new List<float>();
                for (int j = 0; (j < points.Count); j++)
                {
                    xt.Add(Vector3.Distance(points[j], points[(points.Count - j - 1)]));
                }

                float maximumThickness = xt.Max();
                int thickPosition = xt.IndexOf(maximumThickness);
                Vector3 thickPoint = points[thickPosition];
                float thicknessLocation = Vector3.Distance(LeadingPoint, thickPoint);
                float tc = (maximumThickness / 1) * 100f;
                float xtc = (thicknessLocation / 1) * 100f;

                float airfoilArea = (((xtc * 0.01f) + 3) / 6f) * (tc * 0.01f);

                //STORE DATA
                if (airfoil != null)
                {
                    airfoil.x = x;
                    airfoil.y = y;
                    airfoil.xt = xt;
                    airfoil.maximumThickness = maximumThickness;
                    airfoil.thicknessLocation = thicknessLocation;
                    airfoil.tc = tc;
                    airfoil.xtc = xtc;
                    airfoil.airfoilArea = airfoilArea;
                }
            }

            //PLOT DATA
            if (airfoilPerformancePath != null && editPerformance)
            {
                //RESET CURVES
                airfoil.liftCurve = new AnimationCurve();
                airfoil.dragCurve = new AnimationCurve();
                airfoil.momentCurve = new AnimationCurve();
                airfoil.centerCurve = new AnimationCurve();
                airfoil.pressureCurve = new AnimationCurve();


                if (dataFormat == DataFormat.Txt)
                {
                    StreamReader shape = new StreamReader(airfoilPerformancePath);
                    string shapeText = shape.ReadToEnd();
                    string[] foilPlots = shapeText.Split(lineSeperator);
                    for (int j = 0; (j < 2); j++)
                    {
                        //COLLECT REYNOLDS NUMBER
                        string[] dataPlot = foilPlots[1].Split(';');
                        string[] Reyds = dataPlot[1].Split(null);
                        if (Reyds[3] != null)
                        {
                            airfoil.ReynoldsNumber = (Reyds[3]);
                        }
                    }

                    string[] superPlots = shapeText.Split(lineSeperator);
                    for (int j = 5; (j < superPlots.Length - 3); j++)
                    {
                        string[] data = superPlots[j].Split(fieldSeperator);
                        float alpha = float.Parse(data[0]);
                        float cl = float.Parse(data[1]); float cd = float.Parse(data[2]);
                        float cm = float.Parse(data[3]); float ld = float.Parse(data[8]);
                        float ac = float.Parse(data[9]);
                        float cp = float.Parse(data[10]);
                        //STORE LISTS
                        airfoil.alphas.Add(alpha); airfoil.lift.Add(cl); airfoil.drag.Add(cd);
                        airfoil.moment.Add(cm); airfoil.LD.Add(ld); airfoil.AC.Add(ac); airfoil.CP.Add(cp);
                        //STORE CURVES
                        airfoil.liftCurve.AddKey(alpha, cl);
                        airfoil.dragCurve.AddKey(alpha, cd);
                        airfoil.momentCurve.AddKey(alpha, cm);
                        airfoil.centerCurve.AddKey(alpha, ac);
                        airfoil.pressureCurve.AddKey(alpha, cp);
                    }
                    airfoil.detailed = true;
                }

                if (dataFormat == DataFormat.Afl)
                {
                    StreamReader shape = new StreamReader(airfoilPerformancePath);
                    string shapeText = shape.ReadToEnd();
                    string[] superPlots = shapeText.Split(lineSeperator);
                    for (int j = 5; (j < superPlots.Length - 2); j++)
                    {
                        string[] data = superPlots[j].Split(null);
                        float alpha = float.Parse(data[1]);
                        float cl = float.Parse(data[2]);
                        float cd = float.Parse(data[3]);
                        float cm = float.Parse(data[4]);
                        //STORE
                        airfoil.alphas.Add(alpha); airfoil.lift.Add(cl); airfoil.drag.Add(cd); airfoil.moment.Add(cm);
                        airfoil.LD.Add(cl / cd);
                        //STORE CURVES
                        airfoil.liftCurve.AddKey(alpha, cl);
                        airfoil.dragCurve.AddKey(alpha, cd);
                        airfoil.momentCurve.AddKey(alpha, cm);
                    }
                }
                //
                airfoil.maxCd = airfoil.drag.Max();
                float maxCl = airfoil.lift.Max();
                int indexOfStall = airfoil.lift.IndexOf(maxCl);
                airfoil.stallAngle = airfoil.alphas[indexOfStall];
                if (airfoil.LD.Count > 0)
                {
                    airfoil.maxClCd = airfoil.LD.Max();
                }

                float CenterSum = 0f;
                for (int i = 0; i < airfoil.AC.Count; i++)
                {
                    float currentAC = airfoil.AC[i];
                    if (currentAC > 0.27f) { currentAC = 0.27f; }
                    if (currentAC < 0.23f) { currentAC = 0.23f; }
                    CenterSum += currentAC;
                }
                airfoil.aerodynamicCenter = (CenterSum / airfoil.AC.Count) * 100f;
            }
            if (!reported && this.gameObject != null)
            {
                reported = true;
                Debug.Log("Airfoil : " + identifier + " Edited Successfully!");
                DestroyImmediate(this.gameObject);
            }
        }
    }
}
