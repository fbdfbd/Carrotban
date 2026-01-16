using LitJson;
using System;

namespace BackendData.Chart.ShopDefaultGrant
{
    public class Item
    {
        public int GrantID { get; private set; }
        public string ProductID { get; private set; }
        public RewardType RewardType { get; private set; }
        public int RewardItemID { get; private set; } 
        public int Amount { get; private set; }

        public Item(JsonData json)
        {
            GrantID = int.Parse(json["GrantID"].ToString());
            ProductID = json["ProductID"].ToString();

            string rewardTypeStr = json["RewardType"].ToString();
            if (!Enum.TryParse<RewardType>(rewardTypeStr, true, out var parsedRewardType)) // true for ignoreCase
            {
                throw new ArgumentException($"GrantID {GrantID} (Product: {ProductID}) - Invalid RewardType: {rewardTypeStr}");
            }
            this.RewardType = parsedRewardType;

            RewardItemID = int.Parse(json["RewardItemID"].ToString());
            Amount = int.Parse(json["Amount"].ToString());
        }
    }
}