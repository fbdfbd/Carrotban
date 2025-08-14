using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIOptionView : MonoBehaviour
{
    [Header("�г�")]
    [SerializeField] private GameObject optionPanel;

    [Header("�⺻")]
    [SerializeField] private TMP_Text versionText;
    [SerializeField] private TMP_Text otherText;

    [Header("�Ҹ�")]
    [SerializeField] private Toggle BGMToggle;
    [SerializeField] private Toggle SFXToggle;

    [Header("��ư")]
    [SerializeField] private Button contactUsButton;
    [SerializeField] private Button backendLoginButton;
    [SerializeField] private Button googleLoginButton;
    [SerializeField] private Button exitButton;

    public event Action OnContactUsButtonClick;
    public event Action OnBackendLoginButtonClick;
    public event Action OnGoogleLoginButtonClick;
    public event Action OnExitButtonClick;

    public event Action<bool> OnBGMToggleValueChanged;
    public event Action<bool> OnSFXToggleValueChanged;

    public void Init()
    {
        BGMToggle.onValueChanged.RemoveAllListeners();
        BGMToggle.onValueChanged.AddListener(value => OnBGMToggleValueChanged?.Invoke(value));

        SFXToggle.onValueChanged.RemoveAllListeners();
        SFXToggle.onValueChanged.AddListener(value => OnSFXToggleValueChanged?.Invoke(value));

        contactUsButton.onClick.RemoveAllListeners();
        contactUsButton.onClick.AddListener(() => OnContactUsButtonClick?.Invoke());

        backendLoginButton.onClick.RemoveAllListeners();
        backendLoginButton.onClick.AddListener(() => OnBackendLoginButtonClick?.Invoke());

        googleLoginButton.onClick.RemoveAllListeners();
        googleLoginButton.onClick.AddListener(() => OnGoogleLoginButtonClick?.Invoke());

        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(() => OnExitButtonClick?.Invoke());
    }

    public void SetOptionPanel(bool active)
    {
        if (optionPanel == null) return;

        if (active)
        {
            optionPanel.SetActive(true);
            GameManager.UI.PlayFadeInAndPop(optionPanel.GetComponent<RectTransform>());
        }
        else
        {
            GameManager.UI.PlayFadeOut(optionPanel.GetComponent<RectTransform>());
        }
    }

    public void SetSoundToggles(bool bgmOn, bool sfxOn)
    {
        if (BGMToggle != null) BGMToggle.SetIsOnWithoutNotify(bgmOn);
        if (SFXToggle != null) SFXToggle.SetIsOnWithoutNotify(sfxOn);
    }
}
