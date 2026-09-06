using MemoryPack;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class WeaponItemData
    {
        public int ID { get; set; }
        public string WeaponCategory { get; set; }
    }
}
