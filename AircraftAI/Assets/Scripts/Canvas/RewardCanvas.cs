using TMPro;
using UnityEngine;

public class RewardCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text sparseRewardText;
    [SerializeField] private TMP_Text denseRewardText;
    
    [Space(10)]
    [SerializeField] private TMP_Text speedDifferenceRewardLabel;
    [SerializeField] private TMP_Text speedDifferenceRewardText;
    [Space(5)]
    [SerializeField] private TMP_Text groundedRewardLabel;
    [SerializeField] private TMP_Text groundedRewardText;
    [Space(10)]
    [SerializeField] private TMP_Text optimalDistanceRewardText;
    [Space(5)]
    [SerializeField] private TMP_Text actionDifferenceRewardText;
    [Space(5)]
    [SerializeField] private TMP_Text forwardVelocityDifferenceRewardText;
    [Space(5)]
    [SerializeField] private TMP_Text optimalVelocityDifferenceRewardText;
    
    public void ChangeMode(int mode)
    {
        speedDifferenceRewardLabel.gameObject.SetActive(mode == 2);
        speedDifferenceRewardText.gameObject.SetActive(mode == 2);
        groundedRewardLabel.gameObject.SetActive(mode == 2);
        groundedRewardText.gameObject.SetActive(mode == 2);
    }
    
    public void DisplayReward(float sparseReward, float denseReward, float optimalDistanceReward, float actionDifferenceReward, float forwardVelocityDifferenceReward, float optimalVelocityDifferenceReward)
    {
        DisplayGeneralRewards(sparseReward, denseReward);
        DisplayDistanceRewards(optimalDistanceReward);
        DisplayDifferenceRewards(actionDifferenceReward);
        DisplayDirectionRewards(forwardVelocityDifferenceReward, optimalVelocityDifferenceReward);
    }
    
    public void DisplayReward(float sparseReward, float denseReward, float optimalDistanceReward, float actionDifferenceReward, float forwardVelocityDifferenceReward)
    {
        DisplayGeneralRewards(sparseReward, denseReward);
        DisplayDistanceRewards(optimalDistanceReward);
        DisplayDifferenceRewards(actionDifferenceReward);
        DisplayDirectionRewards(forwardVelocityDifferenceReward);
    }
    
    public void DisplayReward(float sparseReward, float denseReward, float optimalDistanceReward, float actionDifferenceReward, float forwardVelocityDifferenceReward, float optimalVelocityDifferenceReward, float speedDifferenceReward, float groundedReward)
    {
        DisplayGeneralRewards(sparseReward, denseReward);
        DisplayDistanceRewards(optimalDistanceReward);
        DisplayDifferenceRewards(actionDifferenceReward, speedDifferenceReward);
        DisplayDirectionRewards(forwardVelocityDifferenceReward, optimalVelocityDifferenceReward);
        DisplayGroundedReward(groundedReward);
    }
    
    private void DisplayDifferenceRewards(float actionDifferenceReward, float speedDifferenceReward = 0)
    {
        actionDifferenceRewardText.text = $"{actionDifferenceReward:F2}";
        speedDifferenceRewardText.text = $"{speedDifferenceReward:F2}";
    }

    private void DisplayDirectionRewards(float forwardVelocityDifferenceReward, float optimalVelocityDifferenceReward = 0)
    {
        forwardVelocityDifferenceRewardText.text = $"{forwardVelocityDifferenceReward:F2}";
        optimalVelocityDifferenceRewardText.text = $"{optimalVelocityDifferenceReward:F2}";
    }
    
    private void DisplayDirectionRewards(float forwardVelocityDifferenceReward)
    {
        forwardVelocityDifferenceRewardText.text = $"{forwardVelocityDifferenceReward:F2}";
    }


    private void DisplayDistanceRewards(float optimalDistanceReward)
    {
        optimalDistanceRewardText.text = $"{optimalDistanceReward:F2}";
    }
    
    private void DisplayGroundedReward(float groundedReward)
    {
        groundedRewardText.text = $"{groundedReward:F2}";
    }

    private void DisplayGeneralRewards(float sparseReward, float denseReward)
    {
        sparseRewardText.text = $"{sparseReward:F2}";
        denseRewardText.text = $"{denseReward:F2}";
    }
}