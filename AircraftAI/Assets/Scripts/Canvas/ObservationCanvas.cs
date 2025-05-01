using TMPro;
using UnityEngine;

public class ObservationCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text behaviourNameText;
    
    [Space(10)]
    [SerializeField] private TMP_Text forwardText;
    [SerializeField] private TMP_Text upDotText;
    
    [Space(10)]
    [SerializeField] private TMP_Text upText;
    [SerializeField] private TMP_Text downDotText;
    
    [Space(10)]
    [SerializeField] private TMP_Text velocityDirText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text speedDifferenceText;
    [SerializeField] private TMP_Text thrustText;
    
    [Space(10)]
    [SerializeField] private TMP_Text optimalPointDistanceText;
    [SerializeField] private TMP_Text[] optimalDirectionTexts;
    
    [Space(10)]
    [SerializeField] private TMP_Text fwdDirDifferenceText;
    [SerializeField] private TMP_Text velDirDifferenceText;
    
    [Space(10)]
    [SerializeField] private TMP_Text dotVelRotText;
    [SerializeField] private TMP_Text dotVelOptText;
    [SerializeField] private TMP_Text dotRotOptText;
    
    [Space(10)]
    [SerializeField] private TMP_Text pitchInputText;
    [SerializeField] private TMP_Text rollInputText;
    [SerializeField] private TMP_Text yawInputText;
    [SerializeField] private TMP_Text throttleInputText;
    
    [Space(10)]
    [SerializeField] private TMP_Text pitchRateText;
    [SerializeField] private TMP_Text rollRateText;
    [SerializeField] private TMP_Text yawRateText;
    
    [Space(10)]
    [SerializeField] private TMP_Text pitchTargetText;
    [SerializeField] private TMP_Text rollTargetText;
    [SerializeField] private TMP_Text yawTargetText;
    
    [Space(10)]
    [SerializeField] private TMP_Text pitchCurrentText;
    [SerializeField] private TMP_Text rollCurrentText;
    [SerializeField] private TMP_Text yawCurrentText;
    
    [Space(10)]
    [SerializeField] private TMP_Text throttleCurrentText;
    
    [Space(10)]
    [SerializeField] private float windArrowChangeSpeed = 0.2f;
    [SerializeField] private RectTransform windArrow;
    [SerializeField] private TMP_Text windSpeedText;
    [SerializeField] private TMP_Text windSpeedNormalizedText;
    [SerializeField] private TMP_Text turbulenceText;
    [SerializeField] private TMP_Text turbulenceNormalizedText;
    
    [Space(10)]
    [SerializeField] private TMP_Text posText;
    [SerializeField] private TMP_Text rotText;
    
    [Space(10)]
    [SerializeField] private TMP_Text[] collisionDistanceTexts;
    
    [Space(10)]
    [SerializeField] private TMP_Text groundedText;
    
    public void ChangeMode(int mode)
    {
        posText.transform.parent.gameObject.SetActive(mode is 0 or 2);
        rotText.transform.parent.gameObject.SetActive(mode is 0 or 2);
        
        foreach (var collisionDistanceText in collisionDistanceTexts)
        {
            collisionDistanceText.transform.parent.gameObject.SetActive(mode is 0 or 2);
        }
        
        speedDifferenceText.transform.parent.gameObject.SetActive(mode is 2);
        throttleCurrentText.transform.parent.gameObject.SetActive(mode is 2);
    }
    
    public void DisplayNormalizedData(
        Vector3 forward, Vector3 up, float upDot, float downDot,
        Vector3 velocityDir, float speed, float thrust,
        float optimalPositionDistance, Vector3[] optimalDirections,
        Vector3 fwdDirDifference, Vector3 velDirDifference,
        float dotVelRot, float dotVelOpt, float dotRotOpt,
        float[] inputs,
        Vector3 axesTarget,
        Vector3 axesCurrent,
        Vector3 axesRate,
        float windAngle, float windSpeed, float windSpeedNormalized, float turbulence, float turbulenceNormalized,
        Vector3 relativePosition, Vector3 relativeRotation,
        float[] collisionDistances)
    {
        DisplayBehaviourName("Take Off");
        DisplayGlobalDirections(forward, up, upDot, downDot);
        DisplayMovement(velocityDir, speed, thrust);
        DisplayOptimal(optimalPositionDistance, optimalDirections);
        DisplayDifference(fwdDirDifference, velDirDifference);
        DisplayDirectionDots(dotVelRot, dotVelOpt, dotRotOpt);
        DisplayInputs(inputs);
        DisplayAxesTargets(axesTarget);
        DisplayAxesCurrents(axesCurrent);
        DisplayAxesRates(axesRate);
        DisplayWind(windAngle, windSpeed, windSpeedNormalized, turbulence, turbulenceNormalized);
        DisplayRelativeTransform(relativePosition, relativeRotation);
        DisplayCollisionDistances(collisionDistances);
    }

    public void DisplayNormalizedData(
        Vector3 forward, Vector3 up, float upDot, float downDot,
        Vector3 velocityDir, float speed, float thrust,
        float optimalPositionDistance, Vector3[] optimalDirections,
        Vector3 fwdDirDifference, Vector3 velDirDifference,
        float dotVelRot, float dotVelOpt, float dotRotOpt,
        float[] inputs,
        Vector3 axesTarget,
        Vector3 axesCurrent,
        Vector3 axesRate,
        float windAngle, float windSpeed, float windSpeedNormalized, float turbulence, float turbulenceNormalized)
    {
        DisplayBehaviourName("Flight");
        DisplayGlobalDirections(forward, up, upDot, downDot);
        DisplayMovement(velocityDir, speed, thrust);
        DisplayOptimal(optimalPositionDistance, optimalDirections);
        DisplayDifference(fwdDirDifference, velDirDifference);
        DisplayDirectionDots(dotVelRot, dotVelOpt, dotRotOpt);
        DisplayInputs(inputs);
        DisplayAxesTargets(axesTarget);
        DisplayAxesCurrents(axesCurrent);
        DisplayAxesRates(axesRate);
        DisplayWind(windAngle, windSpeed, windSpeedNormalized, turbulence, turbulenceNormalized);
    }
    
    public void DisplayNormalizedData(
        Vector3 forward, Vector3 up, float upDot, float downDot,
        Vector3 velocityDir, float speed, float thrust, float speedDifference,
        float optimalPositionDistance, Vector3[] optimalDirections,
        Vector3 fwdDirDifference, Vector3 velDirDifference,
        float dotVelRot, float dotVelOpt, float dotRotOpt,
        float[] inputs,
        Vector3 axesTarget,
        Vector3 axesCurrent,
        Vector3 axesRate,
        float throttleCurrent,
        float windAngle, float windSpeed, float windSpeedNormalized, float turbulence, float turbulenceNormalized,
        Vector3 relativePosition, Vector3 relativeRotation,
        float[] collisionDistances,
        float grounded)
    {
        DisplayBehaviourName("Landing");
        DisplayGlobalDirections(forward, up, upDot, downDot);
        DisplayMovement(velocityDir, speed, thrust, speedDifference);
        DisplayOptimal(optimalPositionDistance, optimalDirections);
        DisplayDifference(fwdDirDifference, velDirDifference);
        DisplayDirectionDots(dotVelRot, dotVelOpt, dotRotOpt);
        DisplayInputs(inputs);
        DisplayAxesTargets(axesTarget);
        DisplayAxesCurrents(axesCurrent);
        DisplayAxesRates(axesRate);
        DisplayThrottleCurrent(throttleCurrent);
        DisplayWind(windAngle, windSpeed, windSpeedNormalized, turbulence, turbulenceNormalized);
        DisplayRelativeTransform(relativePosition, relativeRotation);
        DisplayCollisionDistances(collisionDistances);
        DisplayGrounded(grounded);
    }

    private void DisplayGrounded(float grounded)
    {
        groundedText.text = $"{grounded:F2}";
    }

    private void DisplayBehaviourName(string behaviourName)
    {
        behaviourNameText.text = behaviourName + " Behaviour";
    }

    private void DisplayRelativeTransform(Vector3 relativePosition, Vector3 relativeRotation)
    {
        DisplayRelativePosition(relativePosition);
        DisplayRelativeRotation(relativeRotation);
    }

    private void DisplayWind(float windAngle, float windSpeed, float windSpeedNormalized, float turbulence, float turbulenceNormalized)
    {
        RotateWindArrow(windAngle, windSpeedNormalized);
        DisplayWindSpeed(windSpeed, windSpeedNormalized);
        DisplayTurbulence(turbulence, turbulenceNormalized);
    }
    
    private void DisplayInputs(float[] inputs)
    {
        DisplayPitchInput(inputs[0]);
        DisplayRollInput(inputs[1]);
        DisplayYawInput(inputs[2]);
        if(inputs.Length > 3) DisplayThrottleInput(inputs[3]);
    }

    private void DisplayAxesTargets(Vector3 axesTarget)
    {
        DisplayPitchTarget(axesTarget[0]);
        DisplayRollTarget(axesTarget[1]);
        DisplayYawTarget(axesTarget[2]);
    }
    
    private void DisplayAxesCurrents(Vector3 axesCurrent)
    {
        DisplayPitchCurrent(axesCurrent[0]);
        DisplayRollCurrent(axesCurrent[1]);
        DisplayYawCurrent(axesCurrent[2]);
    }
    
    private void DisplayAxesRates(Vector3 axesRate)
    {
        DisplayPitchRate(axesRate[0]);
        DisplayRollRate(axesRate[1]);
        DisplayYawRate(axesRate[2]);
    }

    private void DisplayDirectionDots(float dotVelRot, float dotVelOpt, float dotRotOpt)
    {
        DisplayDotVelRot(dotVelRot);
        DisplayDotVelOpt(dotVelOpt);
        DisplayDotRotOpt(dotRotOpt);
    }

    private void DisplayOptimal(float optimalPositionDistance, Vector3[] optimalDirections)
    {
        DisplayOptimalPositionDistance(optimalPositionDistance);
        DisplayOptimalDirections(optimalDirections);
    }
    
    private void DisplayDifference(Vector3 fwdDirDifference, Vector3 velDirDifference)
    {
        DisplayFwdDirDifference(fwdDirDifference);
        DisplayVelDirDifference(velDirDifference);
    }

    private void DisplayMovement(Vector3 velocityDir, float speed, float thrust, float speedDifference = 0f)
    {
        DisplayVelocityDir(velocityDir);
        DisplaySpeed(speed);
        DisplaySpeedDifference(speedDifference);
        DisplayThrust(thrust);
    }

    private void DisplayGlobalDirections(Vector3 forward, Vector3 up, float upDot, float downDot)
    {
        DisplayForward(forward);
        DisplayUp(up);
        DisplayUpDot(upDot);
        DisplayDownDot(downDot);
    }
    
    private void DisplayForward(Vector3 forward) => forwardText.text = $"{forward}";
    private void DisplayUp(Vector3 up) => upText.text = $"{up}";
    private void DisplayUpDot(float upDot) => upDotText.text = $"{upDot:F2}";
    private void DisplayDownDot(float downDot) => downDotText.text = $"{downDot:F2}";
    
    private void DisplayVelocityDir(Vector3 velocityDir) => velocityDirText.text = $"{velocityDir}";
    private void DisplaySpeed(float speed) => speedText.text = $"{speed:F2}";
    private void DisplaySpeedDifference(float difference) => speedDifferenceText.text = $"{difference:F2}";
    private void DisplayThrust(float thrust) => thrustText.text = $"{thrust:F2}";
    
    private void DisplayOptimalPositionDistance(float distance) => optimalPointDistanceText.text = $"{distance:F2}";
    private void DisplayOptimalDirections(Vector3[] directions)
    {
        for (var i = 0; i < directions.Length && i < optimalDirectionTexts.Length; i++)
        {
            optimalDirectionTexts[i].gameObject.SetActive(true);
            optimalDirectionTexts[i].text = $"{directions[i]}";
        }
        for (var i = directions.Length; i < optimalDirectionTexts.Length; i++)
        {
            optimalDirectionTexts[i].gameObject.SetActive(false);
        }
    }
    
    private void DisplayFwdDirDifference(Vector3 difference) => fwdDirDifferenceText.text = $"{difference}";
    private void DisplayVelDirDifference(Vector3 difference) => velDirDifferenceText.text = $"{difference}";
    
    private void DisplayDotVelRot(float dot) => dotVelRotText.text = $"{dot:F2}";
    private void DisplayDotVelOpt(float dot) => dotVelOptText.text = $"{dot:F2}";
    private void DisplayDotRotOpt(float dot) => dotRotOptText.text = $"{dot:F2}";
    
    private void DisplayPitchInput(float pitch) => pitchInputText.text = $"{pitch:F2}";
    private void DisplayRollInput(float roll) => rollInputText.text = $"{roll:F2}";
    private void DisplayYawInput(float yaw) => yawInputText.text = $"{yaw:F2}";
    private void DisplayThrottleInput(float throttle) => throttleInputText.text = $"{throttle:F2}";
    
    private void DisplayPitchRate(float pitch) => pitchRateText.text = $"{pitch:F2}";
    private void DisplayRollRate(float roll) => rollRateText.text = $"{roll:F2}";
    private void DisplayYawRate(float yaw) => yawRateText.text = $"{yaw:F2}";
    
    private void DisplayPitchTarget(float pitch) => pitchTargetText.text = $"{pitch:F2}";
    private void DisplayRollTarget(float roll) => rollTargetText.text = $"{roll:F2}";
    private void DisplayYawTarget(float yaw) => yawTargetText.text = $"{yaw:F2}";
    
    private void DisplayPitchCurrent(float pitch) => pitchCurrentText.text = $"{pitch:F2}";
    private void DisplayRollCurrent(float roll) => rollCurrentText.text = $"{roll:F2}";
    private void DisplayYawCurrent(float yaw) => yawCurrentText.text = $"{yaw:F2}";
    
    private void DisplayThrottleCurrent(float throttle) => throttleCurrentText.text = $"{throttle:F2}";
    
    private void RotateWindArrow(float angle, float normalizedSpeed)
    {
        windArrow.transform.localScale = Vector3.Lerp(windArrow.transform.localScale, Vector3.one * Mathf.Clamp(normalizedSpeed, 0.3f, 1f) * 1.5f, windArrowChangeSpeed);
        windArrow.eulerAngles = Mathf.Abs(windArrow.eulerAngles.z - (360-angle)) < 280 ?
            Vector3.Lerp(windArrow.eulerAngles, new Vector3(0, 0, 360 - angle), windArrowChangeSpeed) :
            windArrow.eulerAngles = new Vector3(0, 0, 360 - angle);
    }

    private void DisplayWindSpeed(float speed, float normalizedSpeed)
    {
        windSpeedText.text = $"Wind Speed: {speed:F2}";
        windSpeedNormalizedText.text = $"WN: {normalizedSpeed:F2}";
    }

    private void DisplayTurbulence(float turbulence, float normalizedTurbulence)
    {
        turbulenceText.text = $"Turbulence: {turbulence:F2}";
        turbulenceNormalizedText.text = $"TN: {normalizedTurbulence:F2}";
    }
    
    private void DisplayRelativePosition(Vector3 relativePosition) => posText.text = $"{relativePosition}";
    private void DisplayRelativeRotation(Vector3 relativeRotation) => rotText.text = $"{relativeRotation}";
    
    private void DisplayCollisionDistances(float[] distances)
    {
        for (var i = 0; i < distances.Length && i < collisionDistanceTexts.Length; i++)
        {
            collisionDistanceTexts[i].gameObject.SetActive(true);
            collisionDistanceTexts[i].text = $"{distances[i]:F2}";
        }
        for (var i = distances.Length; i < collisionDistanceTexts.Length; i++)
        {
            collisionDistanceTexts[i].gameObject.SetActive(false);
        }
    }
}