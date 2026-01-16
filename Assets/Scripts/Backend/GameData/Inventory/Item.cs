namespace BackendData.GameData.UserInventory
{
    public class Item
    {
        public int ItemID { get; private set; }
        public int Amount { get; internal set; }

        public Item(int itemId, int amount)
        {
            ItemID = itemId;
            Amount = amount;
        }
    }
}
