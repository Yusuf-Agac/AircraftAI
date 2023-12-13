using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


#if (!SILANTRO_ROTARY)
/// <summary>
/// 
/// </summary>
namespace Oyedoyin.RotaryWing.Editors
{
    public class RotaryExtraElements
    {
        /// <summary>
        /// 
        /// </summary>
        [MenuItem("Oyedoyin/Rotary Wing/Download", false, 6100)]
        private static void DownloadRW()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/physics/silantro-helicopter-simulator-toolkit-142612");
        }
    }
}
#endif
