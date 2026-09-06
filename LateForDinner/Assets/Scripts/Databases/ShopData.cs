using MemoryPack;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class ShopData
    {
        public int ID { get; set; }
        public string NameKey { get; set; }
        public string DescriptionKey { get; set; }
        public int CharacterID { get; set; }
    }
}
