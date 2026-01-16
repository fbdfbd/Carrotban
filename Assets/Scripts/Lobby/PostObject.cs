using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class PostObject : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text contentText;
    public Button collectButton;
    public List<GameObject> rewardItems; 


    public void Bind(BackendData.Post.Item mail, Action onCollect)
    {
        titleText.text = mail.Title;
        contentText.text = mail.Content;

        foreach (var go in rewardItems)
            go.SetActive(false);

        int idx = 0;
        foreach (var kv in mail.Rewards)
        {
            if (idx >= rewardItems.Count) break; 

            var slot = rewardItems[idx++];
            slot.SetActive(true);

            // æ∆¿Ãƒ‹
            // var icon = slot.transform.GetChild(0).GetComponent<Image>();
            // icon.sprite = Resources.Load<Sprite>($"ItemIcons/{kv.Key}");

            var nameText = slot.transform.GetChild(1).GetComponent<TMP_Text>();
            nameText.text = $"{kv.Key} x{kv.Value:N0}";
        }

        collectButton.onClick.RemoveAllListeners();
        collectButton.onClick.AddListener(() => onCollect?.Invoke());
    }
}
