using UnityEngine;
using System.Threading.Tasks;
using System;

public class StageRewardManager : MonoBehaviour
{
    [SerializeField] private StageUIView stageUIView;

    public Task GrantStageClearRewardAsync(int stageKey)
    {
        var reward = GetRewardData(stageKey);
        if (reward == null)
        {
            Debug.LogError($"[보상 지급 오류] 스테이지 보상 데이터를 찾을 수 없음. StageKey: {stageKey}");
            return Task.CompletedTask;
        }

        Debug.Log($"[GrantStageClearRewardAsync] stageKey={stageKey}, reward: +Gold {reward.GoldReward}, +Exp {reward.ExpReward}");

        ApplyRewardToLocalUser(reward);
        return Task.CompletedTask;
    }

    public RewardData GetRewardData(int stageKey)
    {
        var stageDict = GameManager.Backend.Chart.Stage.Dictionary;

        if (!stageDict.TryGetValue(stageKey, out var stageItem))
        {
            Debug.LogError($"스테이지 보상 데이터를 찾을 수 없습니다: {stageKey}");
            return null;
        }

        return new RewardData
        {
            GoldReward = stageItem.GoldReward,
            ExpReward = stageItem.ExpReward
        };
    }

    private void ApplyRewardToLocalUser(RewardData reward)
    {
        var userData = GameManager.Backend.GameData.UserData;
        userData.UpdateUserGold(reward.GoldReward);
        userData.UpdateUserExp(reward.ExpReward);

        if (stageUIView != null)
            stageUIView.SetRewardItem(reward);
    }
}

public class RewardData
{
    public int GoldReward;
    public float ExpReward;
}
