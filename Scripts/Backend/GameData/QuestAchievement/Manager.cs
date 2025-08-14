using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BackEnd;
using LitJson;

namespace BackendData.GameData.QuestAchievement
{
    public class Manager : Base.GameData
    {
        private enum StorageLayout { Unknown, Nested, Flat }
        private StorageLayout _layout = StorageLayout.Unknown;

        private readonly Dictionary<int, bool> _dictionary = new();
        public IReadOnlyDictionary<int, bool> Dictionary => new ReadOnlyDictionary<int, bool>(_dictionary);

        public override string GetTableName() => "QUEST_ACHIEVEMENT";
        public override string GetColumnName() => "QUEST_ACHIEVEMENT";

        protected override void InitializeData()
        {
            _dictionary.Clear();
            foreach (var questData in GameManager.Backend.Chart.Quests.Dictionary)
                _dictionary[questData.Key] = false;
        }

        public override Param GetParam()
        {
            var saveData = new Dictionary<string, bool>(_dictionary.Count);
            foreach (var kv in _dictionary)
                saveData[kv.Key.ToString()] = kv.Value;

            var param = new Param();
            param.Add(GetColumnName(), saveData);
            return param;
        }


        public void ForceLayoutNested() => _layout = StorageLayout.Nested;
        public void ForceLayoutFlat() => _layout = StorageLayout.Flat;

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            _dictionary.Clear();

            bool hasNested = false;
            JsonData questDataJson = null;

            if (gameDataJson != null)
            {
                hasNested = gameDataJson.ContainsKey(GetColumnName())
                            && gameDataJson[GetColumnName()] != null
                            && gameDataJson[GetColumnName()].IsObject;

                if (hasNested)
                {
                    _layout = StorageLayout.Nested;
                    questDataJson = gameDataJson[GetColumnName()];
                }
                else
                {
                    bool looksFlatObject = gameDataJson.IsObject;
                    bool looksFlatByKeys = false;
                    if (looksFlatObject)
                    {
                        try
                        {
                            var keys = gameDataJson.Keys.Cast<string>();
                            looksFlatByKeys = keys.Any() && keys.All(k => int.TryParse(k, out _));
                        }
                        catch
                        {
                            looksFlatByKeys = false;
                        }
                    }

                    if (looksFlatObject && looksFlatByKeys)
                    {
                        _layout = StorageLayout.Flat;
                        questDataJson = gameDataJson;
                    }
                }
            }

            if (questDataJson != null && questDataJson.IsObject)
            {
                var chartKeys = GameManager.Backend.Chart.Quests.Dictionary.Keys.OrderBy(id => id).ToList();

                foreach (var questId in chartKeys)
                {
                    string key = questId.ToString();
                    bool isAchieve = false;

                    if (questDataJson.ContainsKey(key))
                    {
                        try
                        {
                            if (questDataJson[key].IsBoolean)
                                isAchieve = (bool)questDataJson[key];
                            else
                            {
                                string s = questDataJson[key].ToString();
                                if (bool.TryParse(s, out var b))
                                    isAchieve = b;
                                else if (int.TryParse(s, out var n))
                                    isAchieve = n != 0;
                            }
                        }
                        catch { }
                    }

                    _dictionary[questId] = isAchieve;
                }
            }
            else
            {
                _layout = StorageLayout.Unknown;
                InitializeData();
            }
        }

        public void SetAchieve(int questId)
        {
            if (_dictionary.TryGetValue(questId, out var cur))
            {
                if (!cur)
                {
                    _dictionary[questId] = true;
                    IsChangedData = true;
                }
            }
            else
            {
                _dictionary[questId] = true;
                IsChangedData = true;
            }
        }

        public int GetNextIncompleteQuest()
        {
            var chartKeys = GameManager.Backend.Chart.Quests.Dictionary.Keys.OrderBy(id => id).ToList();
            foreach (var questId in chartKeys)
            {
                if (!_dictionary.TryGetValue(questId, out var isAchieve) || !isAchieve)
                    return questId;
            }
            return -1;
        }
    }
}
