#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace Oyedoyin.Common
{
    /// <summary>
    /// 
    /// </summary>
    #region Component
    public class SilantroExplosion : MonoBehaviour
    {
        // Variables
        public float damage = 200f;
        public float explosionForce = 4000f;
        public float explosionRadius = 45f;
        float fractionalDistance;
        private Collider[] hitColliders = new Collider[50];

        // Lights
        public AnimationCurve LightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float exposureTime = 1;
        public float lightIntensity = 5;
        private bool canUpdate;
        private float startTime;
        public Light lightSource;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            //EXPLOSION LIGHT
            lightSource = GetComponentInChildren<Light>();
            if (lightSource)
            {
                lightSource.intensity = LightCurve.Evaluate(0);
                startTime = Time.time;
                canUpdate = true;
            }
            //EFFECT
            Explode();
        }
        /// <summary>
        /// 
        /// </summary>
        private void Explode()
        {
            //AQUIRE SURROUNDING COLLIDERS
            Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, hitColliders);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                Collider hit = hitColliders[i];
                if (hit != null)
                {
                    //DISTANCE FALLOFF
                    float distanceToObject = Vector3.Distance(transform.position, hit.gameObject.transform.position);
                    fractionalDistance = (1 - (distanceToObject / explosionRadius));
                    Vector3 exploionPosition = transform.position;

                    //ONLY AFFECT OBJECTS WITHIN RANGE
                    if (distanceToObject < explosionRadius)
                    {
                        //SEND DAMAGE MESSAGE
                        float actualDamage = damage * fractionalDistance;
                        hit.gameObject.SendMessageUpwards("SilantroDamage", (-actualDamage), SendMessageOptions.DontRequireReceiver);

                        //FORCE
                        //1. OBJECT ITSELF
                        if (hit.GetComponent<Rigidbody>())
                        {
                            float actualForce = explosionForce * fractionalDistance;
                            hit.GetComponent<Rigidbody>().AddExplosionForce(actualForce, transform.position, explosionRadius, 3f, ForceMode.Impulse);
                        }
                        //2. OBJECT PARENT
                        else if (hit.transform.root.gameObject.GetComponent<Rigidbody>())
                        {
                            float actualForce = explosionForce * fractionalDistance;
                            hit.transform.root.gameObject.GetComponent<Rigidbody>().AddExplosionForce(actualForce, transform.position, explosionRadius, (3.0f), ForceMode.Impulse);
                        }

                        //3 Component Damage
                        SilantroMisc misc = hit.gameObject.GetComponent<SilantroMisc>();
                        if (misc != null)
                        {
                            if (misc.m_function == SilantroMisc.Function.Transponder)
                            {
                                Debug.Log("Object " + hit.transform.name + " has been destroyed");
                                Destroy(misc);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            if (lightSource)
            {
                float time = Time.time - startTime;
                if (canUpdate)
                {
                    float eval = LightCurve.Evaluate(time / exposureTime) * lightIntensity;
                    lightSource.intensity = eval;
                }
                if (time >= exposureTime)
                {
                    canUpdate = false;
                }
            }
        }
    }
    #endregion


    #region Editor
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroExplosion))]
    public class SilantroExplosionEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1, 0.4f, 0);
        /// <summary>
        /// 
        /// </summary>
        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector ();
            serializedObject.Update();



            GUILayout.Space(3f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Properties", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"), new GUIContent("Damage"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionForce"), new GUIContent("Explosion Force"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionRadius"), new GUIContent("Effective Radius"));


            GUILayout.Space(15f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Light Settings", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lightIntensity"), new GUIContent("Maximum Intensity"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("exposureTime"), new GUIContent("Exposure Time"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LightCurve"), new GUIContent("Decay Curve"));

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    #endregion
}
