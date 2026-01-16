using System.Collections.Generic;
using System.Collections.ObjectModel;
using LitJson;
using UnityEngine;

namespace BackendData.Chart.ShopDefaultProduct
{
    public class Manager : Base.Chart
    {
        private readonly Dictionary<string, Item> _dictionary = new();

        public IReadOnlyDictionary<string, Item> Dictionary => new ReadOnlyDictionary<string, Item>(_dictionary);

        public override string GetChartFileName()
        {
            return "SHOP_DEFAULT_PRODUCT";
        }

        protected override void LoadChartDataTemplate(JsonData json)
        {
            if (json == null || !json.IsArray)
            {
                Debug.LogWarning($"Chart '{GetChartFileName()}' data is null or not an array. No products loaded.");
                return;
            }

            foreach (JsonData eachItemJson in json)
            {
                try
                {
                    Item productItem = new Item(eachItemJson);

                    if (!_dictionary.ContainsKey(productItem.ProductID))
                    {
                        _dictionary.Add(productItem.ProductID, productItem);
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate ProductID ('{productItem.ProductID}') in chart '{GetChartFileName()}'. Item from JSON: {eachItemJson.ToJson()} was not added.");
                    }
                }
                catch (System.Exception e)
                {
                    string productIdForError = eachItemJson.Keys.Contains("ProductID") ? eachItemJson["ProductID"].ToString() : "UNKNOWN_ID";
                    Debug.LogError($"Failed to load product item (ProductID: '{productIdForError}') from chart '{GetChartFileName()}'. Error: {e.Message}\nJSON: {eachItemJson.ToJson()}");
                }
            }
        }

        public Item GetProductItem(string productId)
        {
            _dictionary.TryGetValue(productId, out Item item);
            return item;
        }
    }
}