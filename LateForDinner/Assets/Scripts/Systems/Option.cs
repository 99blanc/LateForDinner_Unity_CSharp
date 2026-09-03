using MemoryPack;
using UnityEngine;

[MemoryPackable]
public partial class Option
{
    public SoundOption Sound { get; set; }
    public GraphicOption Graphic { get; set; }
    public AccessOption Access { get; set; }
    public DebugOption Debug { get; set; }

    [MemoryPackIgnore]
    public static Option Default => new Option()
    {
        Sound = SoundOption.Default,
        Graphic = GraphicOption.Default,
        Access = AccessOption.Default,
        Debug = DebugOption.Default
    };
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
        mute = false,
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
    public TextureQuality quality;
    public bool bloom;
    public bool vignette;
    public bool mblur;
    public bool contrast;

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
            var (targetWidth, targetHeight, targetRefreshRate) = GetBestResolution();

            return new GraphicOption()
            {
                rWidth = targetWidth,
                rHeight = targetHeight,
                rRefreshRate = targetRefreshRate,
                screenMode = FullScreenMode.ExclusiveFullScreen,
                vSync = true,
                antiAliasing = true,
                quality = TextureQuality.High,
                bloom = true,
                vignette = true,
                mblur = true,
                contrast = true,
            };
        }
    }

    private static (int width, int height, int refreshRate) GetBestResolution()
    {
        int targetWidth = 1920;
        int targetHeight = 1080;
        int targetRefreshRate = 60;
        Resolution[] resolutions = Screen.resolutions;

        if (resolutions == null || resolutions.Length <= 0)
            return (targetWidth, targetHeight, targetRefreshRate);

        int maxWidth = GetMaxWidth(resolutions);
        double maxRefreshRate = 0;

        for (int ndex = 0; ndex < resolutions.Length; ndex++)
        {
            var res = resolutions[ndex];

            if (res.width != maxWidth)
                continue;

            double currentRefresh = res.refreshRateRatio.value;

            if (currentRefresh <= maxRefreshRate)
                continue;

            maxRefreshRate = currentRefresh;
            targetWidth = res.width;
            targetHeight = res.height;
            targetRefreshRate = Mathf.RoundToInt((float)currentRefresh);
        }

        return (targetWidth, targetHeight, targetRefreshRate);
    }

    private static int GetMaxWidth(Resolution[] resolutions)
    {
        int maxWidth = 0;

        for (int index = 0; index < resolutions.Length; index++)
        {
            if (resolutions[index].width > maxWidth)
                maxWidth = resolutions[index].width;
        }

        return maxWidth;
    }
}

[MemoryPackable]
public partial class AccessOption
{
    public string language;
    public string keybind;
    public bool modifierDash;

    [MemoryPackIgnore]
    public static AccessOption Default => new AccessOption()
    {
        language = Literal.Languages.Korean,
        keybind = string.Empty,
        modifierDash = false
    };
}

[MemoryPackable]
public partial class DebugOption
{
    public bool isDebugMode;
    public bool enableConsole;

    [MemoryPackIgnore]
    public static DebugOption Default => new DebugOption()
    {
        isDebugMode = false,
        enableConsole = false
    };
}
