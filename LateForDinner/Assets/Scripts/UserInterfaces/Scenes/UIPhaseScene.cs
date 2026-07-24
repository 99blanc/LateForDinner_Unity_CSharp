using UnityEngine;

public class UIPhaseScene : UIScene
{
    private enum Texts
    {
        ProgressText, 
        MessageText,
    }

    private enum Images
    {
        ProgressImage,
    }

    public override void Init()
    {
        base.Init();
        BindText(typeof(Texts));
        BindImage(typeof(Images));
    }

    public void Phase(float progress, string message)
    {
        GetImage((int)Images.ProgressImage).fillAmount = progress;
        GetText((int)Texts.ProgressText).text = $"{Mathf.RoundToInt(progress * 100f)}%";
        GetText((int)Texts.MessageText).text = message;
    }
}
