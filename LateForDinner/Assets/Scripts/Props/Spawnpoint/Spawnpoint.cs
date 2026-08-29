using UnityEngine;

public class Spawnpoint : Prop
{
    protected override bool UseSaveState => false;

    [SerializeField] private SceneID _toSceneID;
    public SceneID ToSceneID => _toSceneID;

    protected override void Awake()
    {
        base.Awake();
        Managers.Scene.RegisterSpawnpoint(this);
    }

    private void OnDestroy()
        => Managers.Scene.UnregisterSpawnpoint(this);
}
