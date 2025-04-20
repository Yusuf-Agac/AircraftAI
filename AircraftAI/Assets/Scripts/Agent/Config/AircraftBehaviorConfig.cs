using System;
using Unity.MLAgents.Actuators;
using Unity.Sentis;
using UnityEngine;

[CreateAssetMenu(fileName = "BehaviorConfig", menuName = "Behavior Config")]
public class AircraftBehaviorConfig : ScriptableObject
{
    public string behaviorName;
    public int spaceSize;
    public ActionSpec actionSpecs;
    public ModelAsset model;
    [Range(1, 25)] public int decisionPeriod = 1;
    public int maxStep = 2500000;

    [Space(5)]
    public bool trainingMode;

    [Space(5)]
    public int numOfOptimalDirections = 2;
    public int gapBetweenOptimalDirections = 2;
    [Range(0.1f, 25f)] public float manoeuvreSpeed = 10f;
    
    [Space(5)]
    public AtmosphereData trainingAtmosphereData;
    public AtmosphereData evaluateAtmosphereData;
}

[Serializable]
public class BehaviourDependencies
{
    public MeshRenderer[] windArrows;
    public AudioSource windAudioSource;
    public ObservationCanvas observationCanvas;
    public RewardCanvas rewardCanvas;
}