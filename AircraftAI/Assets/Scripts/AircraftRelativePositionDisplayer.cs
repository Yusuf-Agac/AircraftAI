using TMPro;
using UnityEngine;

public class AircraftRelativePositionDisplayer : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    
    public void DisplayRelativePosition(Vector3 relativePosition) => text.text = $"{relativePosition}";
}
