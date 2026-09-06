using MemoryPack;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class ConsumptionItemData
    {
        public int ID { get; set; }
        public string ConsumptionType { get; set; }
        public float Cooldown { get; set; }
        public string TargetType { get; set; }
    }
}
