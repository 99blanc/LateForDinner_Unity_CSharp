using MemoryPack;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class LocalizationData
    {
        public string Key { get; set; }
        public string Text { get; set; }
    }
}
