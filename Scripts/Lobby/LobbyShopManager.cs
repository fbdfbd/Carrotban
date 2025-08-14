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
    public PaymentType PaymentType; // ȭ��/Ŭ�� ������(���� ���ε� ��)
    public RewardType RewardType;   // ���� Ÿ�� (GOLD/HEART/GEM)
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
            Debug.LogWarning($"[Shop] �߸��� ��ǰ �ε���: {index}");
            GameManager.UI.ShowToast("�� �� ���� ��ǰ�Դϴ�.");
            return;
        }

        var info = list[index];
        Debug.Log($"[Shop] Ŭ�� �� {info.ProductID} / {info.Name} / {info.PaymentType} / {info.PriceText}");

        if (!TryParsePrice(info.PriceText, out int price) || price <= 0)
        {
            Debug.LogWarning($"[Shop] ���� �Ľ� ����: {info.PriceText}");
            GameManager.UI.ShowToast("��ǰ ���� ������ �ùٸ��� �ʽ��ϴ�.");
            return;
        }

        if (info.RewardType == RewardType.GEM)
        {
            Debug.Log($"[Shop] ����(IAP) ���� ��ǰ Ŭ����: {info.ProductID} (����� ���� �̱���)");
            GameManager.UI.ShowToast("���� �غ����Դϴ�.");
            return;
        }
        else
        {
            var userGem = GameManager.Backend.GameData.UserGem;
            if (!userGem.TryUseGem(price))
            {
                GameManager.UI.ShowToast("������ �����մϴ�!");
                return;
            }
            GameManager.RaiseWalletChanged();

            if (!TryGrantRewardPaid(info.ProductID))
            {
                userGem.AddGem(price);
                GameManager.RaiseWalletChanged();
                GameManager.UI.ShowToast("���� ���� ����. ������ ��ҵǾ����ϴ�.");
                return;
            }

            GameManager.RaiseWalletChanged();

            GameManager.Backend.UpdateAllGameData(callback =>
            {
                if (callback == null || callback.IsSuccess())
                {
                    // ���� ����ȭ(����)
                    GameManager.RaiseWalletChanged();
                    GameManager.UI.ShowToast("���� �Ϸ�!");
                }
                else
                {
                    Debug.LogWarning($"[Shop] ���� ����: {callback}");
                    GameManager.UI.ShowToast("���� ����. ��� �� �ٽ� �õ��� �ּ���.");
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
            Debug.LogWarning($"[Shop] ���� ���� ����: {productId}");
            return false;
        }

        switch (grant.RewardType)
        {
            case RewardType.GOLD:
                GameManager.Backend.GameData.UserData.UpdateUserGold(grant.Amount);
                break;

            case RewardType.HEART:
                GameManager.Backend.GameData.UserHeart.AddHeartPaid(grant.Amount); // Max �ʰ� ���
                break;

            case RewardType.GEM:
                GameManager.Backend.GameData.UserGem.AddGem(grant.Amount);
                break;

            default:
                Debug.LogWarning($"[Shop] �������� �ʴ� ���� Ÿ��: {grant.RewardType}");
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
