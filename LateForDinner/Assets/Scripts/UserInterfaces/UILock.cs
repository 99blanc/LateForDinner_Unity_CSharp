using Cysharp.Threading.Tasks;
using UnityEngine;

public class UILock : UserInterface
{
    private enum Images
    {
        BackgroundImage,
        RotateImage
    }

    public override void Init()
    {
        base.Init();
        BindImage(typeof(Images));
        SetActive(false);
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);

        if (isActive)
            PlayAsync().Forget();
    }

    public async UniTask PlayAsync()
    {
        var image = GetImage((int)Images.RotateImage);
        var token = GetToken("RotateTask");

        if (image == null)
            return;

        while (!token.IsCancellationRequested)
        {
            image.transform.Rotate(0f, 0f, -300f * Time.unscaledDeltaTime);

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }
}
