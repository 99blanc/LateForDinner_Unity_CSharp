using MemoryPack;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class ShopItemData
    {
        public int ID { get; set; }
        public int ShopID { get; set; }
        public int ItemID { get; set; }
        public float BuyPrice { get; set; }
        public float SellPrice { get; set; }
        public int CurrencyID { get; set; }
        public int Stock { get; set; }
        public string Condition { get; set; }
    }
}
