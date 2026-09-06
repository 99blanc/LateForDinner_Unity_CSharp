using MemoryPack;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class ArmorCategoryData
    {
        public string ArmorCategory { get; set; }
        public int Bitmask { get; set; }
    }
}
