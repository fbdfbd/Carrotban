using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUINoticeView : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject noticePanel;

    [Header("버튼")]
    [SerializeField] private List<Button> noticeButtons;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button noticeScrollPanelexitButton;

    [Header("공지 스크롤뷰")]
    [SerializeField] private GameObject noticeScrollView;
    [SerializeField] private TMP_Text noticeTitle;
    [SerializeField] private TMP_Text noticeContext;
    [SerializeField] private TMP_Text noticeDate;


    public event Action OnExitButtonClick;
    public event Action OnNoticeScrollPanelExitButtonClick;

    public event Action<int> OnNoticePanelButtonClick;

    public void Init()
    {
        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(() => OnExitButtonClick?.Invoke());

        noticeScrollPanelexitButton.onClick.RemoveAllListeners();
        noticeScrollPanelexitButton.onClick.AddListener(() => OnNoticeScrollPanelExitButtonClick?.Invoke());

        for (int i = 0; i < noticeButtons.Count; i++)
        {
            int index = i;
            noticeButtons[i].onClick.RemoveAllListeners();
            noticeButtons[i].onClick.AddListener(() => OnNoticePanelButtonClick?.Invoke(index));
        }
    }


    public void SetNoticePanel(bool active)
    {
        if (noticePanel == null) return;

        if (active)
        {
            noticePanel.SetActive(true);
            GameManager.UI.PlayFadeInAndPop(noticePanel.GetComponent<RectTransform>());
        }
        else
        {
            GameManager.UI.PlayFadeOut(noticePanel.GetComponent<RectTransform>());
        }
    }
    public void SetNoticeScrollPanel(bool active)
    {
        noticeScrollView.SetActive(active);
    }

    public void SetNoticeDetail(string title, string context, string date)
    {
        noticeTitle.text = title;
        noticeContext.text = context;
        noticeDate.text = date;
    }

    public void SetNoticeButton(int num, bool active)
    {
        noticeButtons[num].gameObject.SetActive(active);
    }

    public void SetNoticeButtonTitleText(int num, string title)
    {
        noticeButtons[num].GetComponentInChildren<TMP_Text>().text = title;
    }
}
