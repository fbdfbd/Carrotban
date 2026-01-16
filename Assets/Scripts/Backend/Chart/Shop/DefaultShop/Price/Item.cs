using LitJson;
using System;

namespace BackendData.Chart.ShopDefaultPrice
{
    public class Item
    {
        public int PriceID { get; private set; }
        public string ProductID { get; private set; }
        public PaymentType PaymentType { get; private set; }
        public int Price { get; private set; }
        public CurrencyType CurrencyType { get; private set; }
        public StorePlatform StorePlatform { get; private set; }
        public string StoreProductID { get; private set; }

        public Item(JsonData json)
        {
            PriceID = int.Parse(json["PriceID"].ToString());
            ProductID = json["ProductID"].ToString();

            string paymentTypeStr = json["PaymentType"].ToString();
            if (!Enum.TryParse<PaymentType>(paymentTypeStr, true, out var parsedPaymentType))
            {
                throw new ArgumentException($"PriceID {PriceID} (Product: {ProductID}) - Invalid PaymentType: {paymentTypeStr}");
            }
            this.PaymentType = parsedPaymentType;

            Price = int.Parse(json["Price"].ToString());

            string currencyTypeStr = json["CurrencyType"].ToString();
            if (!Enum.TryParse<CurrencyType>(currencyTypeStr, true, out var parsedCurrencyType))
            {
                throw new ArgumentException($"PriceID {PriceID} (Product: {ProductID}) - Invalid CurrencyType: {currencyTypeStr}");
            }
            this.CurrencyType = parsedCurrencyType;

            string storePlatformStr = json["StorePlatform"].ToString();
            if (!Enum.TryParse<StorePlatform>(storePlatformStr, true, out var parsedStorePlatform))
            {
                throw new ArgumentException($"PriceID {PriceID} (Product: {ProductID}) - Invalid StorePlatform: {storePlatformStr}");
            }
            this.StorePlatform = parsedStorePlatform;

            StoreProductID = json["StoreProductID"].ToString();
        }
    }
}