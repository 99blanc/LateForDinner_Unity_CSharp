using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ZLinq;

public class GraphicManager
{
    private GameObject _root;
    public GameObject Root
    {
        get
        {
            if (_root == null)
                InitRoot();

            return _root;
        }
    }
    private VolumeProfile _volumeProfile;
    private Resolution[] _cachedResolutions;

    public GameObject InitRoot()
    {
        _root = new GameObject { name = Literal.Roots.Graphics };
        _root.transform.SetParent(Managers.Instance.transform, false);
        Log.System(LocalizationKey.Log_Graphic_RootInitialized);
        return _root;
    }

    public void Setup()
    {
        var _ = Root;
        var system = Managers.Resource.Instantiate(Literal.Assets.GlobalVolume, _root.transform, false);
        system.name = Literal.Roots.Volume;
        var asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        if (asset != null)
            _volumeProfile = asset.volumeProfile;

        GetCachedResolutions();
    }

    public Resolution[] GetCachedResolutions()
    {
        if (_cachedResolutions != null)
            return _cachedResolutions;

        _cachedResolutions = Screen.resolutions
        .Select(r => new Resolution { width = r.width, height = r.height, refreshRateRatio = r.refreshRateRatio })
        .GroupBy(r => new { r.width, r.height })
        .Select(g => g.OrderByDescending(r => (double)r.refreshRateRatio.numerator / r.refreshRateRatio.denominator).First())
        .OrderBy(r => r.width)
        .ThenBy(r => r.height)
        .ToArray();
        return _cachedResolutions;
    }

    public void ApplyGraphicOptions(GraphicOption graphic)
    {
        QualitySettings.SetQualityLevel((int)graphic.quality);
        QualitySettings.vSyncCount = graphic.vSync ? 1 : 0;
        Screen.SetResolution(graphic.rWidth, graphic.rHeight, graphic.screenMode, graphic.Resolution.refreshRateRatio);
        Application.targetFrameRate = graphic.rRefreshRate;
        ApplyPostProcessing(graphic);
        ApplyCameraSettings(graphic);
    }

    private void ApplyPostProcessing(GraphicOption graphic)
    {
        if (_volumeProfile == null) 
            return;

        if (_volumeProfile.TryGet(out Bloom bloom))
            bloom.active = graphic.bloom;

        if (_volumeProfile.TryGet(out Vignette vignette))
            vignette.active = graphic.vignette;

        if (_volumeProfile.TryGet(out MotionBlur mblur))
            mblur.active = graphic.mblur;

        if (_volumeProfile.TryGet(out ColorAdjustments contrast))
            contrast.active = graphic.contrast;
    }

    private void ApplyCameraSettings(GraphicOption graphic)
        => Managers.Camera.SetAntialiasing(graphic.antiAliasing);
}
