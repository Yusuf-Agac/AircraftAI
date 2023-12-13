#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Oyedoyin.Common.Misc;


namespace Oyedoyin.Common
{
    /// <summary>
    /// 
    /// </summary>
    #region Component
    public class SilantroGun : MonoBehaviour
    {
        public enum BulletType { Raycast, Rigidbody }
        public BulletType bulletType;
        public RotationAxis rotationAxis = RotationAxis.X;
        public RotationDirection rotationDirection = RotationDirection.CCW;


        // ----------------------------------------------- Connections
        public Rigidbody m_rigidbody;
        public GameObject ammunition;
        public GameObject m_bullet;

        public Transform[] muzzles;
        private Transform currentMuzzle;
        public Transform barrel;

        public GameObject bulletCase;
        public Transform shellEjectPoint;


        // ----------------------------------------------- Variables
        public float rateOfFire = 500;
        public float actualRate;
        public float fireTimer;
        public float accuracy = 80f;
        public float currentAccuracy;
        public float accuracyDrop = 0.2f;
        public float accuracyRecover = 0.5f;
        float acc;
        float bulletMass;

        public float muzzleVelocity = 500;
        public float barrelLength = 2f;
        public float gunWeight = 20;
        public float drumWeight;
        public float damage;
        private float barrelRPM;
        public float currentRPM;


        public float projectileForce;
        public float damperStrength = 90f;
        public int ammoCapacity = 1000;
        public int currentAmmo;
        public bool unlimitedAmmo;
        private int muzzle = 0;
        public bool advancedSettings;
        public float range = 1000f;
        public float rangeRatio = 1f;


        private readonly float shellSpitForce = 1.5f;
        private readonly float shellForceRandom = 1.5f;
        private readonly float shellSpitTorqueX = 0.5f;
        private readonly float shellSpitTorqueY = 0.5f;
        private readonly float shellTorqueRandom = 1.0f;
        public bool ejectShells = false;
        public bool canFire = true;
        public bool running;
        bool allOk;

        // ----------------------------------------------- Sounds
        public AudioClip fireLoopSound;
        public AudioClip fireEndSound;
        public float soundVolume = 0.75f;
        public AudioSource gunLoopSource, gunEndSource;
        public float soundRange = 150f;


        // ---------------------------------------------- Effects
        public GameObject muzzleFlash;
        public GameObject groundHit;
        public GameObject metalHit;
        public GameObject woodHit; //ADD MORE

        /// <summary>
        /// 
        /// </summary>
        public void FireGun(Vector3 velocity)
        {
            if (canFire) { if (fireTimer > actualRate) { Fire(velocity); } }
            //OFFLINE
            else { Debug.Log("Gun System Offline"); }
        }
        /// <summary>
        /// 
        /// </summary>
        protected void CheckPrerequisites()
        {
            if (fireLoopSound != null && fireEndSound != null)
            {
                allOk = true;
            }
            if (fireEndSound == null)
            {
                Debug.LogError("Prerequisites not met on " + transform.name + "....fire end clip not assigned");
                allOk = false;
            }
            else if (fireLoopSound == null)
            {
                Debug.LogError("Prerequisites not met on " + transform.name + "....fire loop clip not assigned");
                allOk = false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            CheckPrerequisites();


            if (allOk)
            {
                //SETUP FIRE RATE
                if (rateOfFire > 0)
                {
                    float secFireRate = rateOfFire / 60f; //FROM RPM TO RPS
                    actualRate = 1.0f / secFireRate;
                }
                else { actualRate = 0.01f; }
                fireTimer = 0.0f;

                // -------------------------------------------------- Base
                currentAmmo = ammoCapacity;
                barrelRPM = rateOfFire;
                currentAccuracy = accuracy;
                CountBullets();


                if (fireLoopSound) { Handler.SetupSoundSource(this.transform, fireLoopSound, "Loop Sound Point", soundRange, true, false, out gunLoopSource); gunLoopSource.volume = soundVolume; }
                if (fireEndSound) { Handler.SetupSoundSource(this.transform, fireEndSound, "End Sound Point", soundRange, false, false, out gunEndSource); gunEndSource.volume = soundVolume; }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void CountBullets()
        {
            if (bulletType == BulletType.Rigidbody)
            {
                if (ammunition != null && ammunition.GetComponent<SilantroMunition>() != null)
                {
                    bulletMass = ammunition.GetComponent<SilantroMunition>().munitionWeight;
                }
                if (ammunition == null) { Debug.Log("Gun " + transform.name + " ammunition gameobject has not been assigned"); return; }
                if (ammunition.GetComponent<SilantroMunition>() == null) { Debug.Log("Gun " + transform.name + " bullet gameobject is invalid, use the prefabs in the Prefabs/Sample/Ammunition/Bullets folder"); }
            }
            else { bulletMass = 200f; }
            drumWeight = currentAmmo * ((bulletMass * 6.6666667e-05f));
            if (currentAmmo > 0) { canFire = true; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected void Fire(Vector3 m_velocity)
        {
            if (muzzles.Length > 0)
            {
                // --------------------------------------------------SELECT A MUZZLE
                muzzle += 1;
                if (muzzle > (muzzles.Length - 1)) { muzzle = 0; }
                currentMuzzle = muzzles[muzzle];
                fireTimer = 0f;

                // --------------------------------------------------REDUCE AMMO COUNT
                if (!unlimitedAmmo) { currentAmmo--; }
                CountBullets();

                // --------------------------------------------------FIRE DIRECTION AND ACCURACY
                Vector3 direction = currentMuzzle.forward;
                Ray rayout = new Ray(currentMuzzle.position, direction);
                if (Physics.Raycast(rayout, out RaycastHit hitout, range / rangeRatio)) { acc = 1 - ((hitout.distance) / (range / rangeRatio)); }

                // --------------------------------------------------VARY ACCURACY
                float accuracyVary = (100 - currentAccuracy) / 1000;
                direction.x += Random.Range(-accuracyVary, accuracyVary);
                direction.y += Random.Range(-accuracyVary, accuracyVary);
                direction.z += Random.Range(-accuracyVary, accuracyVary);
                currentAccuracy -= accuracyDrop;
                if (currentAccuracy <= 0.0f) currentAccuracy = 0.0f;
                Quaternion muzzleRotation = Quaternion.LookRotation(direction);

                //1. FIRE RIGIDBODY AMMUNITION
                if (bulletType == BulletType.Rigidbody)
                {
                    //SHOOT RIGIDBODY
                    m_bullet = Instantiate(ammunition, currentMuzzle.position, muzzleRotation);
                    SilantroMunition munition = m_bullet.GetComponent<SilantroMunition>();
                    if (munition != null)
                    {
                        munition.Initialize();
                        munition.m_armed = true;
                        munition.woodHit = woodHit;
                        munition.metalHit = metalHit;
                        munition.groundHit = groundHit;
                        munition.FireBullet(muzzleVelocity, m_velocity);
                    }
                }

                //2. FIRE RAYCAST AMMUNITION
                if (bulletType == BulletType.Raycast)
                {
                    //SETUP RAYCAST
                    Ray ray = new Ray(currentMuzzle.position, direction);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, range / rangeRatio))
                    {
                        //DAMAGE
                        float damageeffect = damage * acc;
                        hit.collider.gameObject.SendMessage("SilantroDamage", -damageeffect, SendMessageOptions.DontRequireReceiver);
                        //INSTANTIATE EFFECTS
                        if (hit.collider.CompareTag("Ground") && groundHit != null) { Instantiate(groundHit, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)); }
                        //METAL
                        if (hit.collider.CompareTag("Metal") && metalHit != null) { Instantiate(metalHit, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)); }
                        //WOOD
                        if (hit.collider.CompareTag("Wood") && woodHit != null) { Instantiate(woodHit, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal)); }
                    }
                }

                // --------------------------------------------------RECOIL
                if (m_rigidbody != null)
                {
                    //SET BULLET WEIGHT
                    float bulletWeight;
                    if (bulletType == BulletType.Rigidbody)
                    {
                        bulletWeight = m_bullet.GetComponent<SilantroMunition>().munitionWeight;
                    }
                    else
                    {
                        bulletWeight = 150f;
                    }
                    float ballisticEnergy = 0.5f * ((bulletWeight * 0.0648f) / 1000f) * muzzleVelocity * muzzleVelocity * Random.Range(0.9f, 1f);
                    projectileForce = ballisticEnergy / barrelLength;

                    //APPLY
                    Vector3 recoilForce = m_rigidbody.transform.forward * (-projectileForce * (1 - (damperStrength / 100f)));
                    m_rigidbody.AddForce(recoilForce, ForceMode.Impulse);
                }

                // --------------------------------------------------MUZZLE FLASH
                if (muzzleFlash != null)
                {
                    GameObject flash = Instantiate(muzzleFlash, currentMuzzle.position, currentMuzzle.rotation);
                    flash.transform.position = currentMuzzle.position;
                    flash.transform.parent = currentMuzzle.transform;
                }

                // --------------------------------------------------SHELLS
                if (ejectShells && bulletCase != null)
                {
                    GameObject shellGO = Instantiate(bulletCase, shellEjectPoint.position, shellEjectPoint.rotation);
                    shellGO.GetComponent<Rigidbody>().velocity = m_velocity;
                    shellGO.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(shellSpitForce + Random.Range(0, shellForceRandom), 0, 0), ForceMode.Impulse);
                    shellGO.GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(shellSpitTorqueX + Random.Range(-shellTorqueRandom, shellTorqueRandom), shellSpitTorqueY + Random.Range(-shellTorqueRandom, shellTorqueRandom), 0), ForceMode.Impulse);
                }
            }

            //NO AVAILABLE MUZZLE
            else { Debug.Log("Gun barrels not setup properly"); }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Compute()
        {
            if (allOk && canFire)
            {
                if (running && currentAmmo <= 0) { gunLoopSource.Stop(); gunEndSource.PlayOneShot(fireEndSound); running = false; }

                if (fireLoopSound != null && fireEndSound != null && canFire)
                {
                    if (running && gunLoopSource != null && !gunLoopSource.isPlaying) { gunLoopSource.Play(); }
                    if (!running && fireLoopSound != null && gunLoopSource.isPlaying) { gunLoopSource.Stop(); gunEndSource.PlayOneShot(fireEndSound); }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void ComputeLate(float timestep)
        {
            fireTimer += timestep;
            //CLAMP RPM
            if (currentRPM <= 0f) { currentRPM = 0f; }
            //LERP ACCURACY
            currentAccuracy = Mathf.Lerp(currentAccuracy, accuracy, accuracyRecover * timestep);
            //CLAMP AMMO
            if (currentAmmo < 0) { currentAmmo = 0; }
            if (currentAmmo == 0) { canFire = false; }
            //CLAMP ROTATION
            if (currentRPM < 0) { currentRPM = 0; }
            if (currentRPM > barrelRPM) { currentRPM = barrelRPM; }

            //ROTATE BARREL
            if (barrel)
            {
                //ANTICLOCKWISE
                if (rotationDirection == RotationDirection.CCW)
                {
                    if (rotationAxis == RotationAxis.X) { barrel.Rotate(new Vector3(currentRPM * timestep, 0, 0)); }
                    if (rotationAxis == RotationAxis.Y) { barrel.Rotate(new Vector3(0, currentRPM * timestep, 0)); }
                    if (rotationAxis == RotationAxis.Z) { barrel.Rotate(new Vector3(0, 0, currentRPM * timestep)); }
                }
                //CLOCKWISE
                if (rotationDirection == RotationDirection.CW)
                {
                    if (rotationAxis == RotationAxis.X) { barrel.Rotate(new Vector3(-1f * currentRPM * timestep, 0, 0)); }
                    if (rotationAxis == RotationAxis.Y) { barrel.Rotate(new Vector3(0, -1f * currentRPM * timestep, 0)); }
                    if (rotationAxis == RotationAxis.Z) { barrel.Rotate(new Vector3(0, 0, -1f * currentRPM * timestep)); }
                }
            }
            //
            //REV GUN UP AND DOWN
            if (running) { currentRPM = Mathf.Lerp(currentRPM, barrelRPM, timestep * 0.5f); }
            else { currentRPM = Mathf.Lerp(currentRPM, 0f, timestep * 0.5f); }
        }
        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                bulletMass = 200f;
                drumWeight = ammoCapacity * ((bulletMass * 0.0648f) / 1000f);
            }
        }
    }
    #endregion

    #region Editor

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SilantroGun))]
    public class SilantroGunEditor : Editor
    {
        Color backgroundColor;
        Color silantroColor = new Color(1, 0.4f, 0);
        SilantroGun gun;
        /// <summary>
        /// 
        /// </summary>
        private void OnEnable() { gun = (SilantroGun)target; }
        /// <summary>
        /// 
        /// </summary>
        public override void OnInspectorGUI()
        {
            backgroundColor = GUI.backgroundColor;
            //DrawDefaultInspector(); 
            serializedObject.Update();


            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("System Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletType"), new GUIContent("Bullet Type"));


            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Ballistic Settings", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gunWeight"), new GUIContent("Weight"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("barrelLength"), new GUIContent("Barrel Length"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("muzzleVelocity"), new GUIContent("Muzzle Velocity"));
            GUILayout.Space(7f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("range"), new GUIContent("Maximum Range"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rateOfFire"), new GUIContent("Rate of Fire"));
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Actual Rate", gun.actualRate.ToString("0.0000"));
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Fire Timer", gun.fireTimer.ToString("0.0000"));

            GUILayout.Space(3f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Recoil Effect", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.LabelField("Recoil Force", gun.projectileForce.ToString("0.00") + " N");
            GUILayout.Space(3f);
            serializedObject.FindProperty("damperStrength").floatValue = EditorGUILayout.Slider("Damper", serializedObject.FindProperty("damperStrength").floatValue, 0f, 100f);


            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Ammo Settings", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unlimitedAmmo"), new GUIContent("Infinite Ammo"));
            if (!gun.unlimitedAmmo)
            {
                GUILayout.Space(5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoCapacity"), new GUIContent("Capacity"));
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Current Ammo", gun.currentAmmo.ToString());
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Drum Weight", gun.drumWeight.ToString() + " kg");
            }
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ejectShells"), new GUIContent("Release Shells"));
            if (gun.ejectShells)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("shellEjectPoint"), new GUIContent("Release Point"));
            }



            GUILayout.Space(10f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Accuracy Settings", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("accuracy"), new GUIContent("Accuracy"));
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("Current Accuracy", gun.currentAccuracy.ToString("0.00"));
            GUILayout.Space(2f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("advancedSettings"), new GUIContent("Advanced Settings"));

            if (gun.advancedSettings)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("accuracyDrop"), new GUIContent("Drop Per Shot"));
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("accuracyRecover"), new GUIContent("Recovery Per Shot"));
            }


            GUILayout.Space(10f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Bullet Settings", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            if (gun.bulletType == SilantroGun.BulletType.Rigidbody)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ammunition"), new GUIContent("Bullet"));
            }
            else
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("damage"), new GUIContent("Damage"));
            }
            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            SerializedProperty muzs = this.serializedObject.FindProperty("muzzles");
            GUIContent barrelLabel = new GUIContent("Barrel Count");
            EditorGUILayout.PropertyField(muzs.FindPropertyRelative("Array.size"), barrelLabel);
            GUILayout.Space(5f);
            for (int i = 0; i < muzs.arraySize; i++)
            {
                GUIContent label = new GUIContent("Barrel " + (i + 1).ToString());
                EditorGUILayout.PropertyField(muzs.GetArrayElementAtIndex(i), label);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();


            GUILayout.Space(10f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Revolver", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("barrel"), new GUIContent("Revolver"));
            GUILayout.Space(5f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationAxis"), new GUIContent("Rotation Axis"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationDirection"), new GUIContent("Rotation Direction"));

            if (gun.barrel != null)
            {
                GUILayout.Space(3f);
                EditorGUILayout.LabelField("Barrel RPM", gun.currentRPM.ToString("0.0") + " RPM");
            }



            GUILayout.Space(20f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Effects Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("muzzleFlash"), new GUIContent("Muzzle Flash"));
            if (gun.ejectShells)
            {
                GUILayout.Space(3f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bulletCase"), new GUIContent("Bullet Case"));
            }
            GUILayout.Space(5f);
            GUI.color = Color.white;
            EditorGUILayout.HelpBox("Impact Effects", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("groundHit"), new GUIContent("Ground Hit"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("metalHit"), new GUIContent("Metal Hit"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("woodHit"), new GUIContent("Wood Hit"));



            GUILayout.Space(20f);
            GUI.color = silantroColor;
            EditorGUILayout.HelpBox("Sound Configuration", MessageType.None);
            GUI.color = backgroundColor;
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fireLoopSound"), new GUIContent("Fire Loop Sound"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fireEndSound"), new GUIContent("Fire End Sound"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("soundVolume"), new GUIContent("Sound Volume"));
            GUILayout.Space(3f);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("soundRange"), new GUIContent("Sound Range"));

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    #endregion
}