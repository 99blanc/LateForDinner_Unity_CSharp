using Cysharp.Threading.Tasks;
using LateForDinner.Data;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager
{
    private SceneID _previousID = SceneID.Bootstrap;
    private readonly List<SpawnpointProp> _spawnpoints = new List<SpawnpointProp>();
    public SceneID CurrentSceneID { get; private set; }

    public SceneManager()
        => GetScene();

    private void GetScene()
    {
        string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        CurrentSceneID = ParseSceneID(activeSceneName);
    }

    public async UniTask LoadSceneAsync(SceneID targetSceneID, bool forceTransition = false)
    {
        if (!forceTransition && !ValidateSceneTransition(targetSceneID))
            return;

        if (!TryGetSceneData(targetSceneID, out var sceneData))
            return;

        PrepareSceneTransition(targetSceneID);
        await ExecuteUnitySceneLoadAsync(sceneData.Tag);
    }

    public void RegisterSpawnpoint(SpawnpointProp spawn)
    {
        if (spawn != null && !_spawnpoints.Contains(spawn))
            _spawnpoints.Add(spawn);
    }

    public void UnregisterSpawnpoint(SpawnpointProp spawn)
    {
        if (spawn != null)
            _spawnpoints.Remove(spawn);
    }

    public void RelocateCharacterToSpawnpoint()
    {
        if (Managers.Game.Player == null)
        {
            Log.Warning(LocalizationKey.Log_Scene_NotFoundCharacter);
            return;
        }

        if (_previousID < SceneID.Hospital1)
            Log.System(LocalizationKey.Log_Scene_NotExistPreviousScene);

        if (TryGetTargetSpawnpoint(out var targetSpawn))
        {
            Managers.Game.Player.RelocateTo(targetSpawn);
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
        Managers.Prop.Clear();
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

    private bool TryGetTargetSpawnpoint(out SpawnpointProp targetSpawn)
    {
        targetSpawn = null;

        if (_spawnpoints.Count == 0)
            return false;

        targetSpawn = _spawnpoints.Find(s => s != null && s.ToSceneID == _previousID);

        if (targetSpawn == null && _spawnpoints.Count > 0)
        {
            targetSpawn = _spawnpoints[0];
            Log.System(LocalizationKey.Log_Scene_NotFoundSpawnpoint, _previousID.ToString());
        }

        return targetSpawn != null;
    }

    private string GetSceneTag(int sceneID)
        => Managers.Data.Scenes.TryGetValue(sceneID, out var data) ? data.Tag : sceneID.ToString();
}
