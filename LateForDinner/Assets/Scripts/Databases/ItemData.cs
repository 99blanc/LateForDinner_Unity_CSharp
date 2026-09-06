using MemoryPack;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class ItemData
    {
        public int ID { get; set; }
        public string ItemType { get; set; }
        public string NameKey { get; set; }
        public string DescriptionKey { get; set; }
        public string FlavorKey { get; set; }
        public string AddressableKey { get; set; }
        public int MaxStack { get; set; }
    }
}
