using System;
using UnityEngine;
using Oyedoyin.Common.Misc;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Oyedoyin.Common
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TrackObject
    {
        public enum Type { Aircraft, Munition, Misc }
        public Type m_type = Type.Misc;
        public MonoBehaviour m_component;
        public GameObject m_body;
        public float m_distance;
        public float m_heading;
        public float m_altitude;
        public float m_speed;
        public Texture2D m_icon;
    }

    /// <summary>
    /// 
    /// </summary>
    public class SilantroRadar : MonoBehaviour
    {
        public ControlState m_advancedTracking = ControlState.Off;

        // ---------------------------------------- Properties
        public float m_range = 1000f;
        public int m_maximumObjects = 100;
        public float pingRate = 10;
        private float actualPingRate;
        public float pingTime;
        public LayerMask m_collisionLayers;

        public float size = 250f;
        [Range(0, 1f)] public float Transparency = 0.9f;
        private float m_scale;
        public float objectScale = 2.5f;
        public bool m_markTargets;


        public Controller m_controller;
        public GUISkin m_radarSkin;
        private GUIStyle labelStyle = new GUIStyle();
        private Collider[] hitColliders = new Collider[100];
        public List<MonoBehaviour> m_sceneObjects;
        public List<TrackObject> m_targets;
        private GameObject _object;

        // ---------------------------------------- Textures
        public Texture background;
        public Texture compass;
        public Texture2D selectedTargetTexture, lockedTargetTexture;
        public Texture2D TargetLockOnTexture;
        public Texture2D TargetLockedTexture;

        public Texture2D m_aircraftTexture;
        public Texture2D m_missileTexture;
        Texture2D radarTexture;

        public Color generalColor = Color.white;
        public Color labelColor = Color.white;

        // ---------------------------------------- Filtered Objects
        public TrackObject m_currentTarget;
        public TrackObject m_lockedTarget;
        private readonly Vector2 radarPosition = Vector2.zero;
        public int m_selection;
        public bool m_targetLocked;

        // ---------------------------------------- Target Display
        public Camera currentCamera;
        public Camera lockedTargetCamera;
        public Camera targetCamera;
        public float cameraDistance = 40f;
        public float cameraHeight = 30f;
        Vector3 lockedCameraPosition;

        #region Call Functions

        public void SelectedUpperTarget() { m_selection++; FilterPointer(); }//SELECT TARGET ABOVE CURRENT TARGET
        public void SelectLowerTarget() { m_selection--; FilterPointer(); }//SELECT TAREGT BELOW CURRENT TARGET
        public void SelectTargetAtPosition(int position) { m_selection = position; FilterPointer(); }//SELECT TARGET AT A PARTICULAR POSITION
        private void FilterPointer()
        {
            if (m_selection < 0) { m_selection = m_targets.Count - 1; }
            if (m_selection > m_targets.Count - 1) { m_selection = 0; }
        }

        /// <summary>
        /// LOCK ONTO A TARGET
        /// </summary>
        public void LockSelectedTarget()
        {
            //SET TARGET PROPERTIES
            if (m_advancedTracking == ControlState.Active && m_controller.allOk)
            {
                m_lockedTarget = m_currentTarget;
                m_targetLocked = true;
            }
        }
        /// <summary>
        /// RELEASE TARGET LOCK
        /// </summary>
        public void ReleaseLockedTarget()
        {
            if (m_advancedTracking == ControlState.Active && m_controller.allOk)
            {
                m_targetLocked = false;
                m_lockedTarget = new TrackObject();
            }
        }


        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            if (pingRate < 1f) { pingRate = 1f; }
            actualPingRate = 1f / pingRate;
            pingTime = 0f;
            m_sceneObjects = new List<MonoBehaviour>(m_maximumObjects);
            m_targets = new List<TrackObject>(m_maximumObjects);
            hitColliders = new Collider[m_maximumObjects];
        }

        /// <summary>
        /// 
        /// </summary>
        private void Ping()
        {
            m_sceneObjects.Clear();
            m_targets.Clear();
            Physics.OverlapSphereNonAlloc(transform.position, m_range, hitColliders, m_collisionLayers);

            // Make sure object is;
            //1. not NULL
            //2. not a child of this object
            //3. not children of the same parent transform
            //4. is within tracking range
            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i] != null)
                {
                    _object = hitColliders[i].gameObject;
                    float _distance = Vector3.Distance(transform.position, _object.transform.position);
                    // Filter
                    if (_object != gameObject && _distance <= m_range && _object.transform.parent != transform && _object.transform.root != transform.root)
                    {
                        SilantroMunition _munition = _object.GetComponent<SilantroMunition>();
                        SilantroMisc _ponder = _object.GetComponent<SilantroMisc>();
                        Controller _aircraft = _object.GetComponent<Controller>();

                        // Filter Munition
                        if (_munition != null && _munition.munitionType == SilantroMunition.MunitionType.Missile)
                        {
                            if (_munition.gameObject != gameObject && !m_sceneObjects.Contains(_munition))
                            {
                                m_sceneObjects.Add(_munition);
                                TrackObject m_object = new TrackObject
                                {
                                    m_type = TrackObject.Type.Munition,
                                    m_component = _munition,
                                    m_body = _munition.gameObject
                                };
                                m_targets.Add(m_object);
                            }
                        }

                        // Filter Transponder
                        if (_ponder != null && _ponder.m_function == SilantroMisc.Function.Transponder)
                        {
                            if (_ponder.gameObject != gameObject && !m_sceneObjects.Contains(_ponder))
                            {
                                m_sceneObjects.Add(_ponder);
                                TrackObject m_object = new TrackObject
                                {
                                    m_type = TrackObject.Type.Misc,
                                    m_component = _ponder,
                                    m_body = _ponder.gameObject,
                                    m_icon = _ponder.silantroTexture
                                };
                                m_targets.Add(m_object);
                            }
                        }

                        // Filter Aircraft
                        if (_aircraft != null)
                        {
                            if (_aircraft.gameObject != gameObject && !m_sceneObjects.Contains(_aircraft))
                            {
                                m_sceneObjects.Add(_aircraft);
                                TrackObject m_object = new TrackObject
                                {
                                    m_type = TrackObject.Type.Aircraft,
                                    m_component = _aircraft,
                                    m_body = _aircraft.gameObject
                                };
                                m_targets.Add(m_object);
                            }
                        }
                    }
                }
            }

            // Filter of the tracked objects
            for (int i = 0; i < m_targets.Count; i++)
            {
                // Check that Object component exists
                TrackObject _object = m_targets[i];
                if (_object != null)
                {
                    if (_object.m_component == null) { m_targets.Remove(_object); m_sceneObjects.Remove(_object.m_component); }
                    if (_object.m_body == null) { m_targets.Remove(_object); }
                    // Track object properties
                    _object.m_distance = Vector3.Distance(transform.position, _object.m_body.transform.position);
                    _object.m_heading = _object.m_body.transform.eulerAngles.y;
                    _object.m_altitude = _object.m_body.transform.position.y;
                    // Remove if out of range
                    if (_object.m_distance > m_range) { m_targets.Remove(_object); }
                    if (m_lockedTarget.m_body != null && m_lockedTarget.m_distance > m_range) { ReleaseLockedTarget(); }
                    if (m_currentTarget.m_body != null && m_currentTarget.m_distance > m_range) { m_currentTarget = new TrackObject(); }
                }
            }
            // Reset
            pingTime = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Compute()
        {
            if (m_controller != null && m_controller.m_view != null) { currentCamera = m_controller.m_view.currentCamera; }
            pingTime += Time.fixedDeltaTime;
            if (pingTime >= actualPingRate) { Ping(); }
            m_scale = m_range / 100f;

            // Target Cam
            PositionCamera();
            // Track and Lock
            if (m_advancedTracking == ControlState.Active) { CombatRadarSystem(); }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CombatRadarSystem()
        {
            if (m_lockedTarget == null && m_currentTarget == null) { m_selection++; }
            FilterPointer();

            // Select Current Object
            if (m_targets != null && m_targets.Count > 0)
            {
                m_currentTarget = m_targets[m_selection];
                if (m_currentTarget == null)
                {
                    m_targets.Remove(m_currentTarget);
                }
            }

            // Release lock if target is destroyed
            if (m_targetLocked)
            {
                if (m_lockedTarget == null || m_lockedTarget.m_body == null || m_lockedTarget.m_component == null) { ReleaseLockedTarget(); }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnGUI()
        {
            Vector2 radarPosition = Vector2.zero;
            if (m_controller == null || !m_controller.isControllable) { return; }

            // Set GUI Skin
            if (m_radarSkin != null) { GUI.skin = m_radarSkin; }
            GUI.color = new Color(generalColor.r, generalColor.g, generalColor.b, Transparency);

            // Draw Locked Target on Screen
            if (m_lockedTarget != null && m_lockedTarget.m_body != null)
            {
                // Draw Lock Indicator
                if (currentCamera)
                {
                    Vector3 dir = (m_lockedTarget.m_body.transform.position - currentCamera.transform.position).normalized;
                    float direction = Vector3.Dot(dir, currentCamera.transform.forward);
                    if (direction > 0.5f)
                    {
                        Vector3 screenPos = currentCamera.WorldToScreenPoint(m_lockedTarget.m_body.transform.position);
                        if (TargetLockedTexture) GUI.DrawTexture(new Rect(screenPos.x - TargetLockedTexture.width / 2, Screen.height - screenPos.y - TargetLockedTexture.height / 2, TargetLockedTexture.width, TargetLockedTexture.height), TargetLockedTexture);
                        // DISPLAY TARGET PROPERTIES
                        GUI.Label(new Rect(screenPos.x + 40, Screen.height - screenPos.y - 20, 250, 50), m_lockedTarget.m_body.name);
                        //GUI.Label(new Rect(screenPos.x + 40, Screen.height - screenPos.y + 20, 200, 50), "SPD:" + m_lockedTarget.m_speed.ToString("0.0") + " kts");
                        if (m_lockedTarget.m_body)
                        {
                            m_lockedTarget.m_distance = Vector3.Distance(transform.position, m_lockedTarget.m_body.transform.position);
                            m_lockedTarget.m_altitude = m_lockedTarget.m_body.transform.position.y;
                        }
                        GUI.Label(new Rect(screenPos.x + 40, Screen.height - screenPos.y + 40, 200, 50), "ALT: " + m_lockedTarget.m_altitude.ToString("0.0") + " m");
                        GUI.Label(new Rect(screenPos.x + 40, Screen.height - screenPos.y + 60, 200, 50), "DST: " + m_lockedTarget.m_distance.ToString("0.0") + " m");
                    }
                }
            }



            // Draw Filtered Object on Radar
            foreach (TrackObject selectedObject in m_targets)
            {
                if (selectedObject != null && selectedObject.m_body != null)
                {
                    //COLLECT TARGET DATA
                    Vector2 position = GetPosition(selectedObject.m_body.transform.position);
                    if (selectedObject.m_icon != null) { radarTexture = selectedObject.m_icon; }
                    else
                    {
                        if (selectedObject.m_type == TrackObject.Type.Aircraft && m_aircraftTexture != null) { radarTexture = m_aircraftTexture; }
                        if (selectedObject.m_type == TrackObject.Type.Munition && m_missileTexture != null) { radarTexture = m_missileTexture; }
                    }
                    string targetID = selectedObject.m_body.name;
                    float superScale = objectScale / 1;

                    //DRAW ON SCREEN
                    if (radarTexture != null)
                    {
                        GUI.DrawTexture(new Rect(position.x - (float)radarTexture.width / superScale / 2f, position.y + (float)radarTexture.height / superScale / 3f, (float)radarTexture.width / superScale,
                            (float)radarTexture.height / superScale), radarTexture);
                    }

                    //CHOOSE LABEL COLOR
                    labelStyle.normal.textColor = new Color(labelColor.r, labelColor.g, labelColor.b, Transparency);

                    //DRAW LABEL
                    GUI.Label(new Rect(position.x - (float)radarTexture.width / objectScale / 2f, position.y - (float)radarTexture.height / superScale / 2f, 50f / superScale, 40f / superScale), targetID, labelStyle);

                    //DRAW CAMERA INDICATOR
                    if (currentCamera)
                    {
                        Vector3 dir = (selectedObject.m_body.transform.position - currentCamera.transform.position).normalized;
                        float direction = Vector3.Dot(dir, currentCamera.transform.forward);
                        if (direction > 0.5f)
                        {
                            Vector3 screenPos = currentCamera.WorldToScreenPoint(selectedObject.m_body.transform.position);
                            if (TargetLockOnTexture)
                                GUI.DrawTexture(new Rect(screenPos.x - TargetLockOnTexture.width / 2, Screen.height - screenPos.y - TargetLockOnTexture.height / 2, TargetLockOnTexture.width, TargetLockOnTexture.height), TargetLockOnTexture);
                        }
                    }
                }
            }






            // Draw Locked Target on Radar
            if (m_targets.Count > 0 && m_lockedTarget != null && m_lockedTarget.m_body != null)
            {
                //COLLECT DATA
                Vector2 currentposition = GetPosition(m_lockedTarget.m_body.transform.position);
                GUI.DrawTexture(new Rect(currentposition.x - (float)lockedTargetTexture.width / 2.5f / 2f, currentposition.y + (float)lockedTargetTexture.height / 2.5f / 3f,
                    (float)lockedTargetTexture.width / 2.5f, (float)lockedTargetTexture.height / 2.5f), lockedTargetTexture);
            }

            // Draw Selected Target
            if (m_targets.Count > 0 && m_currentTarget != null && m_currentTarget.m_body != null && m_currentTarget.m_component != null)
            {
                //COLLECT DATA
                Vector2 currentposition = GetPosition(m_currentTarget.m_body.transform.position);
                GUI.DrawTexture(new Rect(currentposition.x - (float)selectedTargetTexture.width / 2.5f / 2f, currentposition.y + (float)selectedTargetTexture.height / 2.5f / 3f,
                (float)selectedTargetTexture.width / 2.5f, (float)selectedTargetTexture.height / 2.5f), selectedTargetTexture);
            }

            //Draw Background
            if (background) { GUI.DrawTexture(new Rect(radarPosition.x, radarPosition.y, size, size), background); }
            GUIUtility.RotateAroundPivot(base.transform.eulerAngles.y, radarPosition + new Vector2(size / 2f, size / 2f));

            //Draw Compass
            if (compass) { GUI.DrawTexture(new Rect(radarPosition.x + size / 2f - (float)compass.width / 2f, radarPosition.y + size / 2f - (float)compass.height / 2f, (float)compass.width, (float)compass.height), compass); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private Vector2 GetPosition(Vector3 position)
        {
            Vector2 cronus = Vector2.zero;
            if (m_controller)
            {
                cronus.x = radarPosition.x + (position.x - transform.position.x + size * m_scale / 2f) / m_scale;
                cronus.y = radarPosition.y + (-(position.z - transform.position.z) + size * m_scale / 2f) / m_scale;
            }
            return cronus;
        }

        /// <summary>
        /// 
        /// </summary>
        private void PositionCamera()
        {
            //TARGET CAMERA
            if (m_targets.Count > 0 && m_currentTarget != null && m_currentTarget.m_body != null)
            {
                float x = m_currentTarget.m_body.transform.position.x;
                float y = m_currentTarget.m_body.transform.position.y + cameraHeight;
                float z = m_currentTarget.m_body.transform.position.z + cameraDistance;
                Vector3 cameraPosition = new Vector3(x, y, z);

                if (targetCamera != null)
                {
                    targetCamera.transform.position = cameraPosition;
                    targetCamera.transform.LookAt(m_currentTarget.m_body.transform.position);
                }
            }
            else
            {
                if (targetCamera != null && m_controller.m_view != null)
                {
                    targetCamera.transform.position = m_controller.m_view.currentCamera.transform.position;
                    targetCamera.transform.rotation = m_controller.m_view.currentCamera.transform.rotation;
                }
            }

            if (m_lockedTarget != null && m_lockedTarget.m_body != null)
            {
                float xy = m_lockedTarget.m_body.transform.position.x;
                float yy = m_lockedTarget.m_body.transform.position.y + cameraHeight;
                float zy = m_lockedTarget.m_body.transform.position.z + cameraDistance;

                lockedCameraPosition = new Vector3(xy, yy, zy);
                if (lockedTargetCamera != null)
                {
                    lockedTargetCamera.transform.position = lockedCameraPosition;
                    if (m_lockedTarget != null && m_lockedTarget.m_body != null)
                    {
                        lockedTargetCamera.transform.LookAt(m_lockedTarget.m_body.transform.position);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (m_markTargets)
            {
                //------------------------------------------- Draw Line to each tracked object
                Gizmos.color = Color.white;
                foreach (TrackObject filteredObject in m_targets)
                {
                    if (filteredObject != null && filteredObject != m_lockedTarget && filteredObject != m_currentTarget && filteredObject.m_body != null)
                    {
                        Gizmos.DrawLine(filteredObject.m_body.transform.position, this.transform.position);
                    }
                }

                //------------------------------------------- Draw Line to current object
                if (m_currentTarget != null && m_currentTarget.m_body != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(m_currentTarget.m_body.transform.position, this.transform.position);
                }
                //-------------------------------------------- Draw Line to locked object
                if (m_lockedTarget != null && m_lockedTarget.m_body != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(m_lockedTarget.m_body.transform.position, this.transform.position);
                }
            }
        }
    }

    #region Editor
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroRadar))]
    public class SilantroRadarEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1, 0.4f, 0);
        SilantroRadar radar;

        /// <summary>
        /// 
        /// </summary>
        void OnEnable() { radar = (SilantroRadar)target; }

        /// <summary>
        /// 
        /// </summary>
        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector(); 
            serializedObject.Update();


            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Radar Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_range"), new GUIContent("Effective Range"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_maximumObjects"), new GUIContent("Track Cache Limit"));
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_advancedTracking"), new GUIContent("Targeting Mode"));
            GUILayout.Space(5f);
            SerializedProperty layerMask = serializedObject.FindProperty("m_collisionLayers");
            EditorGUILayout.PropertyField(layerMask);
            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Ping Settings", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pingRate"), new GUIContent("Ping Rate"));
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Last Ping", (radar.pingTime).ToString("0.000") + " s");


            GUILayout.Space(10f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Object Identification", MessageType.None);
            GUI.color = backgroundColor;

            if (radar.m_sceneObjects != null)
            {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField("Visible Objects", radar.m_sceneObjects.Count.ToString());
            }
            if (radar.m_targets != null)
            {
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Filtered Objects", radar.m_targets.Count.ToString());
            }
            if (radar.m_advancedTracking == ControlState.Active)
            {
                string ctn = "NULL";
                string ltn = "NULL";
                if (radar.m_currentTarget != null && radar.m_currentTarget.m_body != null) { ctn = radar.m_currentTarget.m_body.name; }
                if (radar.m_lockedTarget != null && radar.m_lockedTarget.m_body != null) { ltn = radar.m_lockedTarget.m_body.name; }

                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Selected Object", ctn);
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Locked Target", ltn);
            }


            GUILayout.Space(20f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Display Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("size"), new GUIContent("Radar Size"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("objectScale"), new GUIContent("Object Scale"));
            GUILayout.Space(5f);
            serializedObject.FindProperty("m_radarSkin").objectReferenceValue = EditorGUILayout.ObjectField("GUI Skin", serializedObject.FindProperty("m_radarSkin").objectReferenceValue, typeof(GUISkin), true) as GUISkin;
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_markTargets"), new GUIContent("Mark Objects"));


            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Texture Settings", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            serializedObject.FindProperty("background").objectReferenceValue = EditorGUILayout.ObjectField("Radar Background", serializedObject.FindProperty("background").objectReferenceValue, typeof(Texture), true) as Texture;
            GUILayout.Space(5f);
            serializedObject.FindProperty("compass").objectReferenceValue = EditorGUILayout.ObjectField("Compass", serializedObject.FindProperty("compass").objectReferenceValue, typeof(Texture), true) as Texture;
            if (radar.m_advancedTracking == ControlState.Active)
            {
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Radar Screen Icons", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                serializedObject.FindProperty("selectedTargetTexture").objectReferenceValue = EditorGUILayout.ObjectField("Selected Target", serializedObject.FindProperty("selectedTargetTexture").objectReferenceValue, typeof(Texture2D), true) as Texture2D;
                GUILayout.Space(5f);
                serializedObject.FindProperty("lockedTargetTexture").objectReferenceValue = EditorGUILayout.ObjectField("Locked Target", serializedObject.FindProperty("lockedTargetTexture").objectReferenceValue, typeof(Texture2D), true) as Texture2D;
                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Camera Screen Icons", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                serializedObject.FindProperty("TargetLockOnTexture").objectReferenceValue = EditorGUILayout.ObjectField("Selected Target", serializedObject.FindProperty("TargetLockOnTexture").objectReferenceValue, typeof(Texture2D), true) as Texture2D;
                GUILayout.Space(5f);
                serializedObject.FindProperty("TargetLockedTexture").objectReferenceValue = EditorGUILayout.ObjectField("Locked Target", serializedObject.FindProperty("TargetLockedTexture").objectReferenceValue, typeof(Texture2D), true) as Texture2D;

                GUILayout.Space(5f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Track Object Icons", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                serializedObject.FindProperty("m_aircraftTexture").objectReferenceValue = EditorGUILayout.ObjectField("Aircraft", serializedObject.FindProperty("m_aircraftTexture").objectReferenceValue, typeof(Texture2D), true) as Texture2D;
                GUILayout.Space(5f);
                serializedObject.FindProperty("m_missileTexture").objectReferenceValue = EditorGUILayout.ObjectField("Munition", serializedObject.FindProperty("m_missileTexture").objectReferenceValue, typeof(Texture2D), true) as Texture2D;
            }




            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Color Settings", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Transparency"), new GUIContent("Transparency"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("generalColor"), new GUIContent("General Color"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("labelColor"), new GUIContent("Label Color"));

            GUILayout.Space(20f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Camera Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            serializedObject.FindProperty("targetCamera").objectReferenceValue = EditorGUILayout.ObjectField("Radar Camera", serializedObject.FindProperty("targetCamera").objectReferenceValue, typeof(Camera), true) as Camera;
            if (radar.m_advancedTracking == ControlState.Active)
            {
                GUILayout.Space(3f);
                serializedObject.FindProperty("lockedTargetCamera").objectReferenceValue = EditorGUILayout.ObjectField("Locked Camera", serializedObject.FindProperty("lockedTargetCamera").objectReferenceValue, typeof(Camera), true) as Camera;
            }



            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("View Settings", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraHeight"), new GUIContent("Camera Height"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraDistance"), new GUIContent("Camera Distance"));


            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    #endregion
}
