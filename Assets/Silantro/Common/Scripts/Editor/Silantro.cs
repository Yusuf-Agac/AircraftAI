using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System.Collections;
using Oyedoyin.Common.Misc;
using System.Collections.Generic;


/// <summary>
/// 
/// </summary>
namespace Oyedoyin.Common.Editors
{
    public class CommonElements
    {
        [MenuItem("Oyedoyin/Common/Components/Core", false, 300)]
        private static void COG()
        {
            GameObject box;
            if (Selection.activeGameObject != null)
            {
                box = new GameObject();
                box.transform.parent = Selection.activeGameObject.transform;
            }
            else { box = new GameObject(); }

            box.name = "m_core";
            SilantroCore _core = box.AddComponent<SilantroCore>();
            box.transform.localPosition = Vector3.zero;

            GameObject ecog = new GameObject("_empty_cog");
            ecog.transform.parent = box.transform;
            ecog.transform.localPosition = Vector3.zero;

            _core.functionality = SilantroCore.SystemType.Advanced;
            _core.emptyCenterOfMass = ecog.transform;

            EditorSceneManager.MarkSceneDirty(box.scene);
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Components/Fuel Tank/Internal", false, 400)]
        private static void AddInternalTank()
        {
            GameObject tank;
            if (Selection.activeGameObject != null)
            {
                tank = new GameObject();
                tank.transform.parent = Selection.activeGameObject.transform;
                tank.transform.localPosition = Vector3.zero;
                EditorSceneManager.MarkSceneDirty(tank.scene);
                tank.name = "m_interior_tank";
                SilantroTank fuelTank = tank.AddComponent<SilantroTank>();
                fuelTank.tankType = SilantroTank.TankType.Internal;
                EditorSceneManager.MarkSceneDirty(tank.scene);
            }
            else
            {
                Debug.Log("Please select a container gameobject to parent the tanks to!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Components/Fuel Tank/External", false, 500)]
        private static void AddExternalTank()
        {
            GameObject tank;
            if (Selection.activeGameObject != null)
            {
                tank = new GameObject();
                tank.transform.parent = Selection.activeGameObject.transform; tank.transform.localPosition = Vector3.zero;
            }
            else
            {
                tank = new GameObject();
            }
            tank.name = "m_exterior_tank";
            EditorSceneManager.MarkSceneDirty(tank.scene);
            SilantroTank fuelTank = tank.AddComponent<SilantroTank>();
            fuelTank.tankType = SilantroTank.TankType.External;
            EditorSceneManager.MarkSceneDirty(tank.scene);
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Components/Actuator/Control Surface/Create", false, 600)]
        private static void AddSurfaceActuator()
        {
            if (Selection.activeGameObject != null)
            {
                GameObject box;
                box = new GameObject("m_surface_actuator");
                box.transform.parent = Selection.activeGameObject.transform;
                SilantroActuator actuator = box.AddComponent<SilantroActuator>();
                actuator.animationName = "Default Control Surface";
                actuator.actuatorType = SilantroActuator.ActuatorType.ControlSurface;
                actuator.actuationSpeed = 0.3f;
                EditorSceneManager.MarkSceneDirty(actuator.gameObject.scene);
            }
            else
            {
                Debug.Log("Please Select a gameobject to parent the actuator to");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Components/Actuator/Control Surface/Tutorial", false, 700)]
        public static void SurfaceTutorial()
        {
            Application.OpenURL("https://youtu.be/JOyYldBrJ_I");
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Components/Actuator/Door", false, 800)]
        private static void AddDoorActuator()
        {
            if (Selection.activeGameObject != null)
            {
                GameObject box;
                box = new GameObject("m_door_actuator");
                box.transform.parent = Selection.activeGameObject.transform;
                SilantroActuator actuator = box.AddComponent<SilantroActuator>();
                actuator.actuatorType = SilantroActuator.ActuatorType.Door;

                AudioClip end = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Sounds/Canopy/CanopyOpenEnd.wav", typeof(AudioClip));
                AudioClip loop = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Sounds/Canopy/CanopyOpen.wav", typeof(AudioClip));

                actuator.EngageLoopClip = loop;
                actuator.EngageEndClip = end;

                actuator.animationName = "Default Door";
                EditorSceneManager.MarkSceneDirty(actuator.gameObject.scene);
            }
            else
            {
                Debug.Log("Please Select a gameobject to parent the actuator to");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Components/Actuator/Landing Gear/Create", false, 900)]
        private static void AddGearActuator()
        {
            if (Selection.activeGameObject != null)
            {
                GameObject box;
                box = new GameObject("m_gear_actuator");
                box.transform.parent = Selection.activeGameObject.transform;
                SilantroActuator actuator = box.AddComponent<SilantroActuator>();
                actuator.actuatorType = SilantroActuator.ActuatorType.LandingGear;
                AudioClip end = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Sounds/Gear/Pneumatic Gear End.wav", typeof(AudioClip));
                AudioClip loop = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Silantro/Common/Sounds/Gear/Pneumatic Gear Loop.wav", typeof(AudioClip));


                actuator.EngageLoopClip = loop;
                actuator.EngageEndClip = end;

                actuator.animationName = "Default Gear";
                EditorSceneManager.MarkSceneDirty(actuator.gameObject.scene);
            }
            else
            {
                Debug.Log("Please Select a gameobject to parent the actuator to");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Components/Actuator/Landing Gear/Tutorial", false, 1000)]
        public static void GearTutorial()
        {
            Application.OpenURL("https://youtu.be/cwLsm8w8tGg");
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Components/Camera/Orbit Camera", false, 1100)]
        private static void OrbitCamera()
        {
            GameObject box;
            if (Selection.activeGameObject != null)
            {
                box = new GameObject();
                box.transform.parent = Selection.activeGameObject.transform;
            }
            else
            {
                box = new GameObject();
            }
            box.name = "m_camera_system";
            box.transform.localPosition = Vector3.zero;
            SilantroCamera system = box.AddComponent<SilantroCamera>();
            GameObject focalPoint = new GameObject("m_focus_point");
            focalPoint.transform.parent = box.transform;
            focalPoint.transform.localPosition = Vector3.zero;

            GameObject exterior = new GameObject("Exterior Camera");
            exterior.gameObject.transform.parent = box.transform;
            exterior.transform.localPosition = Vector3.zero;
            Camera exteriorCam = exterior.AddComponent<Camera>();
            exteriorCam.farClipPlane = 10000f;

            GameObject interior = new GameObject("Interior Camera");
            interior.gameObject.transform.parent = box.transform;
            interior.transform.localPosition = Vector3.zero;
            Camera InteriorCam = interior.AddComponent<Camera>();
            InteriorCam.farClipPlane = 10000f;

            system.cameraMode = SilantroCamera.CameraMode.Orbit;
            system.normalExterior = exteriorCam;
            system.normalInterior = InteriorCam;
            system.focusPoint = focalPoint.transform;
            EditorSceneManager.MarkSceneDirty(box.gameObject.scene);
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Components/Camera/Free Camera", false, 1200)]
        private static void FreeCamera()
        {
            GameObject box;
            if (Selection.activeGameObject != null)
            {
                box = new GameObject();
                box.transform.parent = Selection.activeGameObject.transform;
            }
            else
            {
                box = new GameObject();
            }

            box.name = "m_camera_system";
            box.transform.localPosition = Vector3.zero;
            SilantroCamera system = box.AddComponent<SilantroCamera>();
            GameObject focalPoint = new GameObject("m_focus_point");
            focalPoint.transform.parent = box.transform;
            focalPoint.transform.localPosition = Vector3.zero;

            GameObject exterior = new GameObject("Exterior Camera");
            exterior.gameObject.transform.parent = box.transform;
            exterior.transform.localPosition = Vector3.zero;
            Camera exteriorCam = exterior.AddComponent<Camera>();
            exteriorCam.farClipPlane = 10000f;

            GameObject interior = new GameObject("Interior Camera");
            interior.gameObject.transform.parent = box.transform;
            interior.transform.localPosition = Vector3.zero;
            Camera InteriorCam = interior.AddComponent<Camera>();
            InteriorCam.farClipPlane = 10000f;

            system.cameraMode = SilantroCamera.CameraMode.Free;
            system.normalExterior = exteriorCam;
            system.normalInterior = InteriorCam;
            system.focusPoint = focalPoint.transform;
            system.azimuthSensitivity = 3;
            system.elevationSensitivity = 3;
            EditorSceneManager.MarkSceneDirty(box.gameObject.scene);
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Components/Instrument/Lever", false, 1300)]
        private static void AddLever()
        {
            if (Selection.activeGameObject != null)
            {
                GameObject box;
                box = new GameObject("m_lever");
                box.transform.parent = Selection.activeGameObject.transform;
                SilantroInstrument lever = box.AddComponent<SilantroInstrument>();
                lever.m_type = SilantroInstrument.Type.Lever;


                EditorSceneManager.MarkSceneDirty(box.scene);
            }
            else
            {
                Debug.Log("Please Select a gameobject to parent the lever to");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Components/Instrument/Dial", false, 1400)]
        private static void AddDial()
        {
            if (Selection.activeGameObject != null)
            {
                GameObject box;
                box = new GameObject("m_dial");
                box.transform.parent = Selection.activeGameObject.transform;
                SilantroInstrument lever = box.AddComponent<SilantroInstrument>();
                lever.m_type = SilantroInstrument.Type.Dial;

                EditorSceneManager.MarkSceneDirty(box.scene);
            }
            else
            {
                Debug.Log("Please Select a gameobject to parent the dial to");
            }
        }


        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/VR/Activate", false, 1500)]
        private static void ActivateVR()
        {
            if (EditorUtility.DisplayDialog("Configure VR Defines?",
                "Please confirm that you have the XR-Integration toolkit installed and your input handing is set to either 'New' or 'Both'." +
            #region Space
                           "\n" +
                           "" +
                           "\n" +
                           "" +
            #endregion
                "If you activate without having these, you will have to manually comment out the XR input lines in the Lever and Hand scripts", "Activate", "Return"))
            {
                // Check Validity of pairs
                DefinitionSettings.Definitions.ConfigurePairs();
                DefinitionSettings.SilantroTag tag = DefinitionSettings.Definitions.CollectVRTag();
                tag.on = true;
                DefinitionSettings.Definitions.UpdatePairInFile(tag);
                DefinitionSettings.Definitions.SetScriptDefines();
                Debug.Log("VR Defines Added!");
            }
            else
            {
                Debug.Log("Requirements for VR Integration: " + "\n" +
                    "1. Input System, switch the input handling in player settings to 'Input System' or 'Both'" + "\n" +
                    "2. XR Plugin Management" + "\n" +
                    "3. XR Integration Toolkit" + "\n" +
                    "4. Open XR or your preferred integration plugin");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/VR/Deactivate", false, 1600)]
        private static void DeactivateVR()
        {
            // Check Validity of pairs
            DefinitionSettings.Definitions.ConfigurePairs();
            DefinitionSettings.SilantroTag tag = DefinitionSettings.Definitions.CollectVRTag();
            tag.on = false;
            DefinitionSettings.Definitions.UpdatePairInFile(tag);
            DefinitionSettings.Definitions.SetScriptDefines();
            Debug.Log("VR Defines Removed!");
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/VR/Tutorials", false, 1650)]
        private static void VRTutorials()
        {
            Application.OpenURL("https://youtube.com/playlist?list=PLJkxX6TkFwO_wbJZrApnyJ-rBjX1ABIaW");
        }
        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Help/Report Bug", false, 1700)]
        public static void ContactBug()
        {
            Application.OpenURL("mailto:" + "silantrosimulator@gmail.com" + "?subject:" + "Silantro Bug" + "&body:" + " ");
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Help/Tutorials", false, 1800)]
        public static void AssetTutorial()
        {
            Application.OpenURL("https://youtube.com/playlist?list=PLJkxX6TkFwO92f8Qphy3ihB5T6-Fbp0Bf");
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Setup Input", false, 1900)]
        private static void ConfigureInput()
        {
            try
            {
                Handler.ControlAxis(new Handler.Axis() { name = "---------------- Buttons", positiveButton = "-", gravity = 0f, sensitivity = 0f, type = 0, descriptiveName = "Key 01" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Start Engine", positiveButton = "f1", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 02" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Start Engine BandL", positiveButton = "f3", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 03" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Start Engine BandR", positiveButton = "f5", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 04" }, false);

                Handler.ControlAxis(new Handler.Axis() { name = "Stop Engine", positiveButton = "f2", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 05" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Stop Engine BandL", positiveButton = "f4", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 06" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Stop Engine BandR", positiveButton = "f6", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 07" }, false);

                Handler.ControlAxis(new Handler.Axis() { name = "Parking Brake", positiveButton = "x", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 08" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Brake Lever", positiveButton = "space", gravity = 0.0f, sensitivity = 0.6f, type = 0, descriptiveName = "Key 09" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Actuate Gear", positiveButton = "0", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 10" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "LightSwitch", positiveButton = "v", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 11" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Fire", positiveButton = "left ctrl", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 12" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Target Up", positiveButton = "m", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 13" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Target Down", positiveButton = "n", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 14" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Target Lock", positiveButton = "numlock", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 15" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Propeller Engage", positiveButton = "y", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 16" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Weapon Select", positiveButton = "q", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 17" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Weapon Release", positiveButton = "z", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 18" }, false);

                Handler.ControlAxis(new Handler.Axis() { name = "DropSwitch", positiveButton = "backspace", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 19" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Speed Brake", positiveButton = "o", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 20" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Afterburner", positiveButton = "f12", altPositiveButton = "joystick button 3", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 21" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Reverse Thrust", positiveButton = "f11", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 22" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Fuel Dump", positiveButton = "6", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 23" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Refuel", positiveButton = "5", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 24" }, false);

                Handler.ControlAxis(new Handler.Axis() { name = "Extend Flap", positiveButton = "3", altPositiveButton = "joystick button 4", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 28" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Retract Flap", positiveButton = "4", altPositiveButton = "joystick button 5", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 29" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Actuate Slat", positiveButton = "k", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 30" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Spoiler", positiveButton = "g", gravity = 0.5f, sensitivity = 1f, type = 0, descriptiveName = "Key 31" }, false);

                Handler.ControlAxis(new Handler.Axis() { name = "---------------- Keyboard Axes", positiveButton = "-", gravity = 0f, sensitivity = 0f, type = 0, descriptiveName = "Key 32" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Throttle", positiveButton = "1", negativeButton = "2", gravity = 0.0f, sensitivity = 0.4f, type = 0, dead = 0.001f, descriptiveName = "Key 33" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Roll", positiveButton = "d", negativeButton = "a", altPositiveButton = "right", altNegativeButton = "left", gravity = 0.6f, sensitivity = 0.65f, type = 0, dead = 0.001f, descriptiveName = "Key 34" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Pitch", positiveButton = "w", negativeButton = "s", altPositiveButton = "up", altNegativeButton = "down", gravity = 0.6f, sensitivity = 0.7f, invert = true, type = 0, dead = 0.001f, descriptiveName = "Key 35" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Rudder", positiveButton = "e", negativeButton = "q", gravity = 1f, sensitivity = 0.9f, type = 0, dead = 0.001f, descriptiveName = "Key 36" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Collective", positiveButton = "right alt", negativeButton = "left alt", gravity = 0.0f, sensitivity = 0.25f, type = 0, dead = 0.001f, descriptiveName = "Key 37" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Propeller", positiveButton = "]", negativeButton = "[", gravity = 0.0f, sensitivity = 0.35f, type = 0, dead = 0.001f, descriptiveName = "Key 38" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Mixture", positiveButton = "6", negativeButton = "7", gravity = 0.0f, sensitivity = 0.25f, type = 0, dead = 0.001f, descriptiveName = "Key 39" }, false);

                Handler.ControlAxis(new Handler.Axis() { name = "---------------- Joystick Axes", positiveButton = "-", gravity = 0f, sensitivity = 0f, type = 0, descriptiveName = "Key 40" }, false);
                Handler.ControlAxis(new Handler.Axis() { name = "Throttle", positiveButton = " ", negativeButton = " ", gravity = 1.0f, sensitivity = 1.0f, type = 2, axis = 3, dead = 0.001f, invert = true, descriptiveName = "Key 41" }, true);
                Handler.ControlAxis(new Handler.Axis() { name = "Roll", positiveButton = " ", negativeButton = " ", gravity = 1.0f, sensitivity = 1.0f, type = 2, axis = 0, dead = 0.001f, descriptiveName = "Key 42" }, true);
                Handler.ControlAxis(new Handler.Axis() { name = "Pitch", positiveButton = " ", negativeButton = " ", gravity = 1.0f, sensitivity = 1.0f, type = 2, axis = 1, dead = 0.001f, descriptiveName = "Key 43" }, true);
                Handler.ControlAxis(new Handler.Axis() { name = "Rudder", positiveButton = " ", negativeButton = " ", gravity = 1.0f, sensitivity = 1.0f, type = 2, axis = 2, dead = 0.001f, descriptiveName = "Key 44" }, true);
                Handler.ControlAxis(new Handler.Axis() { name = "Collective", positiveButton = " ", negativeButton = " ", gravity = 1.0f, sensitivity = 1.0f, type = 2, axis = 4, dead = 0.001f, descriptiveName = "Key 45" }, true);
                Handler.ControlAxis(new Handler.Axis() { name = "Propeller", positiveButton = " ", negativeButton = " ", gravity = 1.0f, sensitivity = 1.0f, type = 2, axis = 5, dead = 0.001f, descriptiveName = "Key 46" }, true);
                Handler.ControlAxis(new Handler.Axis() { name = "Mixture", positiveButton = " ", negativeButton = " ", gravity = 1.0f, sensitivity = 1.0f, type = 2, axis = 3, dead = 0.001f, invert = true, descriptiveName = "Key 47" }, true);

                Debug.Log("Input Setup Successful! Please check that the axes and joystick properties are configured properly");
            }
            catch
            {
                Debug.LogError("Failed to apply input manager bindings.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Notifications/Enable", false, 2000)]
        public static void EnableNotifications()
        {
            string _location = "Assets/Silantro/Common/Storage/Silantro_UPDATE.txt";
            File.WriteAllText(_location, "3.515;3.515;YES");
            Debug.Log("Update Notifications Enabled!");
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Notifications/Disable", false, 2100)]
        public static void DisableNotifications()
        {
            string _location = "Assets/Silantro/Common/Storage/Silantro_UPDATE.txt";
            File.WriteAllText(_location, "3.515;3.515;NO");
            Debug.Log("Update Notifications Disabled!");
        }

        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Common/Help/Reset Scripting Symbols", false, 2200)]
        public static void ResetDefines()
        {
            if (EditorUtility.DisplayDialog("Configure scripting symbols and Restart Editor?", "This action will close and reopen your project to reconfigure the script defines", "Restart", "Return"))
            {
                string _location = "Assets/Silantro/Common/Storage/Silantro_DEFINES.txt";
                File.WriteAllText(_location, "VR_ACTIVE,0;SILANTRO_ROTARY,0;SILANTRO_FIXED,0;");
                EditorApplication.OpenProject(Directory.GetCurrentDirectory()); 
            }
            
        }
    }
}

/// <summary>
/// Please don't mess with/edit this class in any way
/// </summary>
namespace Oyedoyin.Communication
{
    /// <summary>
    /// 
    /// </summary>
    public class StaticCoroutine : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_coroutine"></param>
        public void Work(IEnumerator _coroutine)
        {
            StartCoroutine(WorkCoroutine(_coroutine));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_coroutine"></param>
        /// <returns></returns>
        private IEnumerator WorkCoroutine(IEnumerator _coroutine)
        {
            yield return StartCoroutine(_coroutine);
            DestroyImmediate(this.gameObject);
        }
    }

    [InitializeOnLoad]
    public class BetaNotification : ScriptableObject
    {
        static BetaNotification m_Instance = null;
        private static readonly string _location = "Assets/Silantro/Common/Storage/Silantro_STATE.txt";
        private string l_command; 

        /// <summary>
        /// 
        /// </summary>
        static BetaNotification()
        {
            EditorApplication.update += OnInit;
        }

        /// <summary>
        /// 
        /// </summary>
        static void OnInit()
        {
            EditorApplication.update -= OnInit;
            m_Instance = FindObjectOfType<BetaNotification>();
            if (m_Instance == null)
            {
                m_Instance = CreateInstance<BetaNotification>();
                if (!SessionState.GetBool("FirstInitCommonDone", false))
                {
                    m_Instance.ShowNotification();
                    SessionState.SetBool("FirstInitCommonDone", true);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void ShowNotification()
        {
            if (!File.Exists(_location))
            {
                File.WriteAllText(_location, "NO;");
            }
            else
            {
                // Local Data
                StreamReader m_localFile = new StreamReader(_location);
                string[] m_localData = m_localFile.ReadToEnd().Split(char.Parse(";"));
                l_command = string.Concat(m_localData[0].Where(c => !char.IsWhiteSpace(c)));

                if (l_command == "NO")
                {
                    EditorUtility.DisplayDialog("Silantro Notification",
                           "Welcome to asset version 3.515." +
                    #region Space
                           "\n" +
                           "" +
                           "\n" +
                           "" +
                    #endregion
                           "The communication script will automatically check my Github page when you load the project to see if any patch or update is available. " +
                    #region Space
                           "\n" +
                           "" +
                           "\n" +
                           "" +
                    #endregion
                           "It'll show a notification containing the type of the update, the version and a note containing the things that have been added or changed from the previous version. " +
                           "You can then download the update from the asset store or the package manager." +
                    #region Space
                           "\n" +
                           "" +
                           "\n" +
                           "" +
                    #endregion
                           "If for any reason the notifications get annoying, you can turn it off from the asset menu and manually check for updates." +
                    #region Space
                           "\n" +
                           "" +
                           "\n" +
                           "" +
                    #endregion
                           "If you run into any bug or issue, weirdness in the profiler or require help/clarification about any component/feature...my email is in the help section of the menu." +
                    #region Space
                           "\n" +
                           "" +
                           "\n" +
                           "" +
                    #endregion
                           "Happy Flying :)", 
                          "Close");
                    m_localFile.Dispose();
                    File.WriteAllText(_location, "YES;");
                }
            }
        }
    }
}

/// <summary>
/// Please don't mess with/edit this class in any way
/// </summary>
namespace Oyedoyin.Common.DefinitionSettings
{
    /// <summary>
    /// 
    /// </summary>
    public class SilantroTag
    {
        public string defineSymbol;
        public bool on;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static SilantroTag ParseLine(string line)
        {
            string[] split = line.Split(',');
            if (split.Length != 2)
            {
                return null;
            }
            var pair = new SilantroTag
            {
                defineSymbol = split[0],
                on = split[1] == "1",
            };
            return pair;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToFileString()
        {
            return defineSymbol + "," + (on ? "1" : "0");
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class Definitions
    {
        private static List<SilantroTag> _defineSymbols = new List<SilantroTag>();
        private static readonly string _location = "Assets/Silantro/Common/Storage/Silantro_DEFINES.txt";
        private static bool _hasReadSymbols;



        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            CheckToReadPairsFromFile();
        }
        /// <summary>
        /// 
        /// </summary>
        private static void CheckToReadPairsFromFile()
        {
            if (!_hasReadSymbols)
            {
                // Collect
                _defineSymbols = ParseFileToPairs(_location);
                // Sort
                SortDefines();
                // Tag
                _hasReadSymbols = true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static List<SilantroTag> ParseFileToPairs(string filepath)
        {
            // Read
            string text = File.ReadAllText(filepath);
            string[] lines = text.Split(';');

            var pairs = new List<SilantroTag>();
            foreach (string line in lines)
            {
                var pair = SilantroTag.ParseLine(line);
                if (pair != null)
                {
                    pairs.Add(pair);
                }
            }
            return pairs;
        }
        /// <summary>
        /// 
        /// </summary>
        public static void ConfigurePairs()
        {
            if (!File.Exists(_location)) { File.Create(_location); }
            // Collect
            _defineSymbols = ParseFileToPairs(_location);
            // Sort
            SortDefines();

            if (_defineSymbols.Count < 3)
            {
                var sfixed = new SilantroTag { defineSymbol = "SILANTRO_FIXED", on = false, };
                var srotor = new SilantroTag { defineSymbol = "SILANTRO_ROTARY", on = false, };
                var sactVR = new SilantroTag { defineSymbol = "VR_ACTIVE", on = false, };
                if (!_defineSymbols.Contains(sfixed)) { _defineSymbols.Add(sfixed); AddPairToFile(sfixed); }
                if (!_defineSymbols.Contains(srotor)) { _defineSymbols.Add(srotor); AddPairToFile(srotor); }
                if (!_defineSymbols.Contains(sactVR)) { _defineSymbols.Add(sactVR); AddPairToFile(sactVR); }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pair"></param>
        public static void AddPairToFile(SilantroTag pair)
        {
            string pairStr = pair.ToFileString();
            string appendStr = pairStr + ";";
            File.AppendAllText(_location, appendStr);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pair"></param>
        public static void DeletePairFromFile(SilantroTag pair)
        {
            string needle = pair.defineSymbol;
            string text = File.ReadAllText(_location);
            int index = text.IndexOf(needle);
            int delimiterIndex = text.IndexOf(";", index + 1);
            if (delimiterIndex < 0)
            {
                text = text.Remove(index);
            }
            else
            {
                text = text.Remove(index, delimiterIndex - index + 1);
            }
            File.WriteAllText(_location, text);
        }
        /// <summary>
        /// 
        /// </summary>
        public static void SortDefines()
        {
            _defineSymbols = _defineSymbols.OrderBy(o => o.defineSymbol).ToList();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pair"></param>
        public static void UpdatePairInFile(SilantroTag pair)
        {
            DeletePairFromFile(pair);
            AddPairToFile(pair);
        }
        /// <summary>
        /// 
        /// </summary>
        public static void SetScriptDefines()
        {
            var targetBuildGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = CreateDefinesString();
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetBuildGroup, defines);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string CreateDefinesString()
        {
            string str = "";
            foreach (SilantroTag pair in _defineSymbols)
            {
                if (pair.on)
                    str += pair.defineSymbol + ";";
            }
            return str;
        }


        #region Calls

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SilantroTag CollectVRTag()
        {
            // Collect
            _defineSymbols = ParseFileToPairs(_location);
            // Sort
            SortDefines();

            // Check for VR Tag
            bool _vrTagSet = false;
            SilantroTag vrDefine = new SilantroTag();
            foreach (SilantroTag define in _defineSymbols) { if (define.defineSymbol == "VR_ACTIVE") { vrDefine = define; _vrTagSet = true; } }
            if (!_vrTagSet)
            {
                var sactVR = new SilantroTag { defineSymbol = "VR_ACTIVE", on = false, };
                if (!_defineSymbols.Contains(sactVR)) { _defineSymbols.Add(sactVR); AddPairToFile(sactVR); }
                vrDefine = sactVR;
            }

            return vrDefine;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SilantroTag CollectFixedTag()
        {
            // Collect
            _defineSymbols = ParseFileToPairs(_location);
            // Sort
            SortDefines();

            // Check for VR Tag
            bool _fixedTagSet = false;
            SilantroTag fixedDefine = new SilantroTag();
            foreach (SilantroTag define in _defineSymbols) { if (define.defineSymbol == "SILANTRO_FIXED") { fixedDefine = define; _fixedTagSet = true; } }
            if (!_fixedTagSet)
            {
                var sfixed = new SilantroTag { defineSymbol = "SILANTRO_FIXED", on = false, };
                if (!_defineSymbols.Contains(sfixed)) { _defineSymbols.Add(sfixed); AddPairToFile(sfixed); }
                fixedDefine = sfixed;
            }

            return fixedDefine;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SilantroTag CollectRotaryTag()
        {
            // Collect
            _defineSymbols = ParseFileToPairs(_location);
            // Sort
            SortDefines();

            // Check for VR Tag
            bool _rotaryTagSet = false;
            SilantroTag rotaryDefine = new SilantroTag();
            foreach (SilantroTag define in _defineSymbols) { if (define.defineSymbol == "SILANTRO_ROTARY") { rotaryDefine = define; _rotaryTagSet = true; } }
            if (!_rotaryTagSet)
            {
                var srotary = new SilantroTag { defineSymbol = "SILANTRO_ROTARY", on = false, };
                if (!_defineSymbols.Contains(srotary)) { _defineSymbols.Add(srotary); AddPairToFile(srotary); }
                rotaryDefine = srotary;
            }

            return rotaryDefine;
        }

        #endregion
    }
}

