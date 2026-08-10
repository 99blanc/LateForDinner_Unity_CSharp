using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class Option
{
    public SoundOption Sound { get; set; } = SoundOption.Default;
    public GraphicOption Graphic { get; set; } = GraphicOption.Default;
    public AccessOption Access { get; set; } = AccessOption.Default;
}

[MemoryPackable]
public partial class SoundOption
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
    public bool mute;

    [MemoryPackIgnore]
    public static SoundOption Default => new SoundOption() 
    {
        vMaster = 1.0f,
        vBGM = 1.0f,
        vAmbient = 1.0f,
        vSFX = 1.0f,
        vUI = 1.0f,
        mMaster = true,
        mBGM = true,
        mAmbient = true,
        mSFX = true,
        mUI = true,
        mute = true
    };
}

[MemoryPackable]
public partial class GraphicOption
{
    public int rWidth;
    public int rHeight;
    public int rRefreshRate;
    public FullScreenMode screenMode;
    public bool vSync;
    public bool antiAliasing;
    public Quality quality;
    public bool bloom;
    public bool ambientOccusion;

    [MemoryPackIgnore]
    public Resolution Resolution
    {
        get => new Resolution
        {
            width = rWidth,
            height = rHeight,
            refreshRateRatio = new RefreshRate { numerator = (uint)rRefreshRate, denominator = 1 }
        };
        set
        {
            rWidth = value.width;
            rHeight = value.height;
            rRefreshRate = Mathf.RoundToInt((float)value.refreshRateRatio.numerator / value.refreshRateRatio.denominator);
        }
    }

    [MemoryPackIgnore]
    public static GraphicOption Default
    {
        get
        {
            int targetWidth = 1920;
            int targetHeight = 1080;
            int targetRefreshRate = 60;
            Resolution[] resolutions = Screen.resolutions;

            if (resolutions != null && resolutions.Length > 0)
            {
                int maxWidth = 0;

                foreach (var res in resolutions)
                {
                    if (res.width > maxWidth)
                        maxWidth = res.width;
                }

                double maxRefreshRate = 0;

                foreach (var res in resolutions)
                {
                    if (res.width == maxWidth)
                    {
                        double currentRefresh = res.refreshRateRatio.value;
                        if (currentRefresh > maxRefreshRate)
                        {
                            maxRefreshRate = currentRefresh;
                            targetWidth = res.width;
                            targetHeight = res.height;
                            targetRefreshRate = Mathf.RoundToInt((float)currentRefresh);
                        }
                    }
                }
            }

            return new GraphicOption()
            {
                rWidth = targetWidth,
                rHeight = targetHeight,
                rRefreshRate = targetRefreshRate,
                screenMode = FullScreenMode.FullScreenWindow,
                vSync = true,
                antiAliasing = false,
                quality = Quality.High,
                bloom = true,
                ambientOccusion = true
            };
        }
    }
}

[MemoryPackable]
public partial class AccessOption
{
    public string language;
    public string keybind;
    public bool modifierDash;
    public bool highContrast;

    [MemoryPackIgnore]
    public static AccessOption Default => new AccessOption()
    {
        language = Literal.Languages.Korean,
        keybind = string.Empty,
        modifierDash = false,
        highContrast = false
    };
}
