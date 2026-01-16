namespace BackendData.GameData.QuestAchievement
{
    public class Item
    {
        public bool IsAchieve { get; private set; }
        public int QuestID { get; private set; }

        public Item(int questId, bool isAchieve)
        {
            IsAchieve = isAchieve;
            QuestID = questId;  
        }
    }
}