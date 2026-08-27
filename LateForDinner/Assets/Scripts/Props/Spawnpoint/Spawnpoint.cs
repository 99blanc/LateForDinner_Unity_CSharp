using UnityEngine;

public class Spawnpoint : MonoBehaviour
{
    [SerializeField] private SceneID _fromSceneID;
    public SceneID FromSceneID => _fromSceneID;

    private void Awake()
        => Managers.Scene.RegisterSpawnpoint(this);

    private void OnDestroy()
        => Managers.Scene.UnregisterSpawnpoint(this);
}
