using LitJson;

namespace BackendData.Chart.Stage
{
    public class Item
    {
        public int StageID { get; private set; }
        public int ChapterID { get; private set; }
        public int StageOrder { get; private set; }
        public string Name_KR { get; private set; }
        public string Name_ENG { get; private set; }
        public int GoldReward { get; private set; }
        public int ExpReward { get; private set; }


        public Item(JsonData json)
        {
            StageID = int.Parse(json["StageID"].ToString());
            ChapterID = int.Parse(json["ChapterID"].ToString());
            StageOrder = int.Parse(json["StageOrder"].ToString());
            Name_KR = json["Name_KR"].ToString();
            Name_ENG = json["Name_ENG"].ToString();
            GoldReward = int.Parse(json["GoldReward"].ToString());
            ExpReward = int.Parse(json["ExpReward"].ToString());
        }
    }
}