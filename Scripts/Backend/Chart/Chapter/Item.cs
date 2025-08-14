using LitJson;

namespace BackendData.Chart.Chapter
{
    public class Item
    {
        public int ChapterID { get; private set; }
        public string Name_KR { get; private set; }
        public string Name_ENG { get; private set; }

        public Item(JsonData json)
        {
            ChapterID = int.Parse(json["ChapterID"].ToString());
            Name_KR = json["Name_KR"].ToString();
            Name_ENG = json["Name_ENG"].ToString();
        }
    }
}