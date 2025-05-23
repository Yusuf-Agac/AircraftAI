﻿using Cysharp.Threading.Tasks;
using UnityEngine;

public class BehaviorSelector : MonoBehaviour
{
    [SerializeField] private BehaviourDependencies dependencies;
    
    [SerializeReference, SubclassPicker] private AircraftBehaviour[] behaviors;
    
    private int _behaviorIndex;
    
    private void Start() => SelectBehavior(_behaviorIndex).Forget();

    internal void SelectNextBehavior()
    {
        _behaviorIndex = (_behaviorIndex + 1) % behaviors.Length;
        SelectBehavior(_behaviorIndex).Forget();
    }

    private async UniTask SelectBehavior(int index)
    {
        var previousIndex = ((_behaviorIndex - 1) >= 0 ? _behaviorIndex - 1 : _behaviorIndex + behaviors.Length) % behaviors.Length;
        
        behaviors[previousIndex].RemoveBehaviorComponent();
        await UniTask.Yield(PlayerLoopTiming.Update);
        behaviors[index].SetBehaviorComponent(transform, dependencies);
        _behaviorIndex = index;
    }
}