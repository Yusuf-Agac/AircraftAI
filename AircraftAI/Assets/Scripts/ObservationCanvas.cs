using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObservationCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text posText;
    [SerializeField] private TMP_Text rotText;
    [Space(10)]
    [SerializeField] private TMP_Text exitDirectionText;
    [SerializeField] private TMP_Text exitDistanceText;
    [Space(10)]
    [SerializeField] private TMP_Text optimalPointDistanceText;
    [SerializeField] private TMP_Text[] optimalDirectionTexts;
    [Space(10)]
    [SerializeField] private TMP_Text velocityDirText;
    [SerializeField] private TMP_Text speedText;
    [Space(10)]
    [SerializeField] private RectTransform windArrow;
    [SerializeField] private TMP_Text windSpeedText;
    [SerializeField] private TMP_Text turbulenceText;
    
    public void ChangeMode(int mode)
    {
        posText.gameObject.SetActive(mode == 0);
        rotText.gameObject.SetActive(mode == 0);
        
        exitDirectionText.gameObject.SetActive(mode == 0);
        exitDistanceText.gameObject.SetActive(mode == 0);
        
        optimalPointDistanceText.gameObject.SetActive(mode is 0 or 1);
        
        velocityDirText.gameObject.SetActive(mode is 0 or 1);
        speedText.gameObject.SetActive(mode is 0 or 1);
        
        windArrow.gameObject.SetActive(mode is 0 or 1);
        windSpeedText.gameObject.SetActive(mode is 0 or 1);
        turbulenceText.gameObject.SetActive(mode is 0 or 1);
    }
    
    public void DisplayTakeOffData(
        Vector3 relativePosition, Vector3 relativeRotation, 
        float optimalPositionDistance, Vector3[] optimalDirections,
        Vector3 velocityDir, float speed, 
        Vector3 exitDirection, float exitDistance, 
        float windAngle, float windSpeed, float turbulence)
    {
        DisplayRelativePosition(relativePosition);
        DisplayRelativeRotation(relativeRotation);
        
        DisplayExitDirection(exitDirection);
        DisplayExitDistance(exitDistance);
        
        DisplayOptimalPositionDistance(optimalPositionDistance);
        DisplayOptimalDirections(optimalDirections);
        
        DisplayVelocityDir(velocityDir);
        DisplaySpeed(speed);
        
        RotateWindArrow(windAngle);
        DisplayWindSpeed(windSpeed);
        DisplayTurbulence(turbulence);
    }
    
    public void DisplayFlightData(
        float optimalPositionDistance, Vector3[] optimalDirections,
        Vector3 velocityDir, float speed, 
        float windAngle, float windSpeed, float turbulence)
    {
        DisplayOptimalPositionDistance(optimalPositionDistance);
        DisplayOptimalDirections(optimalDirections);
        
        DisplayVelocityDir(velocityDir);
        DisplaySpeed(speed);
        
        RotateWindArrow(windAngle);
        DisplayWindSpeed(windSpeed);
        DisplayTurbulence(turbulence);
    }
    
    private void DisplayRelativePosition(Vector3 relativePosition) => posText.text = $"Pos{relativePosition}";
    private void DisplayRelativeRotation(Vector3 relativeRotation) => rotText.text = $"Rot{relativeRotation}";
    
    private void DisplayExitDirection(Vector3 exitDirection) => exitDirectionText.text = $"ExitDir{exitDirection}";
    private void DisplayExitDistance(float exitDistance) => exitDistanceText.text = $"ExitDist: {exitDistance:F2}";
    
    private void DisplayOptimalPositionDistance(float distance) => optimalPointDistanceText.text = $"Dist: {distance:F2}";
    private void DisplayOptimalDirections(Vector3[] directions)
    {
        for (var i = 0; i < directions.Length && i < optimalDirectionTexts.Length; i++)
        {
            optimalDirectionTexts[i].gameObject.SetActive(true);
            optimalDirectionTexts[i].text = $"Dir{i}{directions[i]}";
        }
        for (var i = directions.Length; i < optimalDirectionTexts.Length; i++)
        {
            optimalDirectionTexts[i].gameObject.SetActive(false);
        }
    }
    
    private void DisplayVelocityDir(Vector3 velocityDir) => velocityDirText.text = $"VelDir{velocityDir}";
    private void DisplaySpeed(float speed) => speedText.text = $"Spd: {speed:F2}";
    
    private void RotateWindArrow(float angle) => windArrow.eulerAngles = new Vector3(0, 0, angle);
    private void DisplayWindSpeed(float speed) => windSpeedText.text = $"Wind Speed: {speed:F2}";
    private void DisplayTurbulence(float turbulence) => turbulenceText.text = $"Turbulence: {turbulence:F2}";
}