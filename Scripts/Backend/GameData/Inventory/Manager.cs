using System.Collections.Generic;
using System.Collections.ObjectModel;
using BackEnd;
using LitJson;

namespace BackendData.GameData.UserInventory
{
    public class Manager : Base.GameData
    {
        private Dictionary<int, Item> _dictionary = new();

        public IReadOnlyDictionary<int, Item> Dictionary => new ReadOnlyDictionary<int, Item>(_dictionary);

        public override string GetTableName() => "USER_INVENTORY";
        public override string GetColumnName() => "USER_INVENTORY";

        protected override void InitializeData()
        {
            _dictionary.Clear();
        }

        public override Param GetParam()
        {
            Param param = new Param();
            Dictionary<int, int> saveDict = new();
            foreach (var kv in _dictionary)
                saveDict[kv.Key] = kv.Value.Amount;
            param.Add(GetColumnName(), saveDict);
            return param;
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            _dictionary.Clear();
            if (gameDataJson.ContainsKey(GetColumnName()))
            {
                JsonData inventoryDataJson = gameDataJson[GetColumnName()];
                foreach (var chartItem in GameManager.Backend.Chart.Item.Dictionary)
                {
                    int itemId = chartItem.Key;
                    int amount = 0;
                    if (inventoryDataJson.ContainsKey(itemId.ToString()))
                        amount = int.Parse(inventoryDataJson[itemId.ToString()].ToString());
                    _dictionary[itemId] = new Item(itemId, amount);
                }
            }
            else
            {
                InitializeData();
            }
        }

        public void AddItem(int itemId, int amount)
        {
            if (amount <= 0) return;
            if (_dictionary.TryGetValue(itemId, out var item))
            {
                item.Amount += amount;
            }
            else
            {
                _dictionary[itemId] = new Item(itemId, amount);
            }
            IsChangedData = true;
        }

        public bool TryUseItem(int itemId, int amount)
        {
            if (amount <= 0) return false;
            if (_dictionary.TryGetValue(itemId, out var item) && item.Amount >= amount)
            {
                item.Amount -= amount;
                IsChangedData = true;
                return true;
            }
            return false;
        }

        public void SetItemAmount(int itemId, int amount)
        {
            if (_dictionary.ContainsKey(itemId))
                _dictionary[itemId].Amount = amount;
            else
                _dictionary[itemId] = new Item(itemId, amount);
            IsChangedData = true;
        }
    }
}
