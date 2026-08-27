using MemoryPack;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class SceneData
    {
        public int ID { get; set; }
        public string Tag { get; set; }
        public string LocalizationKey { get; set; }
    }
}
