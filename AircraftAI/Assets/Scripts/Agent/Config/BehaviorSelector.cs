using System.Collections;
using UnityEngine;


public class BehaviorSelector : MonoBehaviour
{
    [SerializeField] private BehaviourDependencies dependencies;
    
    [SerializeReference, SubclassPicker]
    private BehaviorConfig[] behaviors;
    
    private int _behaviorIndex;
    
    private void Start() => SelectBehavior(_behaviorIndex);

    internal void SelectNextBehavior()
    {
        _behaviorIndex = (_behaviorIndex + 1) % behaviors.Length;
        SelectBehavior(_behaviorIndex);
    }
    
    private void SelectBehavior(int index) => StartCoroutine(SelectBehaviorCoroutine(index));

    private IEnumerator SelectBehaviorCoroutine(int index)
    {
        var previousIndex = ((_behaviorIndex - 1) >= 0 ? _behaviorIndex - 1 : _behaviorIndex + behaviors.Length) % behaviors.Length;
        behaviors[previousIndex].RemoveBehaviorComponent();
        yield return null;
        behaviors[index].SetBehaviorComponent(transform, dependencies);
        _behaviorIndex = index;
    }
}