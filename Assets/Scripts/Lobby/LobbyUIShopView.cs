using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LobbyUIShopView : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject shopPanel;

    [Header("색인")]
    [SerializeField] private Button goldShopButton;
    [SerializeField] private Button heartShopButton;
    [SerializeField] private Button gemShopButton;

    [SerializeField] private Button exitButton;

    [Header("상품 스크롤뷰")]
    [SerializeField] private GameObject goldShopScrollView;
    [SerializeField] private GameObject heartShopScrollView;
    [SerializeField] private GameObject gemShopScrollView;

    [Header("상품버튼")]
    [SerializeField] private List<Button> goldProductsButtons;
    [SerializeField] private List<Button> heartProductsButtons;
    [SerializeField] private List<Button> gemProductsButtons;

    public event Action OnGoldShopButtonClick;
    public event Action OnHeartShopButtonClick;
    public event Action OnGemShopButtonClick;

    public event Action OnExitButtonClick;

    public event Action<int> OnGoldProductsButtonClick;
    public event Action<int> OnHeartProductsButtonClick;
    public event Action<int> OnJemProductsButtonClick;

    public void Init()
    {
        goldShopButton.onClick.RemoveAllListeners();
        goldShopButton.onClick.AddListener(() => OnGoldShopButtonClick?.Invoke());

        heartShopButton.onClick.RemoveAllListeners();
        heartShopButton.onClick.AddListener(() => OnHeartShopButtonClick?.Invoke());

        gemShopButton.onClick.RemoveAllListeners();
        gemShopButton.onClick.AddListener(() => OnGemShopButtonClick?.Invoke());

        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(() => OnExitButtonClick?.Invoke());


    }

    public void SetShopPanel(bool active)
    {
        if (shopPanel == null) return;

        if (active)
        {
            shopPanel.SetActive(true);
            GameManager.UI.PlayFadeInAndPop(shopPanel.GetComponent<RectTransform>());
        }
        else
        {
            GameManager.UI.PlayFadeOut(shopPanel.GetComponent<RectTransform>());
        }
    }



    public void SetGoldShopScrollView(bool active)
    {
        goldShopScrollView.SetActive(active);
        heartShopScrollView.SetActive(!active);
        gemShopScrollView.SetActive(!active);
    }

    public void SetHeartShopScrollView(bool active)
    {
        goldShopScrollView.SetActive(!active);
        heartShopScrollView.SetActive(active);
        gemShopScrollView.SetActive(!active);
    }

    public void SetGemShopScrollView(bool active)
    {
        goldShopScrollView.SetActive(!active);
        heartShopScrollView.SetActive(!active);
        gemShopScrollView.SetActive(active);
    }
    public void SetGoldShopProductButtons(List<ShopProductInfo> infos)
    {
        SetShopButtons(infos, goldProductsButtons, OnGoldProductsButtonClick);
    }

    public void SetHeartShopProductButtons(List<ShopProductInfo> infos)
    {
        SetShopButtons(infos, heartProductsButtons, OnHeartProductsButtonClick);
    }

    public void SetGemShopProductButtons(List<ShopProductInfo> infos)
    {
        SetShopButtons(infos, gemProductsButtons, OnJemProductsButtonClick);
    }

    private void SetShopButtons(List<ShopProductInfo> infos, List<Button> buttons, Action<int> clickAction)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (i < infos.Count)
            {
                var info = infos[i];
                var btn = buttons[i];
                btn.gameObject.SetActive(true);

                var icon = btn.transform.GetChild(0).GetComponent<Image>();
                var nameText = btn.transform.GetChild(1).GetComponent<TMP_Text>();
                var priceText = btn.transform.GetChild(2).GetComponent<TMP_Text>();

                icon.sprite = Resources.Load<Sprite>($"ShopSprites/{info.IconSpriteName}");

                nameText.text = info.Name;
                priceText.text = info.PriceText;

                int index = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => clickAction?.Invoke(index));
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }
    }
}
