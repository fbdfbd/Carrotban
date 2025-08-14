using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPostManager : MonoBehaviour
{
    [SerializeField] private LobbyUIPostView lobbyUIPostView;

    public event Action OnPostChanged;

    public void Init()
    {
        lobbyUIPostView.Init();
        RegisterUIPostViewEventHandlers();
        LoadPostObjects();
    }


    public void RegisterUIPostViewEventHandlers()
    {
        lobbyUIPostView.OnExitButtonClick += OnPostPanelExitButtonClicked;
        lobbyUIPostView.OnPostCollectButtonClick += OnPostCollectButtonClicked;
        lobbyUIPostView.OnPostAllCollectButtonClick += OnPostAllCollectButtonClicked;
    }
    private void OnDestroy()
    {
        lobbyUIPostView.OnExitButtonClick -= OnPostPanelExitButtonClicked;
        lobbyUIPostView.OnPostCollectButtonClick -= OnPostCollectButtonClicked;
        lobbyUIPostView.OnPostAllCollectButtonClick -= OnPostAllCollectButtonClicked;
    }

    public void OnPostPanelExitButtonClicked()
    {
        lobbyUIPostView.SetPostPanel(false);
    }

    public void PostMenuButtonClicked()
    {
        lobbyUIPostView.SetPostPanel(true);
    }
    public void OnPostCollectButtonClicked(int index)
    {
        Debug.Log($"���� {index} ���� ��ư Ŭ��");
        LoadPostObjects();
        OnPostChanged?.Invoke();
    }

    public void OnPostAllCollectButtonClicked()
    {
        Debug.Log($"���� ��ü ���� ��ư Ŭ��");
    }

    public void LoadPostObjects()
    {
        var post = GameManager.Backend.Post.PostList;
        for (int i = 0; i < post.Count; i++)
        {
            lobbyUIPostView.SetPostObject(i, post[i]);
        }
    }
}
