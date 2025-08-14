using System.Collections.Generic;
using System.Collections.ObjectModel;
using LitJson;
using UnityEngine;

namespace BackendData.Chart.ShopDefaultGrant
{
    public class Manager : Base.Chart
    {
        private readonly Dictionary<int, Item> _dictionary = new();

        public IReadOnlyDictionary<int, Item> Dictionary => new ReadOnlyDictionary<int, Item>(_dictionary);

        public override string GetChartFileName()
        {
            return "SHOP_DEFAULT_GRANT";
        }

        protected override void LoadChartDataTemplate(JsonData json)
        {
            if (json == null || !json.IsArray)
            {
                Debug.LogWarning($"Chart '{GetChartFileName()}' data is null or not an array. No grant entries loaded.");
                return;
            }

            foreach (JsonData eachItemJson in json)
            {
                try
                {
                    Item grantItem = new Item(eachItemJson);

                    if (!_dictionary.ContainsKey(grantItem.GrantID))
                    {
                        _dictionary.Add(grantItem.GrantID, grantItem);
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate GrantID ({grantItem.GrantID}) in chart '{GetChartFileName()}'. Item from JSON: {eachItemJson.ToJson()} was not added.");
                    }
                }
                catch (System.Exception e)
                {
                    string grantIdForError = eachItemJson.Keys.Contains("GrantID") ? eachItemJson["GrantID"].ToString() : "UNKNOWN_ID";
                    Debug.LogError($"Failed to load shop grant item (GrantID: {grantIdForError}) from chart '{GetChartFileName()}'. Error: {e.Message}\nJSON: {eachItemJson.ToJson()}");
                }
            }
        }

        public Item GetShopGrantItem(int grantId)
        {
            _dictionary.TryGetValue(grantId, out Item item);
            return item;
        }

        // 특정 ProductID에 대한 모든 지급 정보(Item 리스트)를 가져오는 편의 메서드
        // (하나의 ProductID에 여러 GrantID가 매핑될 수 있는 경우를 대비)
        public List<Item> GetGrantsForProduct(string productId)
        {
            List<Item> grants = new List<Item>();
            foreach (var item in _dictionary.Values)
            {
                if (item.ProductID == productId)
                {
                    grants.Add(item);
                }
            }
            return grants;
        }
    }
}