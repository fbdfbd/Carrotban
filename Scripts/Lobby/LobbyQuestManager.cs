using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestInfo
{
    public string QuestID;
    public string Name;
    public string Description;
    public QuestType QuestType;
    public int PreqQuestID;
    public int PreqClearStageID;

    public bool IsCleared;   // 해당 퀘스트 대상 스테이지(=QuestID) 클리어 여부
    public bool IsAchieved;  // 보상 수령 완료 여부
    public bool CanCollect;  // = IsCleared && !IsAchieved
}

public class QuestRewardInfo
{
    public int RewardSetID;
    public int QuestID;
    public RewardType RewardType;
    public int ItemID;
    public int Amount;
}

public class LobbyQuestManager : MonoBehaviour
{
    [SerializeField] private LobbyUIQuestView lobbyUIQuestView;

    private QuestInfo currentMainQuestInfo;
    private List<QuestRewardInfo> currentMainQuestRewards;

    public event Action<bool> OnQuestRedDotChanged;

    public void Init()
    {
        RegisterUIQuestViewEventHandlers();
        SetMainQuestInfos();
        lobbyUIQuestView.Init();
        RaiseQuestRedDotChanged();
    }

    public void RegisterUIQuestViewEventHandlers()
    {
        lobbyUIQuestView.OnMainQuestCollectButtonClick += OnMainQuestCollectButtonClicked;
        lobbyUIQuestView.OnExitButtonClick += OnQuestPanelExitButtonClicked;
    }

    private void OnDestroy()
    {
        if (lobbyUIQuestView == null) return;
        lobbyUIQuestView.OnMainQuestCollectButtonClick -= OnMainQuestCollectButtonClicked;
        lobbyUIQuestView.OnExitButtonClick -= OnQuestPanelExitButtonClicked;
    }

    public void QuestMenuButtonClicked()
    {
        lobbyUIQuestView.SetQuestPanel(true);
        SetMainQuestInfos();
        RaiseQuestRedDotChanged();
    }

    private void OnQuestPanelExitButtonClicked()
    {
        lobbyUIQuestView.SetQuestPanel(false);
    }

    private void SetMainQuestInfos()
    {
        int questId = GameManager.Backend.GameData.QuestAchievement.GetNextIncompleteQuest();
        if (questId == -1) return;

        var q = GameManager.Backend.Chart.Quests.GetQuestItem(questId);
        if (q == null) return;

        bool isCleared = CheckQuestCleared(questId);  // StageAchievement.GetAchieve(questId)
        bool isAchieved = IsQuestAchieved(questId);    // QuestAchievement true??..
        bool canCollect = isCleared && !isAchieved;

        currentMainQuestInfo = new QuestInfo
        {
            QuestID = q.QuestID.ToString(),
            Name = q.Name_KR,
            Description = q.Des_KR,
            QuestType = q.QuestType,
            PreqQuestID = q.PreqQuestID,
            PreqClearStageID = q.PreqClearStageID,
            IsCleared = isCleared,
            IsAchieved = isAchieved,
            CanCollect = canCollect
        };

        var rewardChartItems = GameManager.Backend.Chart.QuestReward.GetRewardsForQuest(questId);
        currentMainQuestRewards = new List<QuestRewardInfo>();
        foreach (var r in rewardChartItems)
        {
            currentMainQuestRewards.Add(new QuestRewardInfo
            {
                RewardSetID = r.RewardSetID,
                QuestID = r.QuestID,
                RewardType = r.RewardType,
                ItemID = r.ItemID,
                Amount = r.Amount
            });
        }

        lobbyUIQuestView.SetMainQuest(currentMainQuestInfo, currentMainQuestRewards);
        lobbyUIQuestView.SetMainQuestCollectInteractable(currentMainQuestInfo.CanCollect);
    }

    private bool CheckQuestCleared(int questId)
    {
        return GameManager.Backend.GameData.StageAchievement.GetAchieve(questId);
    }

    private bool IsQuestAchieved(int questId)
    {
        var qa = GameManager.Backend.GameData.QuestAchievement;
        return qa.Dictionary.TryGetValue(questId, out var achievedBool) && achievedBool; // bool 사양
    }

    private void OnMainQuestCollectButtonClicked()
    {
        if (currentMainQuestInfo == null) return;

        int questId = int.Parse(currentMainQuestInfo.QuestID);

        bool cleared = CheckQuestCleared(questId);
        bool achieved = IsQuestAchieved(questId);
        if (!cleared || achieved) return;

        GameManager.Backend.GameData.QuestAchievement.SetAchieve(questId);
        GrantMainQuestRewards();
        SetMainQuestInfos();
        GameManager.RaiseWalletChanged();
        RaiseQuestRedDotChanged();
    }
    private bool ComputeQuestRedDot()
    {
        if (currentMainQuestInfo != null)
            return currentMainQuestInfo.CanCollect;

        int nextId = GameManager.Backend.GameData.QuestAchievement.GetNextIncompleteQuest();
        if (nextId == -1) return false;
        bool cleared = CheckQuestCleared(nextId);
        bool achieved = IsQuestAchieved(nextId);
        return cleared && !achieved;
    }

    private void RaiseQuestRedDotChanged()
    {
        OnQuestRedDotChanged?.Invoke(ComputeQuestRedDot());
    }

    private void GrantMainQuestRewards()
    {
        if (currentMainQuestRewards == null || currentMainQuestRewards.Count == 0) return;

        int totalExp = 0;
        int totalGold = 0;
        int totalGem = 0;

        foreach (var reward in currentMainQuestRewards)
        {
            switch (reward.RewardType)
            {
                case RewardType.EXP: totalExp += reward.Amount; break;
                case RewardType.GOLD: totalGold += reward.Amount; break;
                case RewardType.GEM: totalGem += reward.Amount; break;
            }
        }

        if (totalExp > 0) GameManager.Backend.GameData.UserData.UpdateUserExp(totalExp);
        if (totalGold > 0) GameManager.Backend.GameData.UserData.UpdateUserGold(totalGold);
        if (totalGem > 0) GameManager.Backend.GameData.UserGem.AddGem(totalGem);

        GameManager.Backend.UpdateAllGameData(null);
    }
}
