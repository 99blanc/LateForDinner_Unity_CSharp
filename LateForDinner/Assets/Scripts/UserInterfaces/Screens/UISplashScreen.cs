using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class UISplashScreen : UIScreen, IAnimatable
{
    private enum Images 
    { 
        BackgroundImage,
        UnityImage,
        TeamImage,
        TitleImage
    }

    public override void Init()
    {
        base.Init();
        BindImage(typeof(Images));
        Revert();
        Managers.Control.Subscribe(Literal.Hotkeys.Submit, () => CancelToken("SplashTask")).AddTo(this);
    }

    public override void Get()
        => Revert();

    private void Revert()
    {
        SetAlpha(Images.UnityImage, 0f);
        SetAlpha(Images.TeamImage, 0f);
        SetAlpha(Images.TitleImage, 0f);
    }

    private void SetAlpha(Images imageType, float alpha)
    {
        var image = GetImage((int)imageType);

        if (image != null)
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    public async UniTask PlayAsync()
    {
        try
        {
            var token = GetToken("SplashTask");
            var unityImage = GetImage((int)Images.UnityImage);
            var teamImage = GetImage((int)Images.TeamImage);
            var titleImage = GetImage((int)Images.TitleImage);

            await unityImage.FadeAsync(0f, 1f, 1.2f, token: token);
            await UniTask.Delay(1000, cancellationToken: token);
            await unityImage.FadeAsync(1f, 0f, 1.2f, token: token);
            await teamImage.FadeAsync(0f, 1f, 1.2f, token: token);
            await UniTask.Delay(1000, cancellationToken: token);
            await teamImage.FadeAsync(1f, 0f, 1.2f, token: token);
            await UniTask.Delay(1000, cancellationToken: token);
            GetImage((int)Images.BackgroundImage).color = new Color(1f, 1f, 1f, 0f);
            await titleImage.FadeAsync(0f, 1f, 0.2f, 2.0f, token: token);
            await UniTask.Delay(2000, cancellationToken: token);
        }
        catch (System.OperationCanceledException)
        {
            Log.System(Localization.UI_Splash_Screen_Skip);
        }
    }
}
