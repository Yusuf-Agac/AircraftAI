using UnityEngine;
using UnityEngine.EventSystems;



namespace Oyedoyin.Common
{
    /// <summary>
    /// 
    /// </summary>
    #region Component
    public class SilantroTouch : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public enum Mode { SelfCentering, Static }
        public enum AxisMode { Both, XOnly, YOnly }
        public enum InputMode { Normal, Inverted }

        public Mode m_mode = Mode.SelfCentering;
        public AxisMode m_axis = AxisMode.Both;
        public InputMode m_xAxisMode = InputMode.Normal;
        public InputMode m_yAxisMode = InputMode.Normal;

        [Header("Connections")]
        [SerializeField] private RectTransform m_case = null;
        [SerializeField] private RectTransform m_ball = null;

        [Header("Properties")]
        [SerializeField] private readonly float deadZone = 0;
        [SerializeField] private readonly float m_range = 1;
        private Canvas m_canvas;
        private Camera m_camera;
        public float m_centerSpeed = 1;

        [Header("Output")]
        public float m_xOutput;
        public float m_yOutput;
        private Vector2 m_input;
        public bool isPressed;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            m_canvas = GetComponentInParent<Canvas>();
            if (m_canvas == null) { Debug.LogError("The Joystick is not placed inside a canvas"); return; }

            Vector2 center = new Vector2(0.5f, 0.5f);
            m_case.pivot = center;
            m_ball.anchorMin = center;
            m_ball.anchorMax = center;
            m_ball.pivot = center;
            m_ball.anchoredPosition = Vector2.zero;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
            isPressed = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            if (m_mode == Mode.SelfCentering) { m_ball.anchoredPosition = Vector2.zero; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            m_camera = null;
            if (m_canvas.renderMode == RenderMode.ScreenSpaceCamera) { m_camera = m_canvas.worldCamera; }

            Vector2 position = RectTransformUtility.WorldToScreenPoint(m_camera, m_case.position);
            Vector2 radius = m_case.sizeDelta / 2;
            m_input = (eventData.position - position) / (radius * m_canvas.scaleFactor);

            // --------------------------------- Lock selected Axes
            if (m_axis == AxisMode.XOnly) { m_input = new Vector2(m_input.x, 0f); }
            else if (m_axis == AxisMode.YOnly) { m_input = new Vector2(0f, m_input.y); }

            if (m_input.magnitude > deadZone) { if (m_input.magnitude > 1) { m_input = m_input.normalized; } }
            else { m_input = Vector2.zero; }
            m_ball.anchoredPosition = m_input * radius * m_range;
        }
        /// <summary>
        /// 
        /// </summary>
        public void FixedUpdate()
        {
            if (!isPressed && m_mode == Mode.SelfCentering) { m_input = Vector2.MoveTowards(m_input, Vector2.zero, Time.fixedDeltaTime * m_centerSpeed); }

            // --------------------------------- Filter Inputs
            m_xOutput = m_input.x; if (m_xAxisMode == InputMode.Inverted) { m_xOutput = -m_input.x; }
            m_yOutput = m_input.y; if (m_yAxisMode == InputMode.Inverted) { m_yOutput = -m_input.y; }
        }
    }
    #endregion
}