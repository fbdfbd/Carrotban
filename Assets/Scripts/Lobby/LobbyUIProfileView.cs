using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LobbyUIProfileView : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private GameObject nicknameEditPanel;

    [Header("기본")]
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text achievedStageText;
    [SerializeField] private TMP_Text expCurText;
    [SerializeField] private TMP_Text expMaxText;


    [Header("기본 버튼")]
    [SerializeField] private Button profileExitButton;
    [SerializeField] private Button nicknameEditButton;

    [Header("닉네임 변경 패널")]
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private Button editCancleButton;
    [SerializeField] private Button editConfirmButton;

    public event Action OnProfileEditButtonClick;
    public event Action OnProfilePanelExitButtonClick;
    public event Action OnProfileEditConfirmButtonClick;
    public event Action OnProfileEditCancleButtonClick;

    public void Init()
    {
        profileExitButton.onClick.RemoveAllListeners();
        profileExitButton.onClick.AddListener(() => OnProfilePanelExitButtonClick?.Invoke());

        nicknameEditButton.onClick.RemoveAllListeners();
        nicknameEditButton.onClick.AddListener(() => OnProfileEditButtonClick?.Invoke());

        editConfirmButton.onClick.RemoveAllListeners();
        editConfirmButton.onClick.AddListener(() => OnProfileEditConfirmButtonClick?.Invoke());

        editCancleButton.onClick.RemoveAllListeners();
        editCancleButton.onClick.AddListener(() => OnProfileEditCancleButtonClick?.Invoke());
    }

    public void SetNickname(string name)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
        {
            name = "GUEST";
        }

        nicknameText.text = name;
    }

    public void SetLevel(int level)
    {
        levelText.text = level.ToString();
    }

    public void SetExp(float cur, float max)
    {
        expCurText.text = Mathf.RoundToInt(cur).ToString();
        expMaxText.text = Mathf.RoundToInt(max).ToString();
    }
    public void SetProfilePanel(bool active)
    {
        if (profilePanel == null) return;

        if (active)
        {
            profilePanel.SetActive(true);
            GameManager.UI.PlayFadeInAndPop(profilePanel.GetComponent<RectTransform>());
        }
        else
        {
            GameManager.UI.PlayFadeOut(profilePanel.GetComponent<RectTransform>());
        }
    }

    public void SetNicknameEditPanel(bool active)
    {
        nicknameEditPanel.SetActive(active);
    }

    public void SetNicknameInputFieldReset()
    {
        nicknameInputField.text = null;
    }

    public string GetNicknameInputFieldText()
    {
        return nicknameInputField.text;
    }

    public void SetAchievedStageText()
    {
        int stageKey = GameManager.Backend.GameData.UserData.CurrentStageKey;
        achievedStageText.text = $"{stageKey/100} - {stageKey%100}";
    }
}
