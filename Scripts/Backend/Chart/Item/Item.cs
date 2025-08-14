using LitJson;
using System; 

namespace BackendData.Chart.Item
{
    public class Item
    {
        public int ItemID { get; private set; }
        public string Name_KR { get; private set; }
        public string Name_ENG { get; private set; }
        public string Des_KR { get; private set; }
        public string Des_ENG { get; private set; }
        public string IconID { get; private set; }
        public RewardType RewardType { get; private set; }
        public int Amount { get; private set; }

        public Item(JsonData json)
        {
            ItemID = int.Parse(json["ItemID"].ToString());
            Name_KR = json["Name_KR"].ToString();
            Name_ENG = json["Name_ENG"].ToString();
            Des_KR = json["Des_KR"].ToString();
            Des_ENG = json["Des_ENG"].ToString();
            IconID = json["IconID"].ToString();

            string rewardTypeStr = json["RewardType"].ToString();
            if (!Enum.TryParse<RewardType>(rewardTypeStr, true, out var parsedRewardType)) // true for ignoreCase
            {
                throw new ArgumentException($"ItemID {ItemID} - Invalid RewardType: {rewardTypeStr}");
            }
            this.RewardType = parsedRewardType;

            Amount = int.Parse(json["Amount"].ToString());
        }
    }
}