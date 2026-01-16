using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BackEnd;
using LitJson;

namespace BackendData.GameData.StageAchievement
{
    public class Manager : Base.GameData
    {
        private enum StorageLayout { Unknown, Nested, Flat }
        private StorageLayout _layout = StorageLayout.Unknown;

        private readonly Dictionary<int, bool> _dictionary = new();
        public IReadOnlyDictionary<int, bool> Dictionary => new ReadOnlyDictionary<int, bool>(_dictionary);

        public override string GetTableName() => "STAGE_ACHIEVEMENT";
        public override string GetColumnName() => "STAGE_ACHIEVEMENT";

        protected override void InitializeData()
        {
            _dictionary.Clear();
            foreach (var stageData in GameManager.Backend.Chart.Stage.Dictionary)
                _dictionary[stageData.Key] = false;
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

            JsonData stageDataJson = null;

            if (gameDataJson != null)
            {
                bool hasNested = gameDataJson.ContainsKey(GetColumnName())
                                 && gameDataJson[GetColumnName()] != null
                                 && gameDataJson[GetColumnName()].IsObject;

                if (hasNested)
                {
                    _layout = StorageLayout.Nested;
                    stageDataJson = gameDataJson[GetColumnName()];
                }
                else if (gameDataJson.IsObject)
                {
                    bool looksFlatByKeys = false;
                    try
                    {
                        var keysEnum = gameDataJson.Keys;
                        var any = false;
                        foreach (var kObj in keysEnum)
                        {
                            any = true;
                            var k = kObj as string;
                            if (k == null || !int.TryParse(k, out _))
                            {
                                looksFlatByKeys = false;
                                break;
                            }
                            looksFlatByKeys = true;
                        }
                        if (!any) looksFlatByKeys = false;
                    }
                    catch { looksFlatByKeys = false; }

                    if (looksFlatByKeys)
                    {
                        _layout = StorageLayout.Flat;
                        stageDataJson = gameDataJson;
                    }
                }
            }

            if (stageDataJson != null && stageDataJson.IsObject)
            {
                foreach (var stagePair in GameManager.Backend.Chart.Stage.Dictionary)
                {
                    int stageId = stagePair.Key;
                    string key = stageId.ToString();
                    bool isAchieve = false;

                    if (stageDataJson.ContainsKey(key))
                    {
                        try
                        {
                            if (stageDataJson[key].IsBoolean)
                                isAchieve = (bool)stageDataJson[key];
                            else
                            {
                                string s = stageDataJson[key].ToString();
                                if (bool.TryParse(s, out var b))
                                    isAchieve = b;
                                else if (int.TryParse(s, out var n))
                                    isAchieve = n != 0;
                            }
                        }
                        catch { }
                    }

                    _dictionary[stageId] = isAchieve;
                }
            }
            else
            {
                _layout = StorageLayout.Unknown;
                InitializeData();
            }
        }

        public void SetAchieve(int stageId)
        {
            if (_dictionary.TryGetValue(stageId, out var cur))
            {
                if (cur) return;
                _dictionary[stageId] = true;
            }
            else
            {
                _dictionary[stageId] = true;
            }
            IsChangedData = true;
        }

        public bool GetAchieve(int stageId)
        {
            return _dictionary.TryGetValue(stageId, out var isAchieve) && isAchieve;
        }
    }
}
