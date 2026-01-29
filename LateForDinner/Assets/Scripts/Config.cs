using MemoryPack;
using Token.LANGUAGE;

[MemoryPackable]
public partial class Config
{
    public GameplayConfig gameplay { get; set; } = new();
    public AudioConfig audio { get; set; } = new();
    public ControlConfig control { get; set; } = new();
    public Language language { get; set; } = Language.KOREAN;
}

[MemoryPackable]
public partial struct GameplayConfig
{
    public bool isDashComboOnly { get; set; }
    public float screenShakeIntensity { get; set; }

    [MemoryPackIgnore]
    public static GameplayConfig Default => new()
    {
        isDashComboOnly = false,
        screenShakeIntensity = 1.0f
    };
}

[MemoryPackable]
public partial struct AudioConfig
{
    public float vAll { get; set; }
    public float vBGM { get; set; }
    public float vSFX { get; set; }

    [MemoryPackIgnore]
    public static AudioConfig Default => new()
    {
        vAll = 1.0f,
        vBGM = 1.0f,
        vSFX = 1.0f
    };
}

[MemoryPackable]
public partial struct ControlConfig
{
    public string keybind { get; set; }
}
