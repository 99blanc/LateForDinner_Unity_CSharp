using Cysharp.Threading.Tasks;
using LateForDinner.Data;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager
{
    public SceneManager()
        => GetScene();

    private SceneID _previousID;
    private readonly Dictionary<SceneID, Spawnpoint> _spawnpoints = new Dictionary<SceneID, Spawnpoint>();
    public SceneID CurrentSceneID { get; private set; }

    private void GetScene()
    {
        string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (System.Enum.TryParse<SceneID>(activeSceneName, out var sceneID))
            CurrentSceneID = sceneID;
        else
            CurrentSceneID = SceneID.Bootstrap;
    }

    public async UniTask LoadSceneAsync(SceneID targetSceneID)
    {
        if (TryGetSceneData(targetSceneID, out var sceneData) == false)
            return;

        PrepareSceneTransition(targetSceneID);
        await ExecuteUnitySceneLoadAsync(sceneData.Tag);
    }

    public async UniTask MoveToTransitionAsync(SceneID targetSceneID)
    {
        if (ValidateSceneTransition(targetSceneID) == false)
            return;

        await LoadSceneAsync(targetSceneID);
    }

    public void RegisterSpawnpoint(Spawnpoint spawn)
    {
        if (spawn != null && !_spawnpoints.ContainsKey(spawn.FromSceneID))
            _spawnpoints.Add(spawn.FromSceneID, spawn);
    }

    public void UnregisterSpawnpoint(Spawnpoint spawn)
    {
        if (spawn != null)
            _spawnpoints.Remove(spawn.FromSceneID);
    }

    public void RelocateCharacterToSpawnpoint()
    {
        if (Managers.Game.Character == null)
        {
            Log.Warning(LocalizationKey.Log_Scene_NotFoundCharacter);
            return;
        }

        if (_spawnpoints.TryGetValue(_previousID, out var targetSpawn) && targetSpawn != null)
        {
            Vector3 spawnPosition = targetSpawn.transform.position;
            var collider = Managers.Game.Character.GetComponentAssert<CapsuleCollider2D>();
            spawnPosition.y -= collider.offset.y;
            Managers.Game.Character.transform.position = spawnPosition;
            Managers.Game.Character.transform.rotation = targetSpawn.transform.rotation;
            Log.System(LocalizationKey.Log_Scene_NormalizedSpawn, spawnPosition.ToString());
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

        foreach (var transition in Managers.Data.SceneTransitions.Values)
        {
            if (transition.FromSceneID == currentScene && transition.ToSceneID == targetScene)
                return true;
        }

        string currentTag = GetSceneTag(currentScene);
        string targetTag = GetSceneTag(targetScene);
        Log.Warning(LocalizationKey.Log_Scene_TransitionFailed, currentTag, targetTag);
        return false;
    }

    private string GetSceneTag(int sceneID)
        => Managers.Data.Scenes.TryGetValue(sceneID, out var data) ? data.Tag : sceneID.ToString();
}
