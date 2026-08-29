using MemoryPack;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class SceneTransitionData
    {
        public int SceneID { get; set; }
        public string Type { get; set; }
        public int ToSceneID { get; set; }
    }
}
