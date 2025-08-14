namespace BackendData.GameData.StageAchievement
{
    public class Item
    {
        public bool IsAchieve { get; private set; }
        public int StageID { get; private set; }


        public Item(int stageId, bool isAchieve)
        {
            IsAchieve = isAchieve;
            StageID = stageId;
        }
    }
}
