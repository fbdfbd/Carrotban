using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIQuestView : MonoBehaviour
{
    [SerializeField] private GameObject questPanel;
    [SerializeField] private Button exitButton;

    [Header("����Ʈ �׸��")]
    [SerializeField] private GameObject mainQuestObject;
    [SerializeField] private TMP_Text mainQuestTitleText;
    [SerializeField] private TMP_Text mainQuestDescriptionText;
    [SerializeField] private List<GameObject> mainQuestRewardObjects; // ����: 0=Icon(Image), 1=Amount(TMP_Text)
    [SerializeField] private Button mainQuestCollectButton;
    [SerializeField] private GameObject dailyQuestScrollView;
    [SerializeField] private List<GameObject> dailyQuestObjects;

    public event Action OnMainQuestCollectButtonClick;
    public event Action OnExitButtonClick;

    public void Init()
    {
        if (mainQuestCollectButton != null)
        {
            mainQuestCollectButton.onClick.RemoveAllListeners();
            mainQuestCollectButton.onClick.AddListener(() => OnMainQuestCollectButtonClick?.Invoke());
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(() => OnExitButtonClick?.Invoke());
        }
    }

    public void SetQuestPanel(bool active)
    {
        if (questPanel == null) return;

        if (active)
        {
            questPanel.SetActive(true);
            GameManager.UI.PlayFadeInAndPop(questPanel.GetComponent<RectTransform>());
        }
        else
        {
            GameManager.UI.PlayFadeOut(questPanel.GetComponent<RectTransform>());
        }
    }

    public void SetMainQuest(QuestInfo questInfo, List<QuestRewardInfo> rewardInfos)
    {
        if (mainQuestObject != null) mainQuestObject.SetActive(true);

        if (mainQuestTitleText != null)
            mainQuestTitleText.text = questInfo?.Name ?? "����Ʈ ����";
        if (mainQuestDescriptionText != null)
            mainQuestDescriptionText.text = questInfo?.Description ?? "";

        if (mainQuestRewardObjects == null) return;

        for (int i = 0; i < mainQuestRewardObjects.Count; i++)
        {
            var slot = mainQuestRewardObjects[i];
            if (slot == null) continue;

            if (rewardInfos != null && i < rewardInfos.Count)
            {
                slot.SetActive(true);
                var r = rewardInfos[i];

                var iconImage = slot.transform.GetChild(0).GetComponent<Image>();
                var amountTxt = slot.transform.GetChild(1).GetComponent<TMP_Text>();

                if (iconImage != null)
                {
                    iconImage.sprite = Resources.Load<Sprite>(r.RewardType.ToString());
                    iconImage.enabled = iconImage.sprite != null;
                    if (iconImage.enabled) iconImage.SetNativeSize();
                }
                if (amountTxt != null)
                {
                    amountTxt.text = r.Amount.ToString();
                }
            }
            else
            {
                slot.SetActive(false);
            }
        }
    }

    public void SetMainQuestCollectInteractable(bool interactable)
    {
        if (mainQuestCollectButton != null)
            mainQuestCollectButton.interactable = interactable;
    }
}
