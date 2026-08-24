using MemoryPack;

[MemoryPackable]
public partial class PlayableCharacterData
{
    public int ID { get; set; }
    public string Name { get; set; }
    public int PlayableCharacterTemplateID { get; set; }
}
