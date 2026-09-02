using R3;
using UnityEngine;

public class UIFPSSystem : UISystem
{
    private enum Texts
    {
        FPSText
    }

    private float _pollingTime = Define.Framerate.PollingTime;

    public override void OnInit()
    {
        base.OnInit();
        BindText(typeof(Texts));
    }

    public override void OnGet()
    {
        base.OnGet();
        float accumTime = 0f;
        int frameCount = 0;
        Observable.EveryUpdate()
        .Subscribe(_ =>
        {
            accumTime += Time.unscaledDeltaTime;
            frameCount++;

            if (accumTime >= _pollingTime)
            {
                int fps = Mathf.RoundToInt(frameCount / accumTime);
                GetText(Texts.FPSText).text = Managers.Localization.Get(LocalizationKey.UI_FPS_System_Indicator, fps);
                accumTime = 0f;
                frameCount = 0;
            }
        }).RegisterToPool(this);
    }
}
