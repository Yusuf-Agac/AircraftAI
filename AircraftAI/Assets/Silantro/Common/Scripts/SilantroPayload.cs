using UnityEngine;
using Oyedoyin.Common.Misc;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Oyedoyin.Common
{
	/// <summary>
	/// 
	/// </summary>
	#region Component
	public class SilantroPayload : MonoBehaviour
	{

		/// <summary>
		/// General class
		/// </summary>
		public enum PayloadType
		{
			Crew, Cargo, Equipment
			//ADD MORE
		}
		[HideInInspector] public PayloadType payloadType = PayloadType.Crew;

		/// <summary>
		/// Crew class
		/// </summary>
		public enum CrewType
		{
			Pilot, CoPilot, Passenger
		}
		[HideInInspector] public CrewType crewType = CrewType.Pilot;

		/// <summary>
		/// Equipment class
		/// </summary>
		public enum EquipmentType
		{
			Tyre
			//ADD MORE
		}
		[HideInInspector] public EquipmentType equipmentType = EquipmentType.Tyre;

		// Properties
		public WeightUnit m_weightUnit = WeightUnit.Kilogram;
		public float m_metricWeight;
		[HideInInspector] public float weight;

		/// <summary>
		/// 
		/// </summary>
		private void Start()
		{
			float weightFactor = 1;
			if (m_weightUnit == WeightUnit.Kilogram) { weightFactor = 1f; }
			if (m_weightUnit == WeightUnit.Pounds) { weightFactor = (1 / 2.205f); }
			m_metricWeight = weight * weightFactor;
		}
		/// <summary>
		/// 
		/// </summary>
		private void OnDrawGizmos()
		{
			//DRAW IDENTIFIER
			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(transform.position, 0.1f);
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(this.transform.position, (this.transform.up * 2f + this.transform.position));

			float weightFactor = 1;
			if (m_weightUnit == WeightUnit.Kilogram) { weightFactor = 1f; }
			if (m_weightUnit == WeightUnit.Pounds) { weightFactor = (1 / 2.205f); }
			m_metricWeight = weight * weightFactor;
		}
	}
	#endregion

	/// <summary>
	/// 
	/// </summary>
	#region Editor
#if UNITY_EDITOR
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SilantroPayload))]
	public class SilantroPayloadEditor : Editor
	{
		Color backgroundColor;
		Color silantroColor = new Color(1.0f, 0.40f, 0f);
		SilantroPayload payload;

		private SerializedProperty payloadType;
		private SerializedProperty crewType;
		private SerializedProperty weight;
		private SerializedProperty equipmentType;

		private SerializedProperty m_weightUnit;
		private SerializedProperty metricweight;

		/// <summary>
		/// 
		/// </summary>
		private void OnEnable()
		{
			payload = (SilantroPayload)target;

			payloadType = serializedObject.FindProperty("payloadType");
			crewType = serializedObject.FindProperty("crewType");
			equipmentType = serializedObject.FindProperty("equipmentType");
			weight = serializedObject.FindProperty("weight");

			m_weightUnit = serializedObject.FindProperty("m_weightUnit");
			metricweight = serializedObject.FindProperty("m_metricWeight");
		}
		/// <summary>
		/// 
		/// </summary>
		public override void OnInspectorGUI()
		{
			backgroundColor = GUI.backgroundColor;
			serializedObject.Update();

			GUILayout.Space(2f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Payload Configuration", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(payloadType);
			GUILayout.Space(3f);
			if (payload.payloadType == SilantroPayload.PayloadType.Crew)
			{
				EditorGUILayout.PropertyField(crewType);
			}
			//2. EQUIPMENTS
			if (payload.payloadType == SilantroPayload.PayloadType.Equipment)
			{
				EditorGUILayout.PropertyField(equipmentType);
			}

			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(m_weightUnit);
			GUILayout.Space(3f);
			EditorGUILayout.PropertyField(weight);
			GUILayout.Space(3f);
			EditorGUILayout.LabelField(" ", metricweight.floatValue.ToString("0.000") + " kg");
			serializedObject.ApplyModifiedProperties();
		}
	}
#endif
	#endregion
}