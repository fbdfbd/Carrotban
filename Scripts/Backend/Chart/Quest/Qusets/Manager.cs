using System.Collections.Generic;
using LitJson;
using UnityEngine;
using System.Collections.ObjectModel;

namespace BackendData.Chart.Quests
{
    public class Manager : Base.Chart
    {
        private readonly Dictionary<int, Item> _dictionary = new();

        public IReadOnlyDictionary<int, Item> Dictionary => new ReadOnlyDictionary<int, Item>(_dictionary);

        public override string GetChartFileName()
        {
            return "QUESTS";
        }

        protected override void LoadChartDataTemplate(JsonData json)
        {

            if (json == null || !json.IsArray)
            {
                Debug.LogWarning($"Chart '{GetChartFileName()}' data is null or not an array. No quests loaded.");
                return;
            }

            foreach (JsonData eachItemJson in json)
            {
                try
                {
                    Item questItem = new Item(eachItemJson);

                    if (!_dictionary.ContainsKey(questItem.QuestID))
                    {
                        _dictionary.Add(questItem.QuestID, questItem);
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate QuestID ({questItem.QuestID}) in chart '{GetChartFileName()}'. Item from JSON: {eachItemJson.ToJson()} was not added.");
                    }
                }
                catch (System.Exception e)
                {
                    string questIdForError = eachItemJson.Keys.Contains("QuestID") ? eachItemJson["QuestID"].ToString() : "UNKNOWN_ID";
                    Debug.LogError($"Failed to load quest item (QuestID: {questIdForError}) from chart '{GetChartFileName()}'. Error: {e.Message}\nJSON: {eachItemJson.ToJson()}");
                }
            }
        }

        public Item GetQuestItem(int questId)
        {
            _dictionary.TryGetValue(questId, out Item item);
            return item;
        }
    }
}