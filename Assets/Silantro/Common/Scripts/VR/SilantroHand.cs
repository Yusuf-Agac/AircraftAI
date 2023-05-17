using UnityEngine;
#if (ENABLE_INPUT_SYSTEM)
using UnityEngine.InputSystem;
#endif

#if VR_ACTIVE
using UnityEngine.XR.Interaction.Toolkit;
#endif



/// <summary>
/// 
/// </summary>
namespace Oyedoyin.Common
{
    public class SilantroHand : MonoBehaviour
    {
        public enum HandType { Right, Left }

        [Header("Connections")]
#if VR_ACTIVE
    public XRController m_controller;
    public InputHelpers.Button m_gripButton;
    public InputHelpers.Button m_triggerButton;
    public InputHelpers.Button m_thumbButton;
#endif
        public HandType m_handType = HandType.Right;
        public Animator m_animator;

        [Header("Animator Keys")]
        public string m_gripName = "Grip";
        public string m_triggerName = "Trigger";

        [Header("Data")]
        public float m_speed = 10;
        [HideInInspector] public float gripValue;
        [HideInInspector] public float triggerValue;

        private float m_currentTrigger;
        private float m_currentGrip;

        /// <summary>
        /// 
        /// </summary>
        protected void Update()
        {

#if VR_ACTIVE
        if (m_controller != null)
        {
            m_controller.inputDevice.TryReadSingleValue(m_gripButton, out gripValue);
            m_controller.inputDevice.TryReadSingleValue(m_triggerButton, out triggerValue);
        } 
#endif

            if (m_animator != null)
            { 
                //-------------------------------------------- Grip
                if (m_currentGrip != gripValue)
                {
                    m_currentGrip = Mathf.MoveTowards(m_currentGrip,
                        gripValue, Time.deltaTime * m_speed);
                    m_animator.SetFloat(m_gripName, m_currentGrip);
                }

                //-------------------------------------------- Trigger
                if (m_currentTrigger != triggerValue)
                {
                    m_currentTrigger = Mathf.MoveTowards(m_currentTrigger,
                        triggerValue, Time.deltaTime * m_speed);
                    m_animator.SetFloat(m_triggerName, m_currentTrigger);
                }
            }
        }
    }
}
