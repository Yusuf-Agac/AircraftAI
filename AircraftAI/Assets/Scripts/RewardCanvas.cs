using TMPro;
using UnityEngine;

public class RewardCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text sparseRewardText;
    [SerializeField] private TMP_Text denseRewardText;
    [Space(10)]
    [SerializeField] private TMP_Text optimalDistanceRewardText;
    [SerializeField] private TMP_Text actionDifferenceRewardText;
    [SerializeField] private TMP_Text forwardVelocityDifferenceRewardText;
    [SerializeField] private TMP_Text optimalVelocityDifferenceRewardText;
    
    public void DisplayReward(float sparseReward, float denseReward, float optimalDistanceReward, float actionDifferenceReward, float forwardVelocityDifferenceReward, float optimalVelocityDifferenceReward)
    {
        DisplayGeneralRewards(sparseReward, denseReward);
        DisplayDistanceRewards(optimalDistanceReward);
        DisplayDifferenceRewards(actionDifferenceReward);
        DisplayDirectionRewards(forwardVelocityDifferenceReward, optimalVelocityDifferenceReward);
    }

    private void DisplayDifferenceRewards(float actionDifferenceReward)
    {
        actionDifferenceRewardText.text = $"{actionDifferenceReward:F2}";
    }

    private void DisplayDirectionRewards(float forwardVelocityDifferenceReward, float optimalVelocityDifferenceReward)
    {
        forwardVelocityDifferenceRewardText.text = $"{forwardVelocityDifferenceReward:F2}";
        optimalVelocityDifferenceRewardText.text = $"{optimalVelocityDifferenceReward:F2}";
    }

    private void DisplayDistanceRewards(float optimalDistanceReward)
    {
        optimalDistanceRewardText.text = $"{optimalDistanceReward:F2}";
    }

    private void DisplayGeneralRewards(float sparseReward, float denseReward)
    {
        sparseRewardText.text = $"{sparseReward:F2}";
        denseRewardText.text = $"{denseReward:F2}";
    }
}