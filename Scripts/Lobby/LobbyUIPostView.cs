using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIPostView : MonoBehaviour
{
    [SerializeField] private GameObject postPanel;
    [SerializeField] private List<GameObject> postObjects;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button postAllColloectButton;

    public event Action OnExitButtonClick;
    public event Action OnPostAllCollectButtonClick;
    public event Action<int> OnPostCollectButtonClick;


    public void Init()
    {
        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(() => OnExitButtonClick?.Invoke());

        postAllColloectButton.onClick.RemoveAllListeners();
        postAllColloectButton.onClick.AddListener(() => OnPostAllCollectButtonClick?.Invoke());
    }


    public void SetPostPanel(bool active)
    {
        if (postPanel == null) return;

        if (active)
        {
            postPanel.SetActive(true);
            GameManager.UI.PlayFadeInAndPop(postPanel.GetComponent<RectTransform>());
        }
        else
        {
            GameManager.UI.PlayFadeOut(postPanel.GetComponent<RectTransform>());
        }
    }

    public void SetPostObject(int index, BackendData.Post.Item post)
    {
        if (index < 0 || index >= postObjects.Count) return;

        var slotGO = postObjects[index];
        var slot = slotGO.GetComponent<PostObject>();
        if (slot == null)
        {
            Debug.LogError($"PostObject 컴포넌트가 없습니다: {slotGO.name}");
            return;
        }

        slotGO.SetActive(true);
        slot.Bind(post, () => OnPostCollectButtonClick?.Invoke(index));
    }
}
