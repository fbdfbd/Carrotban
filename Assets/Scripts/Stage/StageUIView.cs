using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;                  

public class StageUIView : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Player player;

    [Header("UI Elements")]
    [SerializeField] private Image seedIcon;
    [SerializeField] private TMP_Text seedStatusText;
    [SerializeField] private TMP_Text stageNameText;
    [SerializeField] private GameObject stageClearPanel;
    [SerializeField] private GameObject stagePausePanel;
    [SerializeField] private Button stageOutAllowButton;
    [SerializeField] private Button stageClearExitButton;
    [SerializeField] private Button stagePauseButton;

    [Header("슬라이더")]
    [SerializeField] private TMP_Text expAmountText;
    [SerializeField] private TMP_Text expGetAmountText;
    [SerializeField] private Slider expBar;
    [SerializeField] private List<GameObject> rewardItemList;

    public event Action OnStagePauseButtonClick;
    public event Action OnStageOutButtonClick;
    public event Action OnStageClearExitClick; 

    private void Awake()
    {
        stageOutAllowButton?.onClick.AddListener(
            () => OnStageOutButtonClick?.Invoke());

        stageClearExitButton?.onClick.AddListener(
            () => OnStageClearExitClick?.Invoke());

        stagePauseButton?.onClick.AddListener(
            () => OnStagePauseButtonClick?.Invoke());
    }

    private void Start()
    {
        if (player == null) player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.OnHasSeedChanged += UpdateSeedUI;
            UpdateSeedUI(player.HasSeed);
        }
        else
            Debug.LogError("Player not found for StageUIView!");
    }

    private void OnDestroy()
    {
        if (player != null)
            player.OnHasSeedChanged -= UpdateSeedUI;
    }

    private void UpdateSeedUI(bool hasSeed) =>
        seedStatusText.text = hasSeed ? "1/1" : "0/1";

    public void SetStageName(string name) => stageNameText.text = name;
    public void SetStageClearPanel(bool active) 
    { 
        stageClearPanel.SetActive(active);

        if (stageClearPanel == null) return;

        if (active)
        {
            stageClearPanel.SetActive(true);
            GameManager.UI.PlayFadeInAndPop(stageClearPanel.GetComponent<RectTransform>());
        }
        else
        {
            //근데 얘는 비활성화가 안되는데
            stageClearPanel.SetActive(false);
        }
    }

    public void SetExpAmountText(int current, int max)
    {
        expAmountText.text = $"{current} / {max}";
    }

    public void SetExpGetAmountText(int exp)
    {
        expGetAmountText.text = $"+ {exp}";
    }

    public void SetExpBar(float prevExp, float addedExp, float prevMaxExp, float newMaxExp)
    {
        GameManager.UI.GetAnimationManager().AnimateExpBarByExpGain(
            expBar, prevExp, addedExp, prevMaxExp, newMaxExp
        );
    }

    public void SetStagePausePanel(bool active)
    {
        if (stagePausePanel == null) return;

        if (active)
        {
            stagePausePanel.SetActive(true);
        }
        else
        {
            stagePausePanel.SetActive(false);
        }
    }


    public void SetRewardItem(RewardData reward)
    {
        string[] iconNames = { "GOLD", "EXP" };
        float[] rewardAmounts = { reward.GoldReward, reward.ExpReward };

        for (int i = 0; i < rewardItemList.Count; i++)
        {
            var item = rewardItemList[i];
            if (i < rewardAmounts.Length)
            {
                var rewardObject = item.GetComponent<RewardItem>();
                var icon = Resources.Load<Sprite>(iconNames[i]);

                if (icon != null)
                {
                    rewardObject.itemIconImage.sprite = icon;
                }

                rewardObject.itemAmountText.text = rewardAmounts[i].ToString();
                item.SetActive(true);
            }
            else
            {
                item.SetActive(false);
            }
        }

    }

}
