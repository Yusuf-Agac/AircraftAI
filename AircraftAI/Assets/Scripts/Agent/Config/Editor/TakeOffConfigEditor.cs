using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(TakeOffConfig))]
public class TakeOffConfigEditor : Editor
{
    [SerializeField] private VisualTreeAsset parentVisualTreeAsset;
    [SerializeField] private VisualTreeAsset currentVisualTreeAsset;

    
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement visualElement = new();
        
        currentVisualTreeAsset.CloneTree(visualElement);
        parentVisualTreeAsset.CloneTree(visualElement);
        
        return visualElement;
    }
}