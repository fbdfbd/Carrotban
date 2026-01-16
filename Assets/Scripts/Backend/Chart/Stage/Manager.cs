using System.Collections.Generic;
using System.Collections.ObjectModel;
using LitJson;

namespace BackendData.Chart.Stage
{
    public class Manager : Base.Chart
    {
        private readonly Dictionary<int, Item> _dictionary = new();
        public IReadOnlyDictionary<int, Item> Dictionary => new ReadOnlyDictionary<int, Item>(_dictionary);

        public override string GetChartFileName()
        {
            return "STAGE"; 
        }

        protected override void LoadChartDataTemplate(JsonData json)
        {
            foreach (JsonData eachItem in json)
            {
                Item info = new Item(eachItem);
                if (!_dictionary.ContainsKey(info.StageID))
                {
                    _dictionary.Add(info.StageID, info);
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"중복된 StageID 발견: {info.StageID}.");
                }
            }
        }

        public Item GetStageItem(int stageId)
        {
            _dictionary.TryGetValue(stageId, out Item item);
            return item;
        }
    }
}