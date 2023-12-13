using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Oyedoyin.Common
{
    #region Component
    /// <summary>
    /// 
    /// </summary>
    public class SilantroBulb : MonoBehaviour
    {
        public enum LightType { Navigation, Strobe, Beacon, Landing }
        public LightType lightType = LightType.Navigation;

        public enum CurrentState { On, Off }
        public CurrentState state = CurrentState.On;


        //----------------------------- Connections
        public GameObject bulbCore;
        public Light bulbLight;
        public Color bulbColor = Color.white, finalColor;
        Material bulbMaterial;
        public AnimationCurve flashCurve;
        public float blinkOffset = 0.2f;
        public float lightRange = 5f;


        //------------------------------ Variables
        public float currentLightIntensity, maximumLightIntensity;
        public float currentEmission, maximumEmission = 10f;
        public float currentValue, blinkFrequency = 5f;
        public float flashTime;
        public bool initialized;


        #region Call Functions
        /// <summary>
        /// 
        /// </summary>
        public void SwitchOn() { if (bulbCore) { bulbCore.SetActive(true); state = CurrentState.On; } }
        /// <summary>
        /// 
        /// </summary>
        public void SwitchOff() { if (bulbCore) { bulbCore.SetActive(false); state = CurrentState.Off; } }

        #endregion

        #region Internal Functions
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            if (bulbCore != null && bulbLight != null)
            {
                bulbMaterial = bulbCore.GetComponent<Renderer>().material;
                bulbMaterial.color = bulbColor;
                bulbCore.GetComponent<Renderer>().material = bulbMaterial;
                finalColor = bulbColor * Mathf.LinearToGammaSpace(0.0f);
                bulbMaterial.SetColor("_EmissionColor", finalColor);
                bulbLight.color = bulbColor;
                bulbLight.range = lightRange;

                //-----------------------------PLOT
                PlotFlashCurve();
                state = CurrentState.Off;
                bulbCore.SetActive(false);
                initialized = true;
            }
            else { Debug.LogError("Prerequisites not met on Light Bulb " + transform.name + "....connections not assigned properly"); }
        }
        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmos() { if (!Application.isPlaying) { PlotFlashCurve(); } }
        /// <summary>
        /// 
        /// </summary>
        void PlotFlashCurve()
        {
            flashCurve = new AnimationCurve();

            //--------------------BEACON
            if (lightType == LightType.Beacon)
            {
                //----------------FLASH ONE
                flashCurve.AddKey(new Keyframe(0, 0));
                flashCurve.AddKey(new Keyframe(1, 1));
                flashCurve.AddKey(new Keyframe(2, 0));
                flashCurve.AddKey(new Keyframe(3, 1));
                flashCurve.AddKey(new Keyframe(4, 0));
            }



            //--------------------NAVIGATION
            if (lightType == LightType.Navigation)
            {
                //----------------FLASH ONE
                flashCurve.AddKey(new Keyframe(0 + blinkOffset, 0));
                flashCurve.AddKey(new Keyframe(0.499f + blinkOffset, 0));
                flashCurve.AddKey(new Keyframe(0.5f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(0.72f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(0.721f + blinkOffset, 0f));


                //----------------FLASH TWO
                flashCurve.AddKey(new Keyframe(1.499f + blinkOffset, 0));
                flashCurve.AddKey(new Keyframe(1.5f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(1.72f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(1.721f + blinkOffset, 0f));

                //----------------FLASH THREE
                flashCurve.AddKey(new Keyframe(2.499f + blinkOffset, 0));
                flashCurve.AddKey(new Keyframe(2.5f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(2.72f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(2.721f + blinkOffset, 0f));
            }


            //--------------------STROBE
            if (lightType == LightType.Strobe)
            {
                //----------------FLASH ONE A
                flashCurve.AddKey(new Keyframe(0 + blinkOffset, 0));
                flashCurve.AddKey(new Keyframe(0.099f + blinkOffset, 0));
                flashCurve.AddKey(new Keyframe(0.1f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(0.22f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(0.221f + blinkOffset, 0f));

                //----------------FLASH ONE B
                flashCurve.AddKey(new Keyframe(0.399f + blinkOffset, 0));
                flashCurve.AddKey(new Keyframe(0.4f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(0.52f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(0.521f + blinkOffset, 0f));



                //----------------FLASH TWO A
                flashCurve.AddKey(new Keyframe(1.099f + blinkOffset, 0));
                flashCurve.AddKey(new Keyframe(1.1f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(1.22f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(1.221f + blinkOffset, 0f));

                //----------------FLASH TWO B
                flashCurve.AddKey(new Keyframe(1.399f + blinkOffset, 0));
                flashCurve.AddKey(new Keyframe(1.4f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(1.52f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(1.521f + blinkOffset, 0f));



                //----------------FLASH THREE A
                flashCurve.AddKey(new Keyframe(2.099f + blinkOffset, 0));
                flashCurve.AddKey(new Keyframe(2.1f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(2.22f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(2.221f + blinkOffset, 0f));

                //----------------FLASH THREE B
                flashCurve.AddKey(new Keyframe(2.399f + blinkOffset, 0));
                flashCurve.AddKey(new Keyframe(2.4f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(2.52f + blinkOffset, 1f));
                flashCurve.AddKey(new Keyframe(2.521f + blinkOffset, 0f));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_timestep"></param>
        public void Compute(float _timestep)
        {
            if (initialized)
            {
                if (state == CurrentState.On)
                {

                    //---------------------------------------------NAVIGATION
                    if (lightType == LightType.Navigation || lightType == LightType.Strobe)
                    {
                        //CORE BLINK
                        flashTime += _timestep * blinkFrequency;
                        currentValue = flashCurve.Evaluate(flashTime);
                        if (flashTime > 3f) { flashTime = 0f; }


                        //-----------------DATA SET
                        currentEmission = maximumEmission * currentValue; currentLightIntensity = currentValue * maximumLightIntensity;
                        bulbLight.intensity = currentLightIntensity;
                        finalColor = bulbColor * Mathf.LinearToGammaSpace(currentEmission * 1000);
                        bulbMaterial.SetColor("_EmissionColor", finalColor);
                    }


                    //---------------------------------------------BEACON
                    if (lightType == LightType.Beacon)
                    {
                        //CORE BLINK
                        flashTime += _timestep * blinkFrequency;
                        currentValue = flashCurve.Evaluate(flashTime);
                        if (flashTime > 4f) { flashTime = 0f; }

                        //-----------------DATA SET
                        currentEmission = maximumEmission * currentValue; currentLightIntensity = currentValue * maximumLightIntensity;
                        bulbLight.intensity = currentLightIntensity;
                        finalColor = bulbColor * Mathf.LinearToGammaSpace(currentEmission * 1000);
                        bulbMaterial.SetColor("_EmissionColor", finalColor);
                    }



                    //---------------------------------------------LANDING
                    if (lightType == LightType.Landing)
                    {
                        bulbLight.intensity = maximumLightIntensity;
                        finalColor = bulbColor * Mathf.LinearToGammaSpace(maximumEmission * 1000);
                        bulbMaterial.SetColor("_EmissionColor", finalColor);
                    }
                }
            }
        }
        #endregion
    }
    #endregion

    #region Editor
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroBulb))]
    public class BulbEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1, 0.4f, 0);
        SilantroBulb bulb;




        //------------------------------------------------------------------------
        private SerializedProperty bulbType;
        private SerializedProperty bulbCore;
        private SerializedProperty bulbColor;
        private SerializedProperty bulbLight;
        private SerializedProperty emission;
        private SerializedProperty intensity;
        private SerializedProperty frequency;
        private SerializedProperty pattern;
        private SerializedProperty blinkOffset;

        void OnEnable()
        {
            bulb = (SilantroBulb)target;

            blinkOffset = serializedObject.FindProperty("blinkOffset");
            bulbType = serializedObject.FindProperty("lightType");
            bulbCore = serializedObject.FindProperty("bulbCore");
            bulbColor = serializedObject.FindProperty("bulbColor");
            bulbLight = serializedObject.FindProperty("bulbLight");
            emission = serializedObject.FindProperty("maximumEmission");
            intensity = serializedObject.FindProperty("maximumLightIntensity");
            frequency = serializedObject.FindProperty("blinkFrequency");
            pattern = serializedObject.FindProperty("flashCurve");
        }

        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector();
            serializedObject.Update();


            //------------------------------------------------------------------------
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Bulb Type", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);


            //------------------------------------------------------------------------
            EditorGUILayout.PropertyField(bulbType);
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("State", bulb.state.ToString());

            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Connections", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(bulbCore);
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(bulbLight);
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(bulbColor);

            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Bulb Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(emission);
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(intensity);
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lightRange"), new GUIContent("Maximum Range"));

            if (bulb.lightType != SilantroBulb.LightType.Landing)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(frequency);
            }
            if (bulb.lightType == SilantroBulb.LightType.Navigation || bulb.lightType == SilantroBulb.LightType.Strobe)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(blinkOffset);
            }


            if (bulb.lightType != SilantroBulb.LightType.Landing)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(pattern);
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    #endregion
}