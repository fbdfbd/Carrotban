using System.Collections.Generic;
using System.Collections.ObjectModel;
using LitJson;
using Unity.VisualScripting;
using UnityEngine;

namespace BackendData.Chart.Item
{
    public class Manager : Base.Chart
    {
        private readonly Dictionary<int, Item> _dictionary = new();

        public IReadOnlyDictionary<int, Item> Dictionary => new ReadOnlyDictionary<int, Item>(_dictionary);

        public override string GetChartFileName()
        {
            return "ITEM";
        }

        protected override void LoadChartDataTemplate(JsonData json)
        {
            _dictionary.Clear();

            if (json == null || !json.IsArray)
            {
                Debug.LogWarning($"Chart '{GetChartFileName()}' data is null or not an array. No items loaded.");
                return;
            }

            foreach (JsonData eachItemJson in json)
            {
                try
                {
                    Item gameItem = new Item(eachItemJson);

                    if (!_dictionary.ContainsKey(gameItem.ItemID))
                    {
                        _dictionary.Add(gameItem.ItemID, gameItem);
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate ItemID ({gameItem.ItemID}) in chart '{GetChartFileName()}'. Item from JSON: {eachItemJson.ToJson()} was not added.");
                    }
                }
                catch (System.Exception e)
                {
                    string itemIdForError = eachItemJson.Keys.Contains("ItemID") ? eachItemJson["ItemID"].ToString() : "UNKNOWN_ID";
                    Debug.LogError($"Failed to load game item (ItemID: {itemIdForError}) from chart '{GetChartFileName()}'. Error: {e.Message}\nJSON: {eachItemJson.ToJson()}");
                }
            }
        }

        public Item GetGameItem(int itemId)
        {
            _dictionary.TryGetValue(itemId, out Item item);
            return item;
        }
    }
}