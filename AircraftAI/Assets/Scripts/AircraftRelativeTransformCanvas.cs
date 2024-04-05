using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class AircraftRelativeTransformCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text posText;
    [SerializeField] private TMP_Text rotText;
    [SerializeField] private TMP_Text idealPointDistanceText;
    [SerializeField] private TMP_Text velocityDirText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text exitDirectionText;
    [SerializeField] private TMP_Text exitDistanceText;
    
    [SerializeField] private RectTransform windArrow;
    [SerializeField] private TMP_Text windSpeedText;
    [SerializeField] private TMP_Text turbulenceText;
    
    public void DisplaySimData(Vector3 relativePosition, Vector3 relativeRotation, float idealPointDistance, Vector3 velocityDir, float speed, Vector3 exitDirection, float exitDistance, float windAngle, float windSpeed, float turbulence)
    {
        DisplayRelativePosition(relativePosition);
        DisplayRelativeRotation(relativeRotation);
        DisplayIdealPointDistance(idealPointDistance);
        DisplayVelocityDir(velocityDir);
        DisplaySpeed(speed);
        DisplayExitDirection(exitDirection);
        DisplayExitDistance(exitDistance);
        RotateWindArrow(windAngle);
        DisplayWindSpeed(windSpeed);
        DisplayTurbulence(turbulence);
    }
    
    private void DisplayRelativePosition(Vector3 relativePosition) => posText.text = $"Pos{relativePosition}";
    private void DisplayRelativeRotation(Vector3 relativeRotation) => rotText.text = $"Rot{relativeRotation}";
    private void DisplayIdealPointDistance(float distance) => idealPointDistanceText.text = $"Dist: {distance:F2}";
    private void DisplayVelocityDir(Vector3 velocityDir) => velocityDirText.text = $"VelDir{velocityDir}";
    private void DisplaySpeed(float speed) => speedText.text = $"Spd: {speed:F2}";
    private void DisplayExitDirection(Vector3 exitDirection) => exitDirectionText.text = $"ExitDir{exitDirection}";
    private void DisplayExitDistance(float exitDistance) => exitDistanceText.text = $"ExitDist: {exitDistance:F2}";
    
    private void RotateWindArrow(float angle) => windArrow.eulerAngles = new Vector3(0, 0, angle);
    private void DisplayWindSpeed(float speed) => windSpeedText.text = $"Wind Speed: {speed:F2}";
    private void DisplayTurbulence(float turbulence) => turbulenceText.text = $"Turbulence: {turbulence:F2}";
}
