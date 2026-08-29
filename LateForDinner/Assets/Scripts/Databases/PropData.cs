using MemoryPack;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class PropData
    {
        public string Key { get; set; }
        public string InteractionType { get; set; }
    }
}