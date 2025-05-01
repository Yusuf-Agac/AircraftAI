using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(AirportNormalizer))]
public class AirportNormalizerEditor : Editor
{
    [SerializeField] private VisualTreeAsset visualTreeAsset;
    
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement visualElement = new();
        
        visualTreeAsset.CloneTree(visualElement);
        
        return visualElement;
    }
}