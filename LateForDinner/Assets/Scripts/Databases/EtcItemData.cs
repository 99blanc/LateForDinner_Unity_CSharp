using MemoryPack;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class EtcItemData
    {
        public int ID { get; set; }
        public string EtcType { get; set; }
    }
}
