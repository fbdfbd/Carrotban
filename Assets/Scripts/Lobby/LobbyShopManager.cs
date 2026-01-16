using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopProductInfo
{
    public string ProductID;
    public string Name;
    public string IconSpriteName;
    public string PriceText;
    public PaymentType PaymentType;
    public RewardType RewardType;   // 보상 타입 (GOLD/HEART/GEM)
}

public class LobbyShopManager : MonoBehaviour
{
    [SerializeField] private LobbyUIShopView lobbyUIShopView;
    public LobbyUIShopView LobbyUIShopView => lobbyUIShopView;

    private List<ShopProductInfo> _goldProducts;
    private List<ShopProductInfo> _heartProducts;
    private List<ShopProductInfo> _gemProducts;

    public void Init()
    {
        lobbyUIShopView.Init();
        RegisterUIShopViewEventHandlers();

        _goldProducts = BuildShopProductInfos(RewardType.GOLD);
        _heartProducts = BuildShopProductInfos(RewardType.HEART);
        _gemProducts = BuildShopProductInfos(RewardType.GEM);

        lobbyUIShopView.SetGoldShopProductButtons(_goldProducts);
        lobbyUIShopView.SetHeartShopProductButtons(_heartProducts);
        lobbyUIShopView.SetGemShopProductButtons(_gemProducts);
    }

    public void RegisterUIShopViewEventHandlers()
    {
        lobbyUIShopView.OnGoldShopButtonClick += OnGoldShopButtonClicked;
        lobbyUIShopView.OnHeartShopButtonClick += OnHeartShopButtonClicked;
        lobbyUIShopView.OnGemShopButtonClick += OnGemShopButtonClicked;

        lobbyUIShopView.OnGoldProductsButtonClick += OnGoldProductButtonClicked;
        lobbyUIShopView.OnHeartProductsButtonClick += OnHeartProductButtonClicked;
        lobbyUIShopView.OnJemProductsButtonClick += OnJemProductButtonClicked;

        lobbyUIShopView.OnExitButtonClick += OnShopPanelExitButtonClicked;
    }

    private void OnDestroy()
    {
        if (lobbyUIShopView == null) return;

        lobbyUIShopView.OnGoldShopButtonClick -= OnGoldShopButtonClicked;
        lobbyUIShopView.OnHeartShopButtonClick -= OnHeartShopButtonClicked;
        lobbyUIShopView.OnGemShopButtonClick -= OnGemShopButtonClicked;

        lobbyUIShopView.OnGoldProductsButtonClick -= OnGoldProductButtonClicked;
        lobbyUIShopView.OnHeartProductsButtonClick -= OnHeartProductButtonClicked;
        lobbyUIShopView.OnJemProductsButtonClick -= OnJemProductButtonClicked;

        lobbyUIShopView.OnExitButtonClick -= OnShopPanelExitButtonClicked;
    }

    public void OnGoldShopButtonClicked() => lobbyUIShopView.SetGoldShopScrollView(true);
    public void OnHeartShopButtonClicked() => lobbyUIShopView.SetHeartShopScrollView(true);
    public void OnGemShopButtonClicked() => lobbyUIShopView.SetGemShopScrollView(true);

    private void OnShopPanelExitButtonClicked()
    {
        lobbyUIShopView.SetShopPanel(false);
    }

    private void OnGoldProductButtonClicked(int index) => HandlePurchaseByIndex(_goldProducts, index);
    private void OnHeartProductButtonClicked(int index) => HandlePurchaseByIndex(_heartProducts, index);
    private void OnJemProductButtonClicked(int index) => HandlePurchaseByIndex(_gemProducts, index);

    private void HandlePurchaseByIndex(List<ShopProductInfo> list, int index)
    {
        if (list == null || index < 0 || index >= list.Count)
        {
            Debug.LogWarning($"[Shop] 잘못된 상품 인덱스: {index}");
            GameManager.UI.ShowToast("알 수 없는 상품입니다.");
            return;
        }

        var info = list[index];
        Debug.Log($"[Shop] 클릭 {info.ProductID} / {info.Name} / {info.PaymentType} / {info.PriceText}");

        if (!TryParsePrice(info.PriceText, out int price) || price <= 0)
        {
            Debug.LogWarning($"[Shop] 가격 파싱 실패: {info.PriceText}");
            GameManager.UI.ShowToast("상품 가격 정보가 올바르지 않습니다.");
            return;
        }

        if (info.RewardType == RewardType.GEM)
        {
            Debug.Log($"[Shop]: {info.ProductID} (스토어 연동 미구현)");
            GameManager.UI.ShowToast("결제 준비중입니다.");
            return;
        }
        else
        {
            var userGem = GameManager.Backend.GameData.UserGem;
            if (!userGem.TryUseGem(price))
            {
                GameManager.UI.ShowToast("보석이 부족합니다!");
                return;
            }
            GameManager.RaiseWalletChanged();

            if (!TryGrantRewardPaid(info.ProductID))
            {
                userGem.AddGem(price);
                GameManager.RaiseWalletChanged();
                GameManager.UI.ShowToast("보상 지급 실패. 결제가 취소되었습니다.");
                return;
            }

            GameManager.RaiseWalletChanged();

            GameManager.Backend.UpdateAllGameData(callback =>
            {
                if (callback == null || callback.IsSuccess())
                {
                    GameManager.RaiseWalletChanged();
                    GameManager.UI.ShowToast("구매 완료!");
                }
                else
                {
                    Debug.LogWarning($"[Shop] 저장 실패: {callback}");
                    GameManager.UI.ShowToast("저장 실패. 잠시 후 다시 시도해 주세요.");
                }
            });
        }
    }

    private bool TryGrantRewardPaid(string productId)
    {
        var grantDict = GameManager.Backend.Chart.ShopDefaultGrant.Dictionary;
        var grant = grantDict.Values.FirstOrDefault(g => g.ProductID == productId);
        if (grant == null)
        {
            Debug.LogWarning($"[Shop] 보상 정보 없음: {productId}");
            return false;
        }

        switch (grant.RewardType)
        {
            case RewardType.GOLD:
                GameManager.Backend.GameData.UserData.UpdateUserGold(grant.Amount);
                break;

            case RewardType.HEART:
                GameManager.Backend.GameData.UserHeart.AddHeart(grant.Amount); // Max 초과 허용
                break;

            case RewardType.GEM:
                GameManager.Backend.GameData.UserGem.AddGem(grant.Amount);
                break;

            default:
                Debug.LogWarning($"[Shop] 지원되지 않는 보상 타입: {grant.RewardType}");
                return false;
        }
        return true;
    }
    private List<ShopProductInfo> BuildShopProductInfos(RewardType rewardType)
    {
        var productDict = GameManager.Backend.Chart.ShopDefaultProduct.Dictionary;
        var priceDict = GameManager.Backend.Chart.ShopDefaultPrice.Dictionary;
        var grantDict = GameManager.Backend.Chart.ShopDefaultGrant.Dictionary;

        var infos = new List<ShopProductInfo>();

        foreach (var productKv in productDict)
        {
            var product = productKv.Value;

            var grantItem = grantDict.Values.FirstOrDefault(g => g.ProductID == product.ProductID);
            if (grantItem == null || grantItem.RewardType != rewardType)
                continue;

            var matchingPrices = priceDict.Values
                .Where(p => p.ProductID == product.ProductID)
                .ToList();

            BackendData.Chart.ShopDefaultPrice.Item priceItem = null;

            var forcedPaymentType = (rewardType == RewardType.GEM) ? PaymentType.CASH : PaymentType.GEM;

            priceItem = matchingPrices.FirstOrDefault(p => p.StorePlatform == StorePlatform.ANDROID)
                     ?? matchingPrices.FirstOrDefault()
                     ?? null;

            string priceText = "?";
            if (priceItem != null)
                priceText = priceItem.Price.ToString();

            infos.Add(new ShopProductInfo
            {
                ProductID = product.ProductID,
                Name = GameManager.Instance.Language == GameLanguage.ENG ? product.Name_ENG : product.Name_KR,
                IconSpriteName = product.ProductID,
                PriceText = priceText,
                PaymentType = forcedPaymentType,
                RewardType = rewardType
            });
        }

        return infos;
    }

    private static bool TryParsePrice(string text, out int price)
    {
        price = 0;
        if (string.IsNullOrEmpty(text)) return false;
        var digits = new string(text.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out price);
    }
}
