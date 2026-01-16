using System;
using BackEnd;
using LitJson;

namespace BackendData.GameData
{
    public partial class UserShop
    {
        public string LastShopCheckedDate { get; private set; }
    }

    /// <summary>
    /// 1일 1상점 확인 체크를 위한 클래스
    /// </summary>
    public partial class UserShop : Base.GameData
    {
        protected override void InitializeData()
        {
            LastShopCheckedDate = "";
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            if (gameDataJson.ContainsKey("LastShopCheckedDate"))
                LastShopCheckedDate = gameDataJson["LastShopCheckedDate"].ToString();
            else
                LastShopCheckedDate = "";
        }

        public override string GetTableName() => "USER_SHOP";
        public override string GetColumnName() => null;

        public override Param GetParam()
        {
            Param param = new Param();
            param.Add("LastShopCheckedDate", LastShopCheckedDate);
            return param;
        }

        public bool CheckShop(DateTime serverDate)
        {
            string today = serverDate.ToString("yyyy-MM-dd");
            if (LastShopCheckedDate == today)
            {
                return false;
            }

            LastShopCheckedDate = today;
            IsChangedData = true;
            return true;
        }

        public bool IsShopCheckedToday(DateTime serverDate)
        {
            return LastShopCheckedDate == serverDate.ToString("yyyy-MM-dd");
        }
    }
}
