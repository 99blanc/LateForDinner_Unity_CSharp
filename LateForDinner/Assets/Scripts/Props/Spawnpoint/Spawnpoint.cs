using UnityEngine;

public class Spawnpoint : Prop
{
    protected override bool UseSaveState => false;

    [SerializeField] private SceneID _toSceneID;
    public SceneID ToSceneID => _toSceneID;

    protected override void Awake()
        => Managers.Scene.RegisterSpawnpoint(this);

    protected override void OnDestroy()
        => Managers.Scene.UnregisterSpawnpoint(this);
}
