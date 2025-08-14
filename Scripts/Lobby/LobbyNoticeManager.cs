using System.Collections.Generic;
using UnityEngine;
using BackendData.Notice;
using TMPro;
using System.Collections;
using System.Linq;

public class LobbyNoticeManager : MonoBehaviour
{
    [SerializeField] private LobbyUINoticeView lobbyUINoticeView;
    public LobbyUINoticeView LobbyUINoticeView => lobbyUINoticeView;

    private List<Item> noticeItems = new List<Item>();

    public void Init()
    {
        lobbyUINoticeView.Init();
        RegisterUINoticeViewEventHandlers();
        LoadNoticeData();
    }

    private void RegisterUINoticeViewEventHandlers()
    {
        lobbyUINoticeView.OnExitButtonClick += OnExitButtonClicked;
        lobbyUINoticeView.OnNoticeScrollPanelExitButtonClick += OnScrollPanelExitButtonClicked;
        lobbyUINoticeView.OnNoticePanelButtonClick += OnNoticePanelButtonClicked;
    }

    private void OnDestroy()
    {
        lobbyUINoticeView.OnExitButtonClick -= OnExitButtonClicked;
        lobbyUINoticeView.OnNoticeScrollPanelExitButtonClick -= OnScrollPanelExitButtonClicked;
        lobbyUINoticeView.OnNoticePanelButtonClick -= OnNoticePanelButtonClicked;
    }

    private void OnExitButtonClicked()
    {
        lobbyUINoticeView.SetNoticePanel(false);
    }

    private void OnScrollPanelExitButtonClicked()
    {
        lobbyUINoticeView.SetNoticeScrollPanel(false);
    }

    private void OnNoticePanelButtonClicked(int index)
    {
        if (index < 0 || index >= noticeItems.Count)
        {
            Debug.LogWarning($"잘못된 공지 인덱스: {index}");
            return;
        }

        var item = noticeItems[index];
        lobbyUINoticeView.SetNoticeScrollPanel(true);
        lobbyUINoticeView.SetNoticeDetail(item.Title, item.Content, item.Date);
    }

    private void LoadNoticeData()
    {
        var notices = GameManager.Backend.Notice.Dictionary;
        noticeItems = new List<Item>();
        int i = 0;

        foreach (var notice in notices)
        {
            lobbyUINoticeView.SetNoticeButton(i, true);
            lobbyUINoticeView.SetNoticeButtonTitleText(i, notice.Key);
            noticeItems.Add(notice.Value);
            i++;
        }
    }
}
