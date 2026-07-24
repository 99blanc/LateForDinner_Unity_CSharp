using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class Settings
{
    public SoundSettings Sound { get; set; } = SoundSettings.Default;
    public GraphicSettings Graphic { get; set; } = GraphicSettings.Default;
    public AccessSettings Access { get; set; } = AccessSettings.Default;
}

[MemoryPackable]
public partial class SoundSettings
{
    public float vMaster;
    public float vBGM;
    public float vAmbient;
    public float vSFX;
    public float vUI;
    public bool mMaster;
    public bool mBGM;
    public bool mAmbient;
    public bool mSFX;
    public bool mUI;
    public bool mBackground;

    [MemoryPackIgnore]
    public static SoundSettings Default => new SoundSettings() 
    {
        vMaster = 1.0f,
        vBGM = 1.0f,
        vAmbient = 1.0f,
        vSFX = 1.0f,
        vUI = 1.0f,
        mMaster = false,
        mBGM = false,
        mAmbient = false,
        mSFX = false,
        mUI = false,
        mBackground = true
    };
}

[MemoryPackable]
public partial class GraphicSettings
{
    public int rWidth;
    public int rHeight;
    public FullScreenMode screenMode;
    public bool vSync;
    public bool antiAliasing;
    public int quality;
    public int frameRate;
    public bool bloom;
    public bool ao;

    public static GraphicSettings Default => new GraphicSettings() 
    {
        rWidth = 1920,
        rHeight = 1080,
        screenMode = FullScreenMode.FullScreenWindow,
        vSync = true,
        antiAliasing = false,
        quality = 1,
        frameRate = 60,
        bloom = true,
        ao = true
    };
}

[MemoryPackable]
public partial class AccessSettings
{
    public string language;
    public string keybind;
    public bool modifierDash;
    public bool highContrast;

    public static AccessSettings Default => new AccessSettings()
    {
        language = Literal.Languages.Korean,
        keybind = string.Empty,
        modifierDash = false,
        highContrast = false
    };
}
