using System.Collections.Generic;
using System.Collections.ObjectModel;
using LitJson;
using UnityEngine;

namespace BackendData.Chart.ShopDefaultPrice
{
    public class Manager : Base.Chart
    {
        private readonly Dictionary<int, Item> _dictionary = new();

        public IReadOnlyDictionary<int, Item> Dictionary => new ReadOnlyDictionary<int, Item>(_dictionary);

        public override string GetChartFileName()
        {
            return "SHOP_DEFAULT_PRICE";
        }

        protected override void LoadChartDataTemplate(JsonData json)
        {
            if (json == null || !json.IsArray)
            {
                Debug.LogWarning($"Chart '{GetChartFileName()}' data is null or not an array. No price entries loaded.");
                return;
            }

            foreach (JsonData eachItemJson in json)
            {
                try
                {
                    Item priceItem = new Item(eachItemJson);

                    if (!_dictionary.ContainsKey(priceItem.PriceID))
                    {
                        _dictionary.Add(priceItem.PriceID, priceItem);
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate PriceID ({priceItem.PriceID}) in chart '{GetChartFileName()}'. Item from JSON: {eachItemJson.ToJson()} was not added.");
                    }
                }
                catch (System.Exception e)
                {
                    string priceIdForError = eachItemJson.Keys.Contains("PriceID") ? eachItemJson["PriceID"].ToString() : "UNKNOWN_ID";
                    Debug.LogError($"Failed to load shop price item (PriceID: {priceIdForError}) from chart '{GetChartFileName()}'. Error: {e.Message}\nJSON: {eachItemJson.ToJson()}");
                }
            }
        }

        public Item GetShopPriceItem(int priceId)
        {
            _dictionary.TryGetValue(priceId, out Item item);
            return item;
        }

        // 특정 ProductID에 대한 모든 가격 정보를 가져오는 편의 메서드
        public List<Item> GetPricesForProduct(string productId)
        {
            List<Item> prices = new List<Item>();
            foreach (var item in _dictionary.Values)
            {
                if (item.ProductID == productId)
                {
                    prices.Add(item);
                }
            }
            return prices;
        }
    }
}