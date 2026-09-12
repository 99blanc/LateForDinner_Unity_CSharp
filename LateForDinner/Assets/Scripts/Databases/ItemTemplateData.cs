using MemoryPack;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class ItemTemplateData
    {
        public int ItemID { get; set; }
        public string ApplyType { get; set; }
        public string AttributeKey { get; set; }
        public string Value { get; set; }
        public float Duration { get; set; }
        public bool Flag { get; set; }
    }
}
