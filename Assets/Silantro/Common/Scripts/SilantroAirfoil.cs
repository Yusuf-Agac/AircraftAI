using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Oyedoyin.Common
{
    #region Component
    public class SilantroAirfoil : ScriptableObject
	{

		// --------------------------SHAPE
		public string Identifier = "NACA Default";
		public List<float> x = new List<float>();
		public List<float> y = new List<float>();
		public List<float> xt = new List<float>();
		public float maximumThickness;
		public float thicknessLocation;
		public float tc, xtc;
		public float airfoilArea;

		// -------------------------DATA
		public AnimationCurve liftCurve, dragCurve, momentCurve;
		public List<float> lift;
		public List<float> drag;
		public List<float> moment;
		public List<float> LD;
		public List<float> AC;
		public List<float> CP;
		public List<float> alphas;
		public AnimationCurve centerCurve;
		public AnimationCurve pressureCurve;
		public AnimationCurve CLαCurve;


		// ------------------------LIMITS
		public float maxCd;
		public float maxClCd;
		public float stallAngle;
		public float aerodynamicCenter;
		public string ReynoldsNumber;
		public bool detailed;
		public AnimationCurve thicknessPlot;
		public float leadingEdgeRadius;

		public float upperStallAngle, lowerStallAngle;
		public float upperLiftLimit, lowerLiftLimit;
		public float liftDragRatio, zeroLiftAOA;
		public float lowerLiftPoint, upperLiftPoint;
		public float lowerAnglePoint, upperAnglePoint;
		public float centerLiftSlope;

		public enum AirfoilType { Conventional, Supercritical }
		public AirfoilType airfoilType = AirfoilType.Conventional;
		public float k = 0.87f;
		public float Mcr;


		/// <summary>
		/// 
		/// </summary>
		public void FilterPoints()
		{
			lowerStallAngle = -180f; lowerLiftLimit = 180f;
			upperStallAngle = 180f; upperLiftLimit = -180f;
			liftDragRatio = -180;


			if (lift != null && lift.Count > 1)
			{
				for (int i = 0; i < lift.Count - 1; i++)
				{
					float currentLiftLimit = lift[i]; float currentAlpha = alphas[i]; float currentDragLimit = drag[i];
					float liftDrag = currentLiftLimit / currentDragLimit;
					if (currentAlpha > -40f && currentAlpha < 40)
					{
						if (currentLiftLimit > upperLiftLimit) { upperLiftLimit = currentLiftLimit; upperStallAngle = currentAlpha; }
						if (currentLiftLimit < lowerLiftLimit) { lowerLiftLimit = currentLiftLimit; lowerStallAngle = currentAlpha; }
						if (liftDrag > liftDragRatio) { liftDragRatio = liftDrag; }
						if (currentLiftLimit > lowerLiftLimit && currentLiftLimit < 0)
						{
							lowerLiftPoint = currentLiftLimit; lowerAnglePoint = currentAlpha;
							upperLiftPoint = lift[i + 1]; upperAnglePoint = alphas[i + 1];
						}
					}
				}
				zeroLiftAOA = lowerAnglePoint + (0 - lowerLiftPoint) * (upperAnglePoint - lowerAnglePoint) / (upperLiftPoint - lowerLiftPoint);

				// ------------------------------------------ Approximate Lift Slope
				float upperSlope = Mathf.Abs(upperLiftLimit) / (Mathf.Abs(upperStallAngle) * Mathf.Deg2Rad);
				float lowerSlope = Mathf.Abs(lowerLiftLimit) / (Mathf.Abs(lowerStallAngle) * Mathf.Deg2Rad);
				centerLiftSlope = ((upperSlope) + (lowerSlope)) / 2f;
			}


			//WAVE DRAG
			if (airfoilType == AirfoilType.Conventional) { k = 0.87f; } else { k = 0.95f; }
			Mcr = (k - 0.108f) - (maximumThickness);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sweepAngle"></param>
		/// <param name="cl"></param>
		/// <returns></returns>
		public float AnalyseCriticalMachNumber(float sweepAngle, float cl)
		{
			float Mcrit;
			float Δc2 = Mathf.Deg2Rad * sweepAngle;

			float a = k - (0.108f * Mathf.Cos(Δc2));
			float b = (0.1f * cl) / (Mathf.Cos(Δc2) * Mathf.Cos(Δc2));
			float c = maximumThickness / Mathf.Cos(Δc2);

			Mcrit = a - b - c;
			return Mcrit;
		}
		/// <summary>
		/// 
		/// </summary>
		public void CheckThickness()
		{
			if (maximumThickness <= 0.15f)
			{
				thicknessPlot = new AnimationCurve();
				thicknessPlot.AddKey(new Keyframe(-0.0003f, 0f));
				thicknessPlot.AddKey(new Keyframe(0.0251f, 0.0378f));
				thicknessPlot.AddKey(new Keyframe(0.042f, 0.1097f));
				thicknessPlot.AddKey(new Keyframe(0.0553f, 0.2118f));
				thicknessPlot.AddKey(new Keyframe(0.0685f, 0.3253f));
				thicknessPlot.AddKey(new Keyframe(0.0829f, 0.469f));
				thicknessPlot.AddKey(new Keyframe(0.0942f, 0.6165f));
				thicknessPlot.AddKey(new Keyframe(0.1071f, 0.7905f));
				thicknessPlot.AddKey(new Keyframe(0.1176f, 0.9418f));
				thicknessPlot.AddKey(new Keyframe(0.128f, 1.1423f));
				thicknessPlot.AddKey(new Keyframe(0.137f, 1.2936f));
				thicknessPlot.AddKey(new Keyframe(0.15f, 1.5773f));
				leadingEdgeRadius = thicknessPlot.Evaluate(maximumThickness);
			}
			else
			{
				leadingEdgeRadius = 1.0192f * maximumThickness * maximumThickness * 100f;
			}

			// Check performance points
			FilterPoints();
		}
	}
    #endregion

    #region Editor

#if UNITY_EDITOR
    [CustomEditor(typeof(SilantroAirfoil))]
	public class SilantroAirfoilEditor : Editor
	{
		Color backgroundColor;
		Color silantroColor = new Color(1, 0.4f, 0);
		// ----------------------------------------------------------------------------------------------------------------------------------------------------------
		public override void OnInspectorGUI()
		{
			backgroundColor = GUI.backgroundColor;
			serializedObject.Update();
			SilantroAirfoil foil = (SilantroAirfoil)target;

			//DrawDefaultInspector();
			foil.CheckThickness();
			GUILayout.Space(3f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Properties", MessageType.None);
			GUI.color = backgroundColor;
			//Write Airfoil Name
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Label", foil.Identifier);
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Type", foil.airfoilType.ToString());


			//DIMENSIONS
			GUILayout.Space(8f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Geometry", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(5f);
			EditorGUILayout.LabelField("Maximum Thickness", foil.tc.ToString("0.00") + " %c");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("LE Radius (Approx)", foil.leadingEdgeRadius.ToString("0.00") + " %c");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Thickness Location", foil.xtc.ToString("0") + " %c");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Surface Area", foil.airfoilArea.ToString("0.0000") + " /m2");
			GUILayout.Space(3f);
			EditorGUILayout.LabelField("Aerodynamic Center", foil.aerodynamicCenter.ToString("0.00") + " %c");
			//PERFORMANCE
			GUILayout.Space(8f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Curve Data", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(5f);
			EditorGUILayout.CurveField("Lift Curve", foil.liftCurve);
			GUILayout.Space(2f);
			EditorGUILayout.CurveField("Drag Curve", foil.dragCurve);
			GUILayout.Space(2f);
			EditorGUILayout.CurveField("Moment Curve", foil.momentCurve);


			//LIMITS
			GUILayout.Space(10f);
			GUI.color = silantroColor;
			EditorGUILayout.HelpBox("Performance", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Stall Data", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Upper Limit", foil.upperStallAngle.ToString("0.0 °"));
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Lower Limit", foil.lowerStallAngle.ToString("0.0 °"));
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Zero Lift α", foil.zeroLiftAOA.ToString("0.0 °"));
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Clα (α=0)", foil.centerLiftSlope.ToString("0.000"));

			GUILayout.Space(5f);
			GUI.color = Color.white;
			EditorGUILayout.HelpBox("Flow Data", MessageType.None);
			GUI.color = backgroundColor;
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Maximum Cl", foil.upperLiftLimit.ToString("0.000"));
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Minimum Cl", foil.lowerLiftLimit.ToString("0.000"));
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Maximum Cd", foil.maxCd.ToString("0.000"));
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Max Cl/Cd", foil.liftDragRatio.ToString("0.00"));
			GUILayout.Space(2f);
			EditorGUILayout.LabelField("Mcrit (cl=0)", foil.Mcr.ToString("0.000"));
		}

	}
#endif

	#endregion
}
