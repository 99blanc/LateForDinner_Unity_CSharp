using MemoryPack;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class CharacterData
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
