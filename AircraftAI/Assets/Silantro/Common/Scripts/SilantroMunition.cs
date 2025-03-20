using UnityEngine;
using System.Collections;
using Oyedoyin.Common.Components;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Oyedoyin.Common
{
    /// <summary>
    /// 
    /// </summary>
    #region Component
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class SilantroMunition : MonoBehaviour
    {
        public enum MunitionType { Missile, Rocket, Bullet, Bomb }
        public enum AmmunitionForm
        {
            SecantOgive,//0.171
            TangentOgive,//0.165
            RoundNose,//0.235
            FlatNose,//0.316
            Spitzer//0.168
        }

        public MunitionType munitionType = MunitionType.Rocket;
        public AmmunitionForm ammunitionForm = AmmunitionForm.SecantOgive;
        private Collider[] hitColliders = new Collider[30];
        public LayerMask m_collisionLayers;


        // ---------------------------------------- Variables
        public string m_identifier;
        public float munitionWeight = 500f;
        public float munitionDiameter = 1f;
        public float munitionLength = 1f;
        public float maximumRange = 1000f;
        public float activationDistance = 30;
        public float maximumSpeed = 1.5f;
        public float ballisticVelocity = 800;
        public float detonationRange = 10;
        private float closestDistance = 1000;
        public float targetDistance;
        private float m_distanceToParent;
        public float destroyTime = 6;
        public float damage = 100;
        public float surfaceArea, skinningRatio, baseCoefficient;


        // ---------------------------------------- Connections
        public Rigidbody m_rigidbody;
        public Transform m_parent;
        public Transform m_exitPoint;
        public Transform m_target;
        public RocketMotor m_motor;
        public SilantroPylon m_pylon;


        // ---------------------------------------- Effects
        public GameObject groundHit;
        public GameObject metalHit;
        public GameObject woodHit; //ADD MORE
        public GameObject explosionPrefab;

        // ---------------------------------------- Bools
        public bool m_armed;
        public bool m_exploded;
        public bool m_falling;
        public float m_lifeTime;
        public bool m_seeking;
        public bool m_active;

        // ---------------------------------------- Navigation
        public float lockDirection;
        public float minimumLockDirection = 0.4f;
        public float navigationConstant = 3f;
        public float maximumTurnRate = 150f;
        public Vector3 sightLine;
        public Vector3 bodyRate;
        Vector3 lastPosition;
        public float distanceTravelled;


        #region Call Functions

        /// <summary>
        /// Fires guided munition.
        /// </summary>
        /// <param name="markedTarget"></param>
        /// <param name="m_velocity"></param>
        public void FireMunition(Transform markedTarget, Vector3 m_velocity, int mode)
        {
            //1. DROP MISSILE
            if (mode == 1)
            {
                m_rigidbody.transform.parent = null;
                m_rigidbody.isKinematic = false;
                m_rigidbody.linearVelocity = m_velocity;
                //FIRE
                StartCoroutine(WaitForDrop(markedTarget));
            }
            //2. TUBE LAUNCH
            if (mode == 2)
            {
                //FIRE
                m_rigidbody.transform.parent = null;
                m_rigidbody.isKinematic = false;
                m_rigidbody.linearVelocity = m_velocity;
                m_rigidbody.useGravity = false;

                m_target = markedTarget;
                m_motor.Fire();
                m_active = true;
                m_seeking = true;
            }


            //3. TRAPEZE LAUNCH RIGHT
            if (mode == 3)
            {
                m_rigidbody.transform.parent = null;
                m_rigidbody.isKinematic = false;
                m_rigidbody.linearVelocity = m_velocity;

                //PUSH OUT
                float pushForce = m_rigidbody.mass * 500f;
                Vector3 force = m_rigidbody.transform.right * pushForce;
                m_rigidbody.AddForce(force);
                //FIRE
                StartCoroutine(WaitForDrop(markedTarget));
            }

            //4. TRAPEZE LAUNCH LEFT
            if (mode == 4)
            {
                m_rigidbody.transform.parent = null;
                m_rigidbody.isKinematic = false;
                m_rigidbody.linearVelocity = m_velocity;
                //PUSH OUT
                float pushForce = m_rigidbody.mass * 500f;
                Vector3 force = m_rigidbody.transform.right * -pushForce;
                m_rigidbody.AddForce(force);
                //FIRE
                StartCoroutine(WaitForDrop(markedTarget));
            }

            //5. TRAPEZE LAUNCH MIDDLE
            if (mode == 5)
            {
                m_rigidbody.transform.parent = null;
                m_rigidbody.isKinematic = false;
                m_rigidbody.linearVelocity = m_velocity;
                //PUSH OUT
                float pushForce = m_rigidbody.mass * 800f;
                Vector3 force = m_rigidbody.transform.up * -pushForce;
                m_rigidbody.AddForce(force);
                //FIRE
                StartCoroutine(WaitForDrop(markedTarget));
            }
            //REMOVE PYLON
            if (m_pylon != null && m_pylon.pylonPosition == SilantroPylon.PylonPosition.External)
            {
                Destroy(m_pylon.gameObject);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="markedTarget"></param>
        /// <returns></returns>
        IEnumerator WaitForDrop(Transform markedTarget)
        {
            yield return new WaitForSeconds(1f);

            ////FIRE
            m_motor.Fire();
            m_target = markedTarget;

            //ACTIVATE SEEKER
            m_active = true;
            m_seeking = true;
            m_rigidbody.useGravity = false;
        }
        /// <summary>
        /// Fires the bullet.
        /// </summary>
        /// <param name="muzzleVelocity"> muzzle/exit velocity of the gun.</param>
        /// <param name="m_velocity">velocity vector of the parent aircraft.</param>
        public void FireBullet(float muzzleVelocity, Vector3 m_velocity)
        {
            //DETERMINE INITIAL SPEED
            float startingSpeed;
            if (muzzleVelocity > ballisticVelocity) { startingSpeed = muzzleVelocity; }
            else { startingSpeed = ballisticVelocity; }

            //ADD BASE SPEED
            Vector3 ejectVelocity = transform.forward * startingSpeed;
            Vector3 resultantVelocity = ejectVelocity + m_velocity;

            //RELEASE BULLET
            m_rigidbody.isKinematic = false;
            m_rigidbody.linearVelocity = resultantVelocity;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m_velocity"></param>
        public void FireRocket(Vector3 m_velocity)
        {
            m_rigidbody.transform.parent = null;
            m_rigidbody.isKinematic = false;
            m_rigidbody.linearVelocity = m_velocity;
            m_motor.Fire();
            m_active = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m_velocity"></param>
        public void ReleaseMunition(Vector3 m_velocity)
        {
            m_rigidbody.transform.parent = null;
            m_rigidbody.isKinematic = false;
            m_rigidbody.linearVelocity = m_velocity;
            m_active = true;
        }


        #endregion



        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            // ---------------------------------- Drag Settings
            if (ammunitionForm == AmmunitionForm.RoundNose) { skinningRatio = 0.95f; baseCoefficient = 0.0235f; }
            if (ammunitionForm == AmmunitionForm.SecantOgive) { skinningRatio = 0.913f; baseCoefficient = 0.0171f; }
            if (ammunitionForm == AmmunitionForm.TangentOgive) { skinningRatio = 0.914f; baseCoefficient = 0.0165f; }
            surfaceArea = skinningRatio * 2f * Mathf.PI * (munitionDiameter / 2f) * munitionLength;

            m_rigidbody = GetComponent<Rigidbody>();
            if (m_rigidbody == null)
            {
                Debug.LogError("Prerequisites not met on " + transform.name + ".... rigidbody not assigned");
                return;
            }
            else
            {

                if (munitionType == MunitionType.Bullet)
                {
                    m_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    float mass = (munitionWeight / 15.4324f) / 1000;
                    m_rigidbody.mass = mass;
                }
                else
                {
                    m_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    m_rigidbody.mass = munitionWeight;
                    m_rigidbody.isKinematic = true;
                }
            }
            if (m_exitPoint == null) { m_exitPoint = this.transform; }
            m_motor.exitPoint = m_exitPoint;
            lastPosition = transform.position;
            m_motor.Initialize();
            m_armed = false;
        }
        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmos()
        {
            if (munitionType != MunitionType.Bullet) { m_motor.DrawGizmos(); }
        }
        /// <summary>
        /// 
        /// </summary>
        private void FixedUpdate()
        {
            if (munitionType == MunitionType.Bullet)
            {
                m_lifeTime += Time.deltaTime;
                if (m_lifeTime > destroyTime) { Destroy(this.gameObject); }
            }
            else
            {
                if (m_active)
                {
                    //----------------------------------- Data
                    float munitionSpeed = m_rigidbody.linearVelocity.magnitude;
                    float airDensity = 0;
                    float soundSpeed = 0;

                    if (munitionSpeed > 1)
                    {
                        float altitude = m_rigidbody.gameObject.transform.position.y * 3.28084f;
                        float a = 0.0000004f * altitude * altitude;
                        float b = (0.0351f * altitude);
                        float ambientPressure = (a - b + 1009.6f) / 10f;
                        float a1 = 0.000000003f * altitude * altitude;
                        float a2 = 0.0021f * altitude;
                        float ambientTemperature = a1 - a2 + 15.443f;
                        float kelvinTemperatrue = ambientTemperature + 273.15f;
                        airDensity = (ambientPressure * 1000f) / (287.05f * kelvinTemperatrue);
                        soundSpeed = Mathf.Pow((1.2f * 287f * (273.15f + ambientTemperature)), 0.5f);
                    }

                    // Thrust Force
                    m_motor.Compute(Time.fixedDeltaTime);
                    Vector3 force = m_rigidbody.transform.forward * m_motor.m_thrust;
                    m_rigidbody.AddForce(force, ForceMode.Force);

                    // Drag Force
                    float trueSpeed = soundSpeed * maximumSpeed;
                    float dynamicForce = 0.5f * airDensity * trueSpeed * trueSpeed * surfaceArea;
                    float dragCoefficient = m_motor.m_thrust / dynamicForce;
                    if (float.IsNaN(dragCoefficient) || float.IsInfinity(dragCoefficient)) { dragCoefficient = 0.01f; }
                    float drag = 0.5f * airDensity * dragCoefficient * munitionSpeed * munitionSpeed * surfaceArea;
                    Vector3 dragForce = m_rigidbody.linearVelocity.normalized * -drag;
                    if (!float.IsNaN(drag) && !float.IsInfinity(drag)) { m_rigidbody.AddForce(dragForce, ForceMode.Force); }

                    // Check For Contact
                    if (m_armed) { CheckCollision(); }
                }
            }

            distanceTravelled += Vector3.Distance(transform.position, lastPosition);
            lastPosition = transform.position;
        }
        /// <summary>
        /// 
        /// </summary>
        private void LateUpdate()
        {
            if (munitionType == MunitionType.Missile && m_active)
            {
                // ------------------ Out of Range
                if (m_target == null && distanceTravelled > maximumRange) { Explode(Quaternion.identity, transform.position, 0); }

                // ------------------ Navigation
                if (m_seeking)
                {
                    Vector3 targetDirection = (m_target.transform.position - m_rigidbody.transform.position).normalized;
                    lockDirection = Vector3.Dot(targetDirection, transform.forward);
                    if (lockDirection < minimumLockDirection) { m_seeking = false; }
                    Vector3 prevSightLine = sightLine;
                    sightLine = m_target.position - transform.position;
                    Vector3 δLOS = sightLine - prevSightLine;
                    δLOS = δLOS - Vector3.Project(δLOS, sightLine);
                    bodyRate = Time.fixedDeltaTime * sightLine + δLOS * navigationConstant + Time.fixedDeltaTime * bodyRate * navigationConstant / 2;
                    float acceleration = m_motor.m_thrust / munitionWeight;
                    bodyRate = Vector3.ClampMagnitude(bodyRate * acceleration, acceleration);
                    Quaternion targetRotation = Quaternion.LookRotation(bodyRate, transform.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * maximumTurnRate);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected void CheckCollision()
        {

            // Check for colliders
            Physics.OverlapSphereNonAlloc(transform.position, 200, hitColliders, m_collisionLayers);

            // Filter
            for (int i = 0; i < hitColliders.Length; i++)
            {
                Collider m_collider = hitColliders[i];
                if (m_collider != null && m_collider.transform != transform)
                {
                    Vector3 contactPosition = m_collider.ClosestPointOnBounds(transform.position);
                    closestDistance = Vector3.Distance(transform.position, contactPosition);
                    if (closestDistance <= 2) { Explode(Quaternion.identity, transform.position, 0); }

                    if (munitionType == MunitionType.Missile)
                    {
                        if (m_target != null && m_collider.transform == m_target)
                        {
                            targetDistance = closestDistance;
                            if (closestDistance < detonationRange) { Explode(Quaternion.identity, transform.position, closestDistance); }
                        }
                        if (m_target == null && closestDistance < 2) { Explode(Quaternion.identity, transform.position, 0); }
                    }
                    else
                    {
                        if (closestDistance <= detonationRange) { }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            if (m_parent != null)
            {
                m_distanceToParent = Vector3.Distance(m_parent.transform.position, transform.position);
                if (m_active && m_distanceToParent > activationDistance && !m_armed) { m_armed = true; }
            }
        }









        /// <summary>
        /// 
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionEnter(Collision collision)
        {
            if (munitionType == MunitionType.Bullet)
            {
                // Impact Effects
                if (collision.collider.CompareTag("Ground") && groundHit != null)
                { Instantiate(groundHit, collision.contacts[0].point, Quaternion.FromToRotation(Vector3.up, collision.contacts[0].normal)); }
                if (collision.collider.CompareTag("Wood") && woodHit != null)
                { Instantiate(woodHit, collision.contacts[0].point, Quaternion.FromToRotation(Vector3.up, collision.contacts[0].normal)); }
                if (collision.collider.CompareTag("Metal") && metalHit != null)
                { Instantiate(metalHit, collision.contacts[0].point, Quaternion.FromToRotation(Vector3.up, collision.contacts[0].normal)); }

                // Destroy Object
                Destroy(gameObject);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="collisionRotation"></param>
        /// <param name="collisionPosition"></param>
        public void Explode(Quaternion collisionRotation, Vector3 collisionPosition, float triggerDistance)
        {
            if (explosionPrefab != null && !m_exploded)
            {
                GameObject explosion = Instantiate(explosionPrefab, collisionPosition, collisionRotation);
                explosion.SetActive(true);
                explosion.GetComponentInChildren<AudioSource>().Play();
                m_exploded = true;
            }

            // Destroy Object
            Destroy(gameObject);
        }
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
#region Editor
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroMunition))]
    public class MunitionEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1, 0.4f, 0);
        SilantroMunition munition;
        SerializedProperty rocket;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            munition = (SilantroMunition)target;
            rocket = serializedObject.FindProperty("m_motor");
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector();
            serializedObject.Update();


            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Munition Type", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionType"), new GUIContent(" "));
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Identifier", munition.transform.name);


            if (munition.munitionType == SilantroMunition.MunitionType.Bullet)
            {
                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Bullet Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ammunitionForm"), new GUIContent("Ammunition Form"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionWeight"), new GUIContent("Mass"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionLength"), new GUIContent("Overall Length"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionDiameter"), new GUIContent("Diameter"));
                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Performance Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ballisticVelocity"), new GUIContent("Ballistic Velocity"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"), new GUIContent("Damage"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("destroyTime"), new GUIContent("Destroy Time"));

            }

            if (munition.munitionType == SilantroMunition.MunitionType.Missile)
            {
                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Missile Dimensions", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionWeight"), new GUIContent("Weight (kg)"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionDiameter"), new GUIContent("Diameter (m)"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionLength"), new GUIContent("Length (m)"));

                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumSpeed"), new GUIContent("Mach Limit"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("activationDistance"), new GUIContent("Activation Distance (m)"));


                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Rocket Motor", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("burnType"), new GUIContent("Burn Type"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("burnCurve"), new GUIContent("Burn Curve"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("m_meanThrust"), new GUIContent("Maximum Thrust"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("fireDuration"), new GUIContent("Burn Duration"));


                GUILayout.Space(15f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Detonation System", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Armed State", munition.m_armed.ToString());
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("detonationRange"), new GUIContent("Trigger Distance (m)"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_collisionLayers"), new GUIContent("Collision Layers"));

                if (munition.m_target != null)
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.LabelField("Current Target", munition.m_target.name);
                    GUILayout.Space(3f);
                    EditorGUILayout.LabelField("Distance To Target", munition.targetDistance.ToString("0.00") + " m");
                }
                else
                {
                    GUILayout.Space(3f);
                    EditorGUILayout.LabelField("Current Target", "Null");
                }
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Distance Traveled", munition.distanceTravelled.ToString("0.0") + " m");
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumRange"), new GUIContent("Maximum Range"));

                GUILayout.Space(10f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Warhead Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionPrefab"), new GUIContent("Explosion Prefab"));
                if (munition.explosionPrefab != null)
                {
                    SilantroExplosion explosive = munition.explosionPrefab.GetComponent<SilantroExplosion>();
                    if (explosive != null)
                    {
                        GUILayout.Space(3f);
                        EditorGUILayout.LabelField("Explosive Force", explosive.explosionForce.ToString("0.0") + " N");
                        GUILayout.Space(1f);
                        EditorGUILayout.LabelField("Explosive Radius", explosive.explosionRadius.ToString("0") + " m");
                    }
                }


                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Rocket Effects", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("exhaustSmoke"), new GUIContent("Exhaust Smoke"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("maximumSmokeEmissionValue"), new GUIContent("Maximum Emission"));
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("exhaustFlame"), new GUIContent("Exhaust Flame"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("maximumFlameEmissionValue"), new GUIContent("Maximum Emission"));

                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Sound Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("motorSound"), new GUIContent("Booster Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("maximumPitch"), new GUIContent("Maximum Pitch"));

                GUILayout.Space(10f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Output", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Thrust Generated", munition.m_motor.m_thrust.ToString("0.00") + " N");
            }

            if (munition.munitionType == SilantroMunition.MunitionType.Rocket)
            {
                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Missile Dimensions", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionWeight"), new GUIContent("Weight (kg)"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionDiameter"), new GUIContent("Diameter (m)"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("munitionLength"), new GUIContent("Length (m)"));

                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumSpeed"), new GUIContent("Mach Limit"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("activationDistance"), new GUIContent("Activation Distance (m)"));


                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Rocket Motor", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("burnType"), new GUIContent("Burn Type"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("burnCurve"), new GUIContent("Burn Curve"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("m_meanThrust"), new GUIContent("Maximum Thrust"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("fireDuration"), new GUIContent("Burn Duration"));


                GUILayout.Space(15f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Detonation System", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Armed State", munition.m_armed.ToString());
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("detonationRange"), new GUIContent("Trigger Distance (m)"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_collisionLayers"), new GUIContent("Collision Layers"));
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Distance Traveled", munition.distanceTravelled.ToString("0.0") + " m");
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumRange"), new GUIContent("Maximum Range"));

                GUILayout.Space(10f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Warhead Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionPrefab"), new GUIContent("Explosion Prefab"));
                if (munition.explosionPrefab != null)
                {
                    SilantroExplosion explosive = munition.explosionPrefab.GetComponent<SilantroExplosion>();
                    if (explosive != null)
                    {
                        GUILayout.Space(3f);
                        EditorGUILayout.LabelField("Explosive Force", explosive.explosionForce.ToString("0.0") + " N");
                        GUILayout.Space(1f);
                        EditorGUILayout.LabelField("Explosive Radius", explosive.explosionRadius.ToString("0") + " m");
                    }
                }


                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Rocket Effects", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("exhaustSmoke"), new GUIContent("Exhaust Smoke"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("maximumSmokeEmissionValue"), new GUIContent("Maximum Emission"));
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("exhaustFlame"), new GUIContent("Exhaust Flame"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("maximumFlameEmissionValue"), new GUIContent("Maximum Emission"));

                GUILayout.Space(10f);
                GUI.color = silantroColor;
                EditorGUILayout.HelpBox("Sound Configuration", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("motorSound"), new GUIContent("Booster Sound"));
                GUILayout.Space(2f);
                EditorGUILayout.PropertyField(rocket.FindPropertyRelative("maximumPitch"), new GUIContent("Maximum Pitch"));

                GUILayout.Space(10f);
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Output", MessageType.None);
                GUI.color = backgroundColor;
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Thrust Generated", munition.m_motor.m_thrust.ToString("0.00") + " N");
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    #endregion
}
