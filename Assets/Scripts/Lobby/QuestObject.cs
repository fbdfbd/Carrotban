using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestObject : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Button collectButton;
    public List<GameObject> rewardItems;

    
    public void Bind(BackendData.Chart.Quests.Item quest, BackendData.Chart.QuestReward.Item questrewards, Action onCollect)
    {
        if (GameManager.Instance.Language == GameLanguage.KOR)
        {
            titleText.text = quest.Name_KR;
            descriptionText.text = quest.Des_KR;
        }
        else
        {
            titleText.text = quest.Name_ENG;
            descriptionText.text = quest.Des_ENG;
        }

        collectButton.onClick.RemoveAllListeners();
        collectButton.onClick.AddListener(() => onCollect?.Invoke());
    }
}
