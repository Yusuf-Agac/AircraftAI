using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class AircraftRelativeTransformCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text posText;
    [SerializeField] private TMP_Text rotText;
    [SerializeField] private TMP_Text targetPointLookRotText;
    [SerializeField] private TMP_Text idealPointDistanceText;
    [SerializeField] private TMP_Text velocityDirText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text exitDirectionText;
    [SerializeField] private TMP_Text exitDistanceText;
    
    public void DisplayRelativeTransform(Vector3 relativePosition, Vector3 relativeRotation, Vector3 targetPointLookRot, float idealPointDistance, Vector3 velocityDir, float speed, Vector3 exitDirection, float exitDistance)
    {
        DisplayRelativePosition(relativePosition);
        DisplayRelativeRotation(relativeRotation);
        DisplayTargetPointLookRot(targetPointLookRot);
        DisplayIdealPointDistance(idealPointDistance);
        DisplayVelocityDir(velocityDir);
        DisplaySpeed(speed);
        DisplayExitDirection(exitDirection);
        DisplayExitDistance(exitDistance);
    }
    
    private void DisplayRelativePosition(Vector3 relativePosition) => posText.text = $"Pos{relativePosition}";
    private void DisplayRelativeRotation(Vector3 relativeRotation) => rotText.text = $"Rot{relativeRotation}";
    private void DisplayTargetPointLookRot(Vector3 targetPointLookRot) => targetPointLookRotText.text = $"Trgt{targetPointLookRot}";
    private void DisplayIdealPointDistance(float distance) => idealPointDistanceText.text = $"Dist: {distance:F2}";
    private void DisplayVelocityDir(Vector3 velocityDir) => velocityDirText.text = $"VelDir{velocityDir}";
    private void DisplaySpeed(float speed) => speedText.text = $"Spd: {speed:F2}";
    private void DisplayExitDirection(Vector3 exitDirection) => exitDirectionText.text = $"ExitDir{exitDirection}";
    private void DisplayExitDistance(float exitDistance) => exitDistanceText.text = $"ExitDist: {exitDistance:F2}";
}
