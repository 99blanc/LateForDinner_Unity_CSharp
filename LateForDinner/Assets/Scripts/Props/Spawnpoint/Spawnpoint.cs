using UnityEngine;

public class Spawnpoint : Prop
{
    [Header("Spawnpoint Settings")]
    [SerializeField] private SceneID _toSceneID;
    public SceneID ToSceneID => _toSceneID;
    protected override bool UseSaveState => false;

    protected override void Awake()
        => Managers.Scene.RegisterSpawnpoint(this);

    protected override void OnDestroy()
        => Managers.Scene.UnregisterSpawnpoint(this);
}
