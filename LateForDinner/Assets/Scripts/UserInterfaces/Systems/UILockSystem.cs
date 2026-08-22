using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class UILockSystem : UISystem, IAnimatable
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
    }

    public async UniTask PlayAsync()
    {
        var image = GetImage((int)Images.RotateImage);
        var token = GetToken("RotateTask");

        try
        {
            while (!token.IsCancellationRequested)
            {
                image?.transform.Rotate(0f, 0f, -300f * Time.unscaledDeltaTime);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException)
        {
            // DESC ::: 비동기 실행 후 탈출
        }
    }
}
