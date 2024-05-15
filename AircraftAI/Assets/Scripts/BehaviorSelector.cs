using System.Collections;
using Unity.MLAgents.Policies;
using UnityEngine;

public class BehaviorSelector : MonoBehaviour
{
    [SerializeReference, SubclassPicker] private BehaviorConfig[] behaviors;
    private int _behaviorIndex;

    private BehaviorParameters _behaviorParameters;
    
    private void Start()
    {
        SelectBehavior(_behaviorIndex);
    }

    internal void SelectNextBehavior()
    {
        _behaviorIndex = (_behaviorIndex + 1) % behaviors.Length;
        SelectBehavior(_behaviorIndex);
    }
    
    private void SelectBehavior(int index)
    {
        StartCoroutine(SelectBehaviorCoroutine(index));
    }
    
    private IEnumerator SelectBehaviorCoroutine(int index)
    {
        if(index != 0) behaviors[index-1].RemoveBehaviorComponent();
        yield return null;
        behaviors[index].SetBehaviorComponent(transform);
        _behaviorIndex = index;
    }
}