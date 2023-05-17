using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Oyedoyin.Common
{
    /// <summary>
    /// 
    /// </summary>
    #region Component
    public class SilantroPylon : MonoBehaviour
    {

        // ------------------------------------------ Selectibles
        public enum PylonPosition { External, Internal }
        public PylonPosition pylonPosition;
        public enum LauncherType { Trapeze, Drop, Tube }
        public LauncherType launcherType = LauncherType.Drop;
        public enum TrapezePosition { Left, Right, Central }
        public TrapezePosition trapezePosition = TrapezePosition.Right;
        public enum OrdnanceType { Bomb, Missile }
        public OrdnanceType munitionType = OrdnanceType.Bomb;
        public enum DropMode { Single, Salvo }
        public DropMode bombMode = DropMode.Single;



        // ------------------------------------------ Variables
        public Controller m_controller;
        public Transform target;
        public SilantroActuator pylonBay;
        public SilantroMunition missile;
        public List<SilantroMunition> bombs;
        public float dropInterval = 1f;
        public float waitTime;



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        //COUNT ATTACHED MUNITIONS
        public void Initialize()
        {
            //CLOSE DOOR
            if (pylonBay != null && pylonPosition == PylonPosition.Internal)
            {
                if (pylonBay.actuatorMode == SilantroActuator.ActuatorMode.DefaultOpen && pylonBay.actuatorState == SilantroActuator.ActuatorState.Engaged)
                {
                    pylonBay.DisengageActuator();
                }
            }

            //IDENTIFY ATTACHED MUNITION
            SilantroMunition[] munitions = GetComponentsInChildren<SilantroMunition>();
            bombs = new List<SilantroMunition>();
            foreach (SilantroMunition munition in munitions)
            {
                //MISSILE
                if (munitionType == OrdnanceType.Missile)
                {
                    if (munition.munitionType == SilantroMunition.MunitionType.Missile)
                    {
                        missile = munition;
                        missile.m_pylon = this.gameObject.GetComponent<SilantroPylon>();
                    }
                }
                //BOMB
                if (munitionType == OrdnanceType.Bomb)
                {
                    if (munition.munitionType == SilantroMunition.MunitionType.Bomb)
                    {
                        bombs.Add(munition);
                        munition.m_pylon = this.gameObject.GetComponent<SilantroPylon>();
                    }
                }
            }
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        //RECOUNT BOMBS
        void CountBombs()
        {
            SilantroMunition[] munitions = GetComponentsInChildren<SilantroMunition>();
            bombs = new List<SilantroMunition>();
            foreach (SilantroMunition munition in munitions)
            {
                //BOMB
                if (munitionType == OrdnanceType.Bomb)
                {
                    if (munition.munitionType == SilantroMunition.MunitionType.Bomb)
                    {
                        bombs.Add(munition);
                    }
                }
            }
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        //START SEQUENCE LAUNCH
        public void StartLaunchSequence()
        {
            engaged = true;
            //DETERMINE LAUNCH SEQUENCE
            if (pylonPosition == PylonPosition.External)
            {
                LaunchMissile();
            }
            if (pylonPosition == PylonPosition.Internal)
            {
                //OPEN DOOR
                if (pylonBay != null)
                {
                    StartCoroutine(OpenBayDoor());
                }
                //LAUNCH IF DOOR IS UNAVAILABLE
                else
                {
                    LaunchMissile();
                }
            }
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        //START SEQUENCE DROP
        public void StartDropSequence()
        {
            if (pylonPosition == PylonPosition.External)
            {
                BombRelease();
            }
            if (pylonPosition == PylonPosition.Internal)
            {
                //OPEN DOOR
                engaged = true;
                if (pylonBay != null)
                {
                    bombMode = DropMode.Salvo;
                    StartCoroutine(OpenBayDoor());
                }
                //LAUNCH IF DOOR IS UNAVAILABLE
                else
                {
                    BombRelease();
                }
            }
        }

        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool engaged;
        //OPEN DOOR
        IEnumerator OpenBayDoor()
        {
            if (pylonBay.actuatorState == SilantroActuator.ActuatorState.Disengaged) { pylonBay.EngageActuator(); }

            yield return new WaitUntil(() => pylonBay.actuatorState == SilantroActuator.ActuatorState.Engaged);
            //RELEASE MUNITION
            if (munitionType == OrdnanceType.Missile) { LaunchMissile(); }
            if (munitionType == OrdnanceType.Bomb) { BombRelease(); }
        }


        //CLOSE DOOR
        IEnumerator CloseDoor()
        {
            yield return new WaitForSeconds(0.5f);
            if (pylonBay.actuatorState == SilantroActuator.ActuatorState.Engaged) { pylonBay.DisengageActuator(); }
            //REMOVE PYLON
            Destroy(this.gameObject);
        }


        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        //ACTUAL MISSILE LAUNCH
        void LaunchMissile()
        {
            //1. TUBE LAUNCH
            if (launcherType == LauncherType.Tube)
            {
                missile.FireMunition(target, m_controller.m_rigidbody.velocity, 2);
            }
            //2. DROP LAUNCH
            if (launcherType == LauncherType.Drop)
            {
                missile.FireMunition(target, m_controller.m_rigidbody.velocity, 1);
            }

            //3. TRAPEZE LAUNCH RIGHT
            if (launcherType == LauncherType.Trapeze && trapezePosition == TrapezePosition.Right)
            {
                missile.FireMunition(target, m_controller.m_rigidbody.velocity, 3);
            }

            //4. TRAPEZE LAUNCH LEFT
            if (launcherType == LauncherType.Trapeze && trapezePosition == TrapezePosition.Left)
            {
                missile.FireMunition(target, m_controller.m_rigidbody.velocity, 4);
            }

            //5. TRAPEZE LAUNCH MIDDLE
            if (launcherType == LauncherType.Trapeze && trapezePosition == TrapezePosition.Central)
            {
                missile.FireMunition(target, m_controller.m_rigidbody.velocity, 5);
            }

            //CLOSE BAY DOOR
            if (pylonPosition == PylonPosition.Internal && pylonBay != null)
            {
                StartCoroutine(CloseDoor());
            }

            m_controller.CountOrdnance();
        }


        /// <summary>
        /// ACTUAL BOMB DROP
        /// </summary>
        void BombRelease()
        {
            //1. SINGLE BOMB DROP
            //SELECT RANDOM BOMB
            if (bombs.Count > 0)
            {
                if (bombs[0] != null)
                {
                    bombs[0].ReleaseMunition(m_controller.m_rigidbody.velocity);
                    m_controller.CountOrdnance();
                    CountBombs();
                }



                //2. SALVO DROP
                if (bombMode == DropMode.Salvo)
                {
                    StartCoroutine(WaitForNextDrop());
                }
            }
            else
            {
                if (pylonPosition == PylonPosition.Internal && pylonBay != null && pylonBay.actuatorState == SilantroActuator.ActuatorState.Engaged)
                {
                    StartCoroutine(CloseDoor());
                }
            }
        }

        /// <summary>
        /// SALVO TIMER
        /// </summary>
        /// <returns></returns>
        IEnumerator WaitForNextDrop()
        {
            yield return new WaitForSeconds(dropInterval);
            BombRelease();
            m_controller.CountOrdnance();
            CountBombs();
        }
    }
    #endregion


    #region Editor
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroPylon))]
    public class PylonEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1, 0.4f, 0);
        SilantroPylon pylon;


        private void OnEnable()
        {
            pylon = (SilantroPylon)target;
        }

        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector ();
            serializedObject.Update();

            GUILayout.Space(1f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Pylon Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pylonPosition"), new GUIContent("Position"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionType"), new GUIContent("Ordnance"));

            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Launch Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            if (pylon.pylonPosition == SilantroPylon.PylonPosition.Internal)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pylonBay"), new GUIContent("Bay Actuator"));
            }
            if (pylon.munitionType == SilantroPylon.OrdnanceType.Bomb)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bombMode"), new GUIContent("Drop Mode"));

                if (pylon.bombMode == SilantroPylon.DropMode.Salvo)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dropInterval"), new GUIContent("Drop Interval"));
                }
            }
            if (pylon.munitionType == SilantroPylon.OrdnanceType.Missile)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("launcherType"), new GUIContent("Launch Mode"));

                if (pylon.launcherType == SilantroPylon.LauncherType.Trapeze)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("trapezePosition"), new GUIContent("Launch Position"));
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    #endregion
}
