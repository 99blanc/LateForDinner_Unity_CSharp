using Cysharp.Threading.Tasks;
using LateForDinner.Data;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager
{
    private SceneID _previousID;
    private readonly Dictionary<SceneID, Spawnpoint> _spawnpoints = new Dictionary<SceneID, Spawnpoint>();
    public SceneID CurrentSceneID { get; private set; }

    public SceneManager()
        => GetScene();

    private void GetScene()
    {
        string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        CurrentSceneID = ParseSceneID(activeSceneName);
    }

    public async UniTask LoadSceneAsync(SceneID targetSceneID)
    {
        if (!TryGetSceneData(targetSceneID, out var sceneData))
            return;

        PrepareSceneTransition(targetSceneID);
        await ExecuteUnitySceneLoadAsync(sceneData.Tag);
    }

    public async UniTask MoveToTransitionAsync(SceneID targetSceneID)
    {
        if (!ValidateSceneTransition(targetSceneID))
            return;

        await LoadSceneAsync(targetSceneID);
    }

    public void RegisterSpawnpoint(Spawnpoint spawn)
    {
        if (IsSpawnValid(spawn) && !_spawnpoints.ContainsKey(spawn.ToSceneID))
            _spawnpoints.Add(spawn.ToSceneID, spawn);
    }

    public void UnregisterSpawnpoint(Spawnpoint spawn)
    {
        if (IsSpawnValid(spawn))
            _spawnpoints.Remove(spawn.ToSceneID);
    }

    public void RelocateCharacterToSpawnpoint()
    {
        if (IsCharacterNull())
        {
            Log.Warning(LocalizationKey.Log_Scene_NotFoundCharacter);
            return;
        }

        if (IsPreviousSceneBeforeHospital())
        {
            Log.System(LocalizationKey.Log_Scene_NotExistPreviousScene);
            return;
        }

        if (TryGetTargetSpawnpoint(out var targetSpawn))
        {
            Managers.Game.Character.RelocateTo(targetSpawn);
            Log.System(LocalizationKey.Log_Scene_NormalizedSpawn, targetSpawn.transform.position.ToString());
        }
        else
            Log.Warning(LocalizationKey.Log_Scene_NotFoundSpawnpoint, _previousID.ToString());
    }

    private bool TryGetSceneData(SceneID sceneID, out SceneData data)
    {
        int id = (int)sceneID;

        if (Managers.Data.Scenes.TryGetValue(id, out data) && data != null)
            return true;

        Log.Error(LocalizationKey.Log_Scene_LoadFailed, id.ToString());
        data = null;
        return false;
    }

    private void PrepareSceneTransition(SceneID targetSceneID)
    {
        _previousID = CurrentSceneID;
        _spawnpoints.Clear();
        CurrentSceneID = targetSceneID;
        Managers.Control.ClearInputStates();
    }

    private async UniTask ExecuteUnitySceneLoadAsync(string sceneTag)
    {
        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneTag);
        Log.System(LocalizationKey.Log_Scene_LoadSuccess, sceneTag);
    }

    private bool ValidateSceneTransition(SceneID targetSceneID)
    {
        if (_previousID == SceneID.Bootstrap)
            return true;

        int currentScene = (int)CurrentSceneID;
        int targetScene = (int)targetSceneID;
        var transitions = Managers.Data.SceneTransitions[currentScene];

        foreach (var transition in transitions)
        {
            if (transition.ToSceneID == targetScene)
                return true;
        }

        Log.Warning(LocalizationKey.Log_Scene_TransitionFailed, GetSceneTag(currentScene), GetSceneTag(targetScene));
        return false;
    }

    private SceneID ParseSceneID(string sceneName)
        => System.Enum.TryParse<SceneID>(sceneName, out var sceneID) ? sceneID : SceneID.Bootstrap;

    private bool IsSpawnValid(Spawnpoint spawn)
        => spawn != null;

    private bool IsCharacterNull()
        => Managers.Game.Character == null;

    private bool IsPreviousSceneBeforeHospital()
        => _previousID < SceneID.Hospital1;

    private bool TryGetTargetSpawnpoint(out Spawnpoint targetSpawn)
        => _spawnpoints.TryGetValue(_previousID, out targetSpawn) && targetSpawn != null;

    private string GetSceneTag(int sceneID)
        => Managers.Data.Scenes.TryGetValue(sceneID, out var data) ? data.Tag : sceneID.ToString();
}
