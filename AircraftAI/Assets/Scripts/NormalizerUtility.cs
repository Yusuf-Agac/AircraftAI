using UnityEngine;

namespace DefaultNamespace
{
    public static class NormalizerUtility
    {
        public static Vector3 NormalizeRotation(Vector3 rotation)
        {
            return new Vector3(NormalizeAngle(rotation.x), NormalizeAngle(rotation.y), NormalizeAngle(rotation.z));
        }
        
        private static float NormalizeAngle(float angle)
        {
            Debug.LogError("!!Not Implemented Yet!!");
            return ((angle % 360) + 180f) / 360f;
        }
    }
}