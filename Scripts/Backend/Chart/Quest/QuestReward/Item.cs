using LitJson;
using System;

namespace BackendData.Chart.QuestReward
{
    public class Item
    {
        public int RewardSetID { get; private set; }
        public int QuestID { get; private set; }
        public RewardType RewardType { get; private set; }
        public int ItemID { get; private set; } 
        public int Amount { get; private set; }

        public Item(JsonData json)
        {
            RewardSetID = int.Parse(json["RewardSetID"].ToString());
            QuestID = int.Parse(json["QuestID"].ToString());

            string rewardTypeStr = json["RewardType"].ToString();
            if (!Enum.TryParse<RewardType>(rewardTypeStr, true, out var parsedRewardType))
            {
                throw new Exception($"RewardSetID {RewardSetID}, QuestID {QuestID} - 지정되지 않은 RewardType 입니다: {rewardTypeStr}");
            }
            this.RewardType = parsedRewardType;

            ItemID = int.Parse(json["ItemID"].ToString());
            Amount = int.Parse(json["Amount"].ToString());
        }
    }
}