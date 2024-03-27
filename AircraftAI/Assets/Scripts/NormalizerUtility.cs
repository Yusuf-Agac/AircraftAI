using UnityEngine;

namespace DefaultNamespace
{
    public static class NormalizerUtility
    {
        public static Vector3 NormalizeRotation(Vector3 rotation)
        {
            return new Vector3(NormalizeAngle(rotation.x), NormalizeAngle(rotation.y), NormalizeAngle(rotation.z));
        }

        static float NormalizeAngle(float angle) => angle <= 180 ? angle / 180f : -(360 - angle) / 180;
        
        public static Vector3 DirectionToRotation(Vector3 direction)
        {
            return Quaternion.LookRotation(direction).eulerAngles;
        }
    }
}