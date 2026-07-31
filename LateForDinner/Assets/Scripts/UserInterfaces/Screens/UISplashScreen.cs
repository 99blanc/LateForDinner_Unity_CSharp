using Cysharp.Threading.Tasks;
using UnityEngine;

public class UISplashScreen : UIScreen
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
        GetImage((int)Images.UnityImage).color = new Color(1f, 1f, 1f, 0f);
        GetImage((int)Images.TeamImage).color = new Color(1f, 1f, 1f, 0f);
        GetImage((int)Images.TitleImage).color = new Color(1f, 1f, 1f, 0f);
    }

    public async UniTask PlayAsync()
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
        await titleImage.FadeAsync(0f, 1f, 0.2f, 20.0f, token: token);
        GetImage((int)Images.BackgroundImage).color = new Color(1f, 1f, 1f, 0f);
        await UniTask.Delay(1000, cancellationToken: token);
    }
}
