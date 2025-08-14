using System;
using BackEnd;
using LitJson;

namespace BackendData.GameData
{
    public partial class UserGem : Base.GameData
    {
        private int _gem;
        public int Gem => _gem;

        public void AddGem(int amount)
        {
            if (amount <= 0) return;
            _gem = Math.Max(0, _gem + amount);
            IsChangedData = true;
        }

        public bool TryUseGem(int amount)
        {
            if (amount <= 0) return false;
            if (_gem < amount) return false;
            _gem -= amount;
            IsChangedData = true;
            return true;
        }

        protected override void InitializeData() { _gem = 0; }
        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            _gem = Math.Max(0, int.Parse(gameDataJson["Gem"].ToString()));
        }

        public override string GetTableName() => "USER_GEM";
        public override string GetColumnName() => null;

        public override Param GetParam()
        {
            Param param = new Param();
            param.Add("Gem", _gem);
            return param;
        }
    }
}
