using UnityEngine;

namespace DefaultNamespace
{
    public static class NormalizerUtility
    {
        public static Vector3 NormalizeRotation(Vector3 rotation)
        {
            return new Vector3(NormalizeAngle(rotation.x), NormalizeAngle(rotation.y), NormalizeAngle(rotation.z));
        }
        
        static float NormalizeAngle(float angle)
        {
            angle %= 360;
            angle = (angle + 360) % 360;

            return angle / 360f;
        }
    }
}