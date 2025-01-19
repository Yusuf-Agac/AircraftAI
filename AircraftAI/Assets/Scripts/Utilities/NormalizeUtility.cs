using UnityEngine;

public static class NormalizeUtility
{
    public static Vector3 NormalizeRotation(Vector3 rotation) => new(NormalizeAngle(rotation.x), NormalizeAngle(rotation.y), NormalizeAngle(rotation.z));

    private static float NormalizeAngle(float angle) => ClampNP1(angle <= 180 ? angle / 180f : -(360 - angle) / 180);
        
    public static Vector3 DirectionToRotation(Vector3 direction) => Quaternion.LookRotation(direction).eulerAngles;
    
    public static float ClampNP1(float value) => Mathf.Clamp(value, -1, 1);
}