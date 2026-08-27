using MemoryPack;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class AttributeData
    {
        public string Key { get; set; }
        public string DataType { get; set; }
    }
}
