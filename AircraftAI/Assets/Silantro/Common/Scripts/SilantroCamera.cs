using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif




namespace Oyedoyin.Common
{
    #region Component
    /// <summary>
    ///
    /// 
    /// Function:		 Handles the movement and rotation of the aircraft cameras
    /// </summary>
    public class SilantroCamera : MonoBehaviour
    {
        //----------------------------------------------SELECTIBLES
        public enum CameraFocus { Normal, VR }
        public CameraFocus cameraFocus = CameraFocus.Normal;
        public enum CameraState { Interior, Exterior }
        public CameraState cameraState = CameraState.Exterior;
        public enum CameraMode { Orbit, Free, ByStand }
        public CameraMode cameraMode = CameraMode.Orbit;
        public enum CameraStartState { Interior, Exterior }
        public CameraStartState startState = CameraStartState.Exterior;
        public enum CameraAttachment { None, ExteriorOnly, Dual }
        public CameraAttachment attachment = CameraAttachment.None;

        public GameObject exteriorObject, interiorObject;
        public enum CameraShake { Active, Off }
        public CameraShake cameraShake = CameraShake.Off;

        //----------------------------------------------CONNECTIONS
        public Rigidbody aircraft;
        public Controller controller;
        public Camera normalExterior;
        public Camera normalInterior;
        public Transform vrExterior;
        public Transform vrInterior;
        public Transform focusPoint;
        public Camera currentCamera;
        public Camera[] sceneCameras;


        public float zoomSensitivity = 3;
        public bool zoomEnabled = true;
        public float maximumFOV = 20, currentFOV, baseFOV;

        //------------------------------------------ORBIT CAMERA
        public float orbitDistance = 20.0f;
        public float orbitHeight = 2.0f;
        private bool FirstClick = false;
        private float orbitAngle = 180.0f;
        public float verticalAngle;
        Vector3 cameraRange, cameraPosition;
        Vector3 basePositionition;
        public float maximumInteriorVolume = 0.8f;

        //------------------------------------------FREE CAMERA
        public float azimuthSensitivity = 1;
        public float elevationSensitivity = 1;
        public float radiusSensitivity = 10;
        private float azimuth, elevation;
        public float radius;
        public float maximumRadius = 20f;
        Vector3 filterPosition; float filerY; Vector3 filteredPosition;
        Vector3 cameraDirection;

        //------------------------------------------INTERIOR CAMERA
        public float viewSensitivity = 80f;
        public float maximumViewAngle = 80f;
        float verticalRotation, horizontalRotation;
        Vector3 baseCameraPosition;
        Quaternion baseCameraRotation, currentCameraRotation;
        public GameObject pilotObject;
        public float mouseSensitivity = 100.0f;
        public float clampAngle = 80.0f;
        public float scrollValue;

        //------------------------------------------GENERAL
        public float currentCameraSector;
        static float jester;
        public bool allOk;

        // ----------------------------------------- Shake
        public float shakeStrength = 0.1f;
        public float shakeSpeed = 5f;
        public float maximumStrength = 2f;
        public Vector3 basePosition, predictedPosition;
        public bool shakeFactor = true;
        public float shakeLevel;
        float time = 10f;

        public float xHatInput;
        public float yHatInput;
        public float xMouseInput;
        public float yMouseInput;
        public float xInput, yInput;

        private void Update()
        {
            if (allOk)
            {
                if (controller != null && controller.isControllable)
                {
                    if (cameraState == CameraState.Exterior)
                    {
                        //--------------ORBIT
                        if (cameraMode == CameraMode.Orbit) { AnalyseOrbitCamera(); }

                        //--------------FREE
                        if (cameraMode == CameraMode.Free) { AnalyseFreeCamera(); }
                    }
                    else { AnalyseInteriorCamera(); }
                }
            }
        }



        // ---------------------------------------------------------CONTROL FUNCTIONS-------------------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        public void ActivateInteriorCamera()
        {
            if (cameraFocus == CameraFocus.Normal)
            {
                // ------------------ Normal Interior Camera
                if (normalInterior != null)
                {
                    normalInterior.enabled = true;
                    AudioListener interiorListener = normalInterior.GetComponent<AudioListener>();
                    if (interiorListener != null) { interiorListener.enabled = true; }
                }
                else { Debug.Log("Interior Camera has not been setup"); return; }


                // ------------------ Normal Exterior Camera
                normalExterior.enabled = false; currentCamera = normalInterior;
                AudioListener exteriorListener = normalExterior.GetComponent<AudioListener>();
                if (exteriorListener != null) { exteriorListener.enabled = false; }
            }

            if (attachment != CameraAttachment.None)
            {
                if (exteriorObject != null && exteriorObject.activeSelf) { exteriorObject.SetActive(false); }
                if (interiorObject != null && !interiorObject.activeSelf) { interiorObject.SetActive(true); }
            }

            // ------------------ Pilot
            if (pilotObject != null && pilotObject.activeSelf) { pilotObject.SetActive(false); }
            cameraState = CameraState.Interior;
            if (controller != null) { controller.m_cameraState = CameraState.Interior; }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ActivateExteriorCamera()
        {

            if (cameraFocus == CameraFocus.Normal)
            {
                // ------------------ Normal Interior Camera
                if (normalInterior != null)
                {
                    normalInterior.enabled = false;
                    AudioListener interiorListener = normalInterior.GetComponent<AudioListener>();
                    if (interiorListener != null) { interiorListener.enabled = false; }
                }

                // ------------------ Normal Exterior Camera
                normalExterior.enabled = true;
                currentCamera = normalExterior;
                AudioListener exteriorListener = normalExterior.GetComponent<AudioListener>();
                if (exteriorListener != null) { exteriorListener.enabled = true; }
            }

            if (attachment != CameraAttachment.None)
            {
                if (exteriorObject != null && !exteriorObject.activeSelf) { exteriorObject.SetActive(true); }
                if (interiorObject != null && interiorObject.activeSelf) { interiorObject.SetActive(false); }
            }

            //if (controller != null && controller.core != null) { controller.core.ResetSound(); }

            // ------------------ Pilot
            if (pilotObject != null && !pilotObject.activeSelf) { pilotObject.SetActive(true); }
            cameraState = CameraState.Exterior;
            if (controller != null) { controller.m_cameraState = CameraState.Exterior; }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ToggleCamera()
        {
            if (cameraState == CameraState.Exterior) { ActivateInteriorCamera(); }
            else { ActivateExteriorCamera(); }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ResetCameras()
        {
            if (normalExterior != null)
            {
                normalExterior.enabled = false;
                if (normalExterior.GetComponent<AudioListener>() != null) { normalExterior.GetComponent<AudioListener>().enabled = false; }
            }

            if (normalInterior != null)
            {
                normalInterior.enabled = false;
                if (normalInterior.GetComponent<AudioListener>() != null) { normalInterior.GetComponent<AudioListener>().enabled = false; }
            }
        }





        /// <summary>
        /// 
        /// </summary>
        protected void CheckPrerequisites()
        {
            if (focusPoint == null) { focusPoint = this.transform; }

            if (cameraFocus == CameraFocus.Normal)
            {
                if (normalExterior == null) { Debug.LogError("Prerequisites not met on Camera " + transform.name + "....Exterior camera not assigned"); allOk = false; return; }
                else { allOk = true; }
            }
            if (cameraFocus == CameraFocus.VR)
            {
                if (vrExterior == null) { Debug.LogError("Prerequisites not met on Camera " + transform.name + "....Exterior VR camera holder not assigned"); allOk = false; return; }
                else { allOk = true; }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {

            //CHECK COMPONENTS
            CheckPrerequisites();


            if (allOk)
            {
                // Disable all other camera listeners
                sceneCameras = Camera.allCameras;
                foreach (Camera cam in sceneCameras)
                {
                    //cam.enabled = false;
                    AudioListener Listener = cam.GetComponent<AudioListener>();
                    if (Listener != null) { Listener.enabled = false; }
                }

                // Configure aircraft properties
                if (normalExterior != null && normalExterior.GetComponent<AudioListener>() == null) { normalExterior.gameObject.AddComponent<AudioListener>(); }
                if (normalInterior != null && normalInterior.GetComponent<AudioListener>() == null) { normalInterior.gameObject.AddComponent<AudioListener>(); }

                ResetCameras();
                if (cameraMode == CameraMode.Orbit && normalExterior != null)
                {
                    CartesianToSpherical(focusPoint.transform.InverseTransformDirection(normalExterior.transform.position - focusPoint.transform.position), out radius, out azimuth, out elevation);
                    normalExterior.transform.LookAt(focusPoint.transform);
                }
                if (normalInterior != null)
                {
                    baseCameraPosition = normalInterior.transform.localPosition;
                    baseCameraRotation = normalInterior.transform.localRotation;
                    baseFOV = normalInterior.fieldOfView;
                }

                if (controller != null && controller.m_controlType == Controller.ControlType.External)
                {
                    // ------------------------ Start Mode
                    if (startState == CameraStartState.Exterior) { ActivateExteriorCamera(); }
                    else { ActivateInteriorCamera(); }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void AnalyseOrbitCamera()
        {
#if (ENABLE_LEGACY_INPUT_MANAGER)
            //----------------SET START POSITION
            if (Input.GetMouseButton(0) && Application.isFocused && controller.touchPressed == false)
            {
                if (FirstClick) { basePositionition = Input.mousePosition; FirstClick = false; }
                if (Application.isFocused)
                {
                    orbitAngle += (Input.mousePosition - basePositionition).x * controller._timestep;
                }
            }
            else { FirstClick = true; }
#endif

            //-----------------PROCESS VARIABLES
            cameraRange = focusPoint.forward; cameraRange.y = 0f; cameraRange.Normalize();
            cameraRange = Quaternion.Euler(verticalAngle, orbitAngle, 0) * cameraRange;
            cameraPosition = focusPoint.transform.position;
            cameraPosition += cameraRange * orbitDistance;
            cameraPosition += Vector3.up * orbitHeight;

            //----------------APPLY
            if (cameraFocus == CameraFocus.Normal) { normalExterior.transform.position = cameraPosition; normalExterior.transform.LookAt(focusPoint.position); }
            if (cameraFocus == CameraFocus.VR) { vrExterior.position = cameraPosition; vrExterior.LookAt(focusPoint.position); }
        }
        /// <summary>
        /// 
        /// </summary>
        private void AnalyseFreeCamera()
        {

#if (ENABLE_LEGACY_INPUT_MANAGER)
            if (Input.GetMouseButton(0))
            {
                xMouseInput = Input.GetAxis("Mouse X");
                yMouseInput = Input.GetAxis("Mouse Y");
            }
            else { xMouseInput = yMouseInput = 0; }
#endif

            if (Application.isFocused && controller.touchPressed == false)
            {
                xInput = xMouseInput + xHatInput;
                yInput = yMouseInput + yHatInput;
                azimuth -= xInput * azimuthSensitivity * controller._timestep;
                elevation -= yInput * elevationSensitivity * controller._timestep;
            }


            //CALCULATE DIRECTION AND POSITION
            SphericalToCartesian(radius, azimuth, elevation, out cameraDirection);
            //CLAMP ROTATION IF AIRCRAFT IS ON THE GROUND//LESS THAN radius meters
            if (focusPoint.position.y < maximumRadius)
            {
                filterPosition = focusPoint.position + cameraDirection;
                filerY = filterPosition.y;
                if (filerY < 2) filerY = 2;
                filteredPosition = new Vector3(filterPosition.x, filerY, filterPosition.z);
                normalExterior.transform.position = filteredPosition;
            }
            else
            {
                normalExterior.transform.position = focusPoint.position + cameraDirection; ;
            }


            //POSITION CAMERA
            normalExterior.transform.LookAt(focusPoint);
            radius = maximumRadius;//use this to control the distance from the aircraft
        }
        /// <summary>
        /// 
        /// </summary>
        private void LateUpdate()
        {
            if (cameraShake == CameraShake.Active && cameraState == CameraState.Interior)
            {
                time += controller._timestep;
                float currentShakeStrength = shakeStrength * shakeLevel;
                float currentShakeSpeed = shakeSpeed * shakeLevel;

                predictedPosition = (Mathf.PerlinNoise(time * currentShakeSpeed, time * currentShakeSpeed * 2) - 0.5f) * currentShakeStrength * transform.right
                    + (Mathf.PerlinNoise(time * currentShakeSpeed * 2, time * currentShakeSpeed) - 0.5f) * currentShakeStrength * transform.up;
                normalInterior.transform.Translate(shakeFactor ? (predictedPosition - basePosition) : predictedPosition);
                basePosition = predictedPosition;
                if (time > 10) { time = 9f; }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void AnalyseInteriorCamera()
        {
            if (cameraFocus == CameraFocus.Normal)
            {
                if (normalInterior != null)
                {
#if (ENABLE_LEGACY_INPUT_MANAGER)
                    if (Input.GetMouseButton(0))
                    {
                        xMouseInput = Input.GetAxis("Mouse X");
                        yMouseInput = Input.GetAxis("Mouse Y");
                    }
                    else { xMouseInput = 0; yMouseInput = 0; }
#endif

                    if (Application.isFocused && controller.touchPressed == false)
                    {
                        xInput = xMouseInput + xHatInput;
                        yInput = yMouseInput + yHatInput;
                        verticalRotation += xInput * mouseSensitivity * controller._timestep;
                        horizontalRotation += -yInput * mouseSensitivity * controller._timestep;
                    }

                    //CLAMP ANGLES (You can make them independent to have a different maximum for each)
                    horizontalRotation = Mathf.Clamp(horizontalRotation, -clampAngle, clampAngle);
                    verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);
                    //ASSIGN ROTATION
                    currentCameraRotation = Quaternion.Euler(horizontalRotation, verticalRotation, 0.0f);
                    normalInterior.transform.localRotation = currentCameraRotation;
                }

                //ZOOM
                if (zoomEnabled)
                {
                    currentFOV = normalInterior.fieldOfView;
#if (ENABLE_LEGACY_INPUT_MANAGER)
                    currentFOV += Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;
#endif
                    currentFOV = Mathf.Clamp(currentFOV, maximumFOV, baseFOV);
                    normalInterior.fieldOfView = currentFOV;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void PlayerSystem()
        {
            //CALCULATE CAMERA ANGLE
            cameraRange = focusPoint.transform.forward;
            cameraRange.y = 0f; cameraRange.Normalize();
            cameraRange = Quaternion.Euler(0, orbitAngle, 0) * cameraRange;

            //CALCULATE CAMERA POSITION
            cameraPosition = focusPoint.transform.position;
            cameraPosition += cameraRange * orbitDistance;
            cameraPosition += focusPoint.up * orbitHeight;

            //APPLY TO CAMERA
            normalExterior.transform.position = cameraPosition;
            normalExterior.transform.LookAt(focusPoint.transform.position);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="referencePoint"></param>
        /// <returns></returns>
        public float AnalyseCameraAngle()
        {
            //--------------ESTIMATE SECTOR
            float angle = normalExterior.transform.localEulerAngles.y - 90;
            if (angle < 0) { angle += 360f; }

            return angle;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cordinatesInput"></param>
        /// <param name="outRadius"></param>
        /// <param name="outPolar"></param>
        /// <param name="outElevation"></param>
        static void CartesianToSpherical(Vector3 cordinatesInput, out float outRadius, out float outPolar, out float outElevation)
        {
            if (cordinatesInput.x == 0)
                cordinatesInput.x = Mathf.Epsilon;
            outRadius = Mathf.Sqrt((cordinatesInput.x * cordinatesInput.x) + (cordinatesInput.y * cordinatesInput.y) + (cordinatesInput.z * cordinatesInput.z));
            outPolar = Mathf.Atan(cordinatesInput.z / cordinatesInput.x);
            if (cordinatesInput.x < 0) outPolar += Mathf.PI;
            outElevation = Mathf.Asin(cordinatesInput.y / outRadius);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="polar"></param>
        /// <param name="elevation"></param>
        /// <param name="outCart"></param>
        public static void SphericalToCartesian(float radius, float polar, float elevation, out Vector3 outCart)
        {
            jester = radius * Mathf.Cos(elevation);
            outCart.x = jester * Mathf.Cos(polar);
            outCart.y = radius * Mathf.Sin(elevation);
            outCart.z = jester * Mathf.Sin(polar);
        }
    }
    #endregion

    #region Editor
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroCamera))]
    public class SilantroCameraEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1.0f, 0.40f, 0f);
        SilantroCamera camera;
        public int toolbarTab;
        public string currentTab;

        private SerializedProperty cameraType;
        private SerializedProperty cameraFocus;
        private SerializedProperty cameraMode;
        private SerializedProperty startState;

        private SerializedProperty baseExteriorCamera;
        private SerializedProperty baseInteriorCamera;
        private SerializedProperty vrExteriorCamera;
        private SerializedProperty vrInteriorCamera;
        private SerializedProperty lookPosition;

        private SerializedProperty zoomToggle;
        private SerializedProperty zoomMaximumFOV;
        private SerializedProperty zoomSensitivity;

        private SerializedProperty orbitDistance;
        private SerializedProperty orbitHeight;
        private SerializedProperty aziSensitivity;
        private SerializedProperty eleSensitivity;

        private SerializedProperty baseSensitivity;
        private SerializedProperty viewAngle;
        private SerializedProperty maxRadius;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            camera = (SilantroCamera)target;

            cameraType = serializedObject.FindProperty("cameraType");
            cameraFocus = serializedObject.FindProperty("cameraFocus");
            cameraMode = serializedObject.FindProperty("cameraMode");
            startState = serializedObject.FindProperty("startState");

            baseExteriorCamera = serializedObject.FindProperty("normalExterior");
            baseInteriorCamera = serializedObject.FindProperty("normalInterior");
            vrExteriorCamera = serializedObject.FindProperty("vrExterior");
            vrInteriorCamera = serializedObject.FindProperty("vrInterior");

            zoomToggle = serializedObject.FindProperty("zoomEnabled");
            zoomSensitivity = serializedObject.FindProperty("zoomSensitivity");
            zoomMaximumFOV = serializedObject.FindProperty("maximumFOV");

            maxRadius = serializedObject.FindProperty("maximumRadius");

            orbitHeight = serializedObject.FindProperty("orbitHeight");
            orbitDistance = serializedObject.FindProperty("orbitDistance");
            lookPosition = serializedObject.FindProperty("focusPoint");

            viewAngle = serializedObject.FindProperty("clampAngle");
            baseSensitivity = serializedObject.FindProperty("mouseSensitivity");

            aziSensitivity = serializedObject.FindProperty("azimuthSensitivity");
            eleSensitivity = serializedObject.FindProperty("elevationSensitivity");
        }
        /// <summary>
        /// 
        /// </summary>
        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector();
            serializedObject.Update();


            GUILayout.Space(4f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Functionality Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(cameraFocus);
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(startState);
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attachment"), new GUIContent("Attachment"));

            GUILayout.Space(10f);
            toolbarTab = GUILayout.Toolbar(toolbarTab, new string[] { "Exterior Camera", "Interior Camera" });
            switch (toolbarTab)
            {
                case 0: currentTab = "Exterior Camera"; break;
                case 1: currentTab = "Interior Camera"; break;
            }

            switch (currentTab)
            {
                case "Exterior Camera":
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(baseExteriorCamera, new GUIContent("Exterior Camera"));
                    GUILayout.Space(7f);
                    EditorGUILayout.PropertyField(cameraMode, new GUIContent(" "));

                    if (camera.cameraMode == SilantroCamera.CameraMode.Orbit)
                    {
                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Orbit Camera Settings", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(lookPosition);
                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Camera Positioning", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(orbitDistance);
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(orbitHeight);
                    }

                    if (camera.cameraMode == SilantroCamera.CameraMode.Free)
                    {
                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Free Camera Settings", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        EditorGUILayout.PropertyField(lookPosition);
                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Camera Movement", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        eleSensitivity.floatValue = EditorGUILayout.Slider("Elevation Sensitivity", eleSensitivity.floatValue, 0f, 5f);
                        GUILayout.Space(3f);
                        aziSensitivity.floatValue = EditorGUILayout.Slider("Azimuth Sensitivity", aziSensitivity.floatValue, 0f, 5f);
                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Camera Positioning", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(2f);
                        camera.maximumRadius = EditorGUILayout.FloatField("Camera Distance", camera.maximumRadius);


                    }

                    if (camera.attachment != SilantroCamera.CameraAttachment.None)
                    {
                        GUILayout.Space(10f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Exterior Attachment (e.g Pilot Body)", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("exteriorObject"), new GUIContent(" "));
                    }

                    break;
                case "Interior Camera":
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(baseInteriorCamera, new GUIContent("Interior Camera"));
                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Camera Movement", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(2f);
                    baseSensitivity.floatValue = EditorGUILayout.Slider("Mouse Sensitivity", baseSensitivity.floatValue, 0f, 100f);
                    GUILayout.Space(5f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Camera Positioning", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(2f);
                    viewAngle.floatValue = EditorGUILayout.Slider("View Angle", viewAngle.floatValue, 10f, 210f);


                    GUILayout.Space(10f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Zoom Settings", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(5f);
                    EditorGUILayout.PropertyField(zoomToggle);
                    if (camera.zoomEnabled)
                    {
                        GUILayout.Space(3f);
                        zoomMaximumFOV.floatValue = EditorGUILayout.Slider("Minimum FOV", zoomMaximumFOV.floatValue, 0f, 100f);
                        GUILayout.Space(3f);
                        zoomSensitivity.floatValue = EditorGUILayout.Slider("Zoom Sensitivity", zoomSensitivity.floatValue, 0f, 20f);
                    }

                    if (camera.attachment != SilantroCamera.CameraAttachment.None && camera.attachment != SilantroCamera.CameraAttachment.ExteriorOnly)
                    {
                        GUILayout.Space(10f);
                        GUI.color = Color.white;
                        EditorGUILayout.HelpBox("Interior Attachment", MessageType.None);
                        GUI.color = backgroundColor;
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("interiorObject"), new GUIContent(" "));
                    }


                    GUILayout.Space(10f);
                    GUI.color = Color.white;
                    EditorGUILayout.HelpBox("Vibration Settings", MessageType.None);
                    GUI.color = backgroundColor;
                    GUILayout.Space(5f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraShake"), new GUIContent("Shake State"));
                    if (camera.cameraShake == SilantroCamera.CameraShake.Active)
                    {
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("shakeStrength"), new GUIContent("Shake Strength"));
                        GUILayout.Space(3f);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("shakeSpeed"), new GUIContent("Shake Speed"));
                    }
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    #endregion
}