using Cysharp.Text;
using System;
using UnityEngine;

public abstract class Prop : MonoBehaviour
{
    [SerializeField] public PropKey _propKey;
    [SerializeField, HideInInspector] private string _guid;
    private string _uniqueKey;
    public string UniqueKey => _uniqueKey;
    public int Priority
    {
        get
        {
            if (Managers.Data.Props != null && Managers.Data.Props.TryGetValue(_propKey.ToString(), out var data))
                return data.Priority;
            return 0;
        }
    }
    protected abstract bool UseSaveState { get; }

    protected virtual void Awake()
    {
        if (UseSaveState)
            GenerateUniqueKey();

        Managers.Scene.RegisterProp(this);
    }

    protected virtual void Start()
    {
        if (UseSaveState)
            LoadState();
    }

    private void GenerateUniqueKey()
    {
        int currentSceneID = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        _uniqueKey = ZString.Concat(_guid, currentSceneID, "_", _propKey.ToString());
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (UseSaveState && string.IsNullOrEmpty(_guid) && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            _guid = Guid.NewGuid().ToString("N");
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif

    protected virtual void LoadState()
    {
        if (!UseSaveState || string.IsNullOrEmpty(_uniqueKey)) 
            return;

        if (Managers.Save != null && Managers.Save.CurrentData.InteractableStates.TryGetValue(_uniqueKey, out bool state))
            ApplyState(state);
        else
            ApplyState(false);
    }

    protected void SaveState(bool state)
    {
        if (!UseSaveState || Managers.Save == null || string.IsNullOrEmpty(_uniqueKey)) 
            return;

        Managers.Save.CurrentData.InteractableStates[_uniqueKey] = state;
    }

    protected virtual void ApplyState(bool state) { }

    protected virtual void OnDestroy()
        => Managers.Scene.UnregisterProp(this);
}
