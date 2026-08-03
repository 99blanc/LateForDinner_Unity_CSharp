using Cysharp.Threading.Tasks;
using UnityEngine;

public class SceneManager
{
    public async UniTask LoadAsync(string scene)
    {
        var loadScene = await Managers.UI.OpenScreenAsync<UILoadScreen>();

        await loadScene.LoadAsync(0.1f, Managers.Localization.Get(Localization.Log_SceneManager_Load_Map));
        await loadScene.PlayAsync();

        var async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene);
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            float targetProgress = Mathf.Lerp(0.1f, 0.9f, async.progress / 0.9f);

            await loadScene.LoadAsync(targetProgress, Managers.Localization.Get(Localization.Log_SceneManager_Load_Data));
            
            if (async.progress >= 0.9f)
            {
                await loadScene.LoadAsync(1.0f, Managers.Localization.Get(Localization.Log_SceneManager_Load_Complete));
                await UniTask.Delay(500);

                async.allowSceneActivation = true;

                break;
            }

            await UniTask.Yield();
        }

        loadScene.Close();
    }
}
