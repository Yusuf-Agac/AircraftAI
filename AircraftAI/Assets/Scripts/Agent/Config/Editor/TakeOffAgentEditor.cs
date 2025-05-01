using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(AircraftTakeOffAgent))]
public class TakeOffAgentEditor : Editor
{
    [SerializeField] private VisualTreeAsset visualTreeAsset;
    
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement visualElement = new();
        
        visualTreeAsset.CloneTree(visualElement);
        
        return visualElement;
    }
}