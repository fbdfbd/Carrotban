using System.Collections.Generic;
using System.Collections.ObjectModel;
using LitJson;
using UnityEngine;

namespace BackendData.Chart.QuestReward
{
    public class Manager : Base.Chart
    {
        private readonly Dictionary<int, Item> _dictionary = new();
        public IReadOnlyDictionary<int, Item> Dictionary => new ReadOnlyDictionary<int, Item>(_dictionary); // Value 타입을 Item으로 변경

        public override string GetChartFileName()
        {
            return "QUESTREWARD";
        }

        protected override void LoadChartDataTemplate(JsonData json)
        {
            if (json == null || !json.IsArray)
            {
                Debug.LogWarning($"Chart '{GetChartFileName()}' data is null or not an array. No quest rewards loaded.");
                return;
            }

            foreach (JsonData eachItemJson in json)
            {
                try
                {
                    Item questRewardItem = new Item(eachItemJson);

                    if (!_dictionary.ContainsKey(questRewardItem.RewardSetID))
                    {
                        _dictionary.Add(questRewardItem.RewardSetID, questRewardItem);
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate RewardSetID ({questRewardItem.RewardSetID}) in chart '{GetChartFileName()}'. Item from JSON: {eachItemJson.ToJson()} was not added.");
                    }
                }
                catch (System.Exception e)
                {
                    string rewardSetIdForError = eachItemJson.Keys.Contains("RewardSetID") ? eachItemJson["RewardSetID"].ToString() : "UNKNOWN_ID";
                    Debug.LogError($"Failed to load quest reward item (RewardSetID: {rewardSetIdForError}) from chart '{GetChartFileName()}'. Error: {e.Message}\nJSON: {eachItemJson.ToJson()}");
                }
            }
        }


        public Item GetQuestRewardItem(int rewardSetId)
        {
            _dictionary.TryGetValue(rewardSetId, out Item item);
            return item;
        }

        public List<Item> GetRewardsForQuest(int questId) 
        {
            List<Item> rewards = new List<Item>();
            foreach (var item in _dictionary.Values)
            {
                if (item.QuestID == questId)
                {
                    rewards.Add(item);
                }
            }
            return rewards;
        }
    }
}