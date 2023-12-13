using System;
using UnityEngine;


namespace Oyedoyin.Common
{
    /// <summary>
    /// 
    /// </summary>
    #region Component
    [RequireComponent(typeof(CharacterController))]
    public class SilantroPlayer : MonoBehaviour
    {
        public enum Type { Default, Custom }
        public Type m_type = Type.Default;

        // Properties
        public float m_walkSpeed = 5;
        public float m_groundForce = 10;
        public float maxRayDistance = 2f;
        private Vector2 m_input;
        private Vector3 m_direction = Vector3.zero;

        // Components
        public CameraControl m_cameraControl;
        private CharacterController m_controller;
        public Camera m_camera;
        public Transform m_headPoint;

        public bool isClose = false; //Is the Player Close to an aircraft
        public bool canEnter = false;
        Controller controller;


        /// <summary>
        /// 
        /// </summary>
        [Serializable]
        public class CameraControl
        {
            public float azimuthSensitivity = 3;
            public float elevationSensitivity = 3;
            public float MinimumX = -90F;
            public float MaximumX = 90F;
            private bool m_lockCursor = true;
            private bool m_cursorIsLocked = true;

            private Quaternion m_characterTarget;
            private Quaternion m_cameraTarget;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="character"></param>
            /// <param name="camera"></param>
            public void Init(Transform character, Transform camera)
            {
                m_characterTarget = character.localRotation;
                m_cameraTarget = camera.localRotation;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="character"></param>
            /// <param name="camera"></param>
            public void LookRotation(Transform character, Transform camera)
            {
                float yRot = Input.GetAxis("Mouse X") * azimuthSensitivity;
                float xRot = Input.GetAxis("Mouse Y") * elevationSensitivity;

                m_characterTarget *= Quaternion.Euler(0f, yRot, 0f);
                m_cameraTarget *= Quaternion.Euler(-xRot, 0f, 0f);
                m_cameraTarget = ClampRotationAroundXAxis(m_cameraTarget);
                character.localRotation = m_characterTarget;
                camera.localRotation = m_cameraTarget;

                UpdateCursorLock();
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            public void SetCursorLock(bool value)
            {
                m_lockCursor = value;
                if (!m_lockCursor)
                {
                    //we force unlock the cursor if the user disable the cursor locking helper
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            public void UpdateCursorLock()
            {
                //if the user set "lockCursor" we check & properly lock the cursors
                if (m_lockCursor) { InternalLockUpdate(); }
            }
            /// <summary>
            /// 
            /// </summary>
            private void InternalLockUpdate()
            {
                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    m_cursorIsLocked = false;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    m_cursorIsLocked = true;
                }

                if (m_cursorIsLocked)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else if (!m_cursorIsLocked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="q"></param>
            /// <returns></returns>
            Quaternion ClampRotationAroundXAxis(Quaternion q)
            {
                q.x /= q.w;
                q.y /= q.w;
                q.z /= q.w;
                q.w = 1.0f;

                float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

                angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

                q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

                return q;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            if (m_headPoint == null) { m_headPoint = transform; }
            if (m_type == Type.Default)
            {
                m_controller = GetComponent<CharacterController>();
                if (m_controller == null) { return; }
                if (m_camera != null)
                {
                    m_camera.transform.parent = m_headPoint;
                    m_camera.transform.localPosition = Vector3.zero;
                    m_camera.transform.localRotation = Quaternion.identity;
                    m_cameraControl.Init(transform, m_camera.transform);
                }
            }


        }

        /// <summary>
        /// 
        /// </summary>
        protected void FixedUpdate()
        {
            if (m_type == Type.Default) { ComputeDefault(); }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void Update()
        {
            if (m_type == Type.Default) { m_cameraControl.LookRotation(transform, m_camera.transform); }

            // Send Check Data
            CheckAircraftState();

            // Enter
            if (Input.GetKeyDown(KeyCode.F)) { SendEntryData(); }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void ComputeDefault()
        {
            #region Check Camera
            if (m_camera != null)
            {
                AudioListener m_listener = m_camera.GetComponent<AudioListener>();
                if (m_listener != null && m_listener.enabled == false) { m_listener.enabled = true; }
            }
            #endregion

            #region Movement

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            m_input = new Vector2(horizontal, vertical);
            if (m_input.sqrMagnitude > 1) { m_input.Normalize(); }

            Vector3 m_commandDirection = transform.forward * m_input.y + transform.right * m_input.x;
            Physics.SphereCast(transform.position, m_controller.radius, Vector3.down, out RaycastHit hitInfo,
                               m_controller.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            m_commandDirection = Vector3.ProjectOnPlane(m_commandDirection, hitInfo.normal).normalized;

            m_direction.x = m_commandDirection.x * m_walkSpeed;
            m_direction.z = m_commandDirection.z * m_walkSpeed;

            if (m_controller.isGrounded) { m_direction.y = -m_groundForce; }
            m_controller.Move(m_direction * Time.fixedDeltaTime);

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        void OnGUI()
        {
            if (isClose && canEnter)
            {
                GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 25, 100, 100), "Press F to Enter");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void OnDrawGizmos()
        {
            if (m_headPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(m_headPoint.position, transform.forward * maxRayDistance);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void CheckAircraftState()
        {
            Vector3 direction = transform.TransformDirection(Vector3.forward);
            RaycastHit aircraft;

            if (Physics.Raycast(m_headPoint.position, direction, out aircraft, maxRayDistance))
            {
                // Collect Controller
                controller = aircraft.transform.gameObject.GetComponent<Controller>();

                // Process Controller
                if (controller != null) { if (!controller.pilotOnBoard) { isClose = true; } canEnter = true; }
                else { isClose = false; canEnter = false; }
            }

            else { isClose = false; canEnter = false; }
        }
        /// <summary>
        /// 
        /// </summary>
        public void SendEntryData()
        {
            if (isClose && canEnter)
            {
                // Player Info
                if (controller != null)
                {
                    controller.m_player = this.gameObject;
                    controller.EnterAircraft();
                }
            }
        }
    }
    #endregion
}
