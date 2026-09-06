using MemoryPack;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class ArmorItemData
    {
        public int ID { get; set; }
        public string ArmorCategory { get; set; }
    }
}
