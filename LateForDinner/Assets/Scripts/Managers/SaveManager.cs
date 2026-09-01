using Cysharp.Threading.Tasks;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.IO;
using LateForDinner.Data;
using Cysharp.Text;

public class SaveManager
{
    private int _currentSlot = -1;
    public int CurrentSlot => _currentSlot;
    public SaveMeta MetaData { get; private set; } = new SaveMeta();
    public SaveData CurrentData { get; private set; } = new SaveData();

    public async UniTask InitAsync()
        => await MetaAsync();

    public async UniTask LoadAsync(int index)
    {
        string path = GetPath(index);
        string backupPath = GetBackupPath(index);
        RestoreBackupIfNeeded(path, backupPath, index);

        try
        {
            if (!FileExists(path))
            {
                Newgame(index);
                return;
            }

            byte[] bytes = await File.ReadAllBytesAsync(path);
            CurrentData = MemoryPackSerializer.Deserialize<SaveData>(bytes) ?? SaveData.Default;
            _currentSlot = index;
            Log.System(LocalizationKey.Log_Save_LoadSuccess, index);
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Save_LoadFailed, index);
            Newgame(index);
        }
    }

    public async UniTask SaveAsync()
    {
        if (IsSlotInvalid())
            return;

        Sync();
        string path = GetPath(_currentSlot);
        string backupPath = GetBackupPath(_currentSlot);

        try
        {
            SyncMeta();
            byte[] bytes = MemoryPackSerializer.Serialize(CurrentData);
            string dir = Path.GetDirectoryName(path);
            EnsureDirectoryExists(dir);
            BackupExistingFile(path, backupPath);
            await File.WriteAllBytesAsync(path, bytes);
            await SaveMetaAsync();
            Log.System(LocalizationKey.Log_Save_SaveSuccess, _currentSlot);
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Save_SaveFailed, _currentSlot);
        }
    }

    public async UniTask MetaAsync()
    {
        string path = GetMetaPath();
        string backupPath = GetMetaBackupPath();
        RestoreBackupIfNeeded(path, backupPath);

        try
        {
            if (!FileExists(path))
            {
                MetaData = SaveMeta.Default;
                ValidateMeta();
                return;
            }

            byte[] bytes = await File.ReadAllBytesAsync(path);
            MetaData = MemoryPackSerializer.Deserialize<SaveMeta>(bytes) ?? SaveMeta.Default;
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Save_MetaLoadFailed);
            MetaData = SaveMeta.Default;
        }

        ValidateMeta();
    }

    public void EnsureSlot(int count)
    {
        InitMetaCollectionsIfNeeded();

        while (MetaData.Slots.Count < count)
            MetaData.Slots.Add(SlotMeta.Default);

        if (MetaData.SlotOrder.Count == MetaData.Slots.Count)
            return;

        MetaData.SlotOrder.Clear();

        for (int index = 0; index < MetaData.Slots.Count; index++)
            MetaData.SlotOrder.Add(index);
    }

    private void ValidateMeta()
    {
        InitMetaCollectionsIfNeeded();

        if (MetaData.SlotOrder.Count != MetaData.Slots.Count)
        {
            MetaData.SlotOrder.Clear();

            for (int index = 0; index < MetaData.Slots.Count; index++)
                MetaData.SlotOrder.Add(index);
        }

        for (int index = 0; index < MetaData.Slots.Count; index++)
        {
            if (MetaData.Slots[index] == null)
                MetaData.Slots[index] = SlotMeta.Default;
        }
    }

    private void InitMetaCollectionsIfNeeded()
    {
        if (MetaData.Slots == null)
            MetaData.Slots = new List<SlotMeta>();

        if (MetaData.SlotOrder == null)
            MetaData.SlotOrder = new List<int>();
    }

    private void SyncMeta()
    {
        InitMetaCollectionsIfNeeded();

        if (MetaData.Slots[_currentSlot] == null)
            MetaData.Slots[_currentSlot] = SlotMeta.Default;

        DateTime now = DateTime.Now;
        CurrentData.Year = now.Year;
        CurrentData.Month = now.Month;
        CurrentData.Date = now.Day;
        var meta = MetaData.Slots[_currentSlot];
        meta.Year = CurrentData.Year;
        meta.Month = CurrentData.Month;
        meta.Date = CurrentData.Date;
        meta.IsActive = true;
    }

    private async UniTask SaveMetaAsync()
    {
        string path = GetMetaPath();
        string backupPath = GetMetaBackupPath();

        try
        {
            byte[] bytes = MemoryPackSerializer.Serialize(MetaData);
            string dir = Path.GetDirectoryName(path);
            EnsureDirectoryExists(dir);
            BackupExistingFile(path, backupPath);
            await File.WriteAllBytesAsync(path, bytes);
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Save_MetaSaveFailed);
        }
    }

    public void Select(int index)
        => _currentSlot = index;

    public async UniTask ClearAsync(int index)
    {
        EnsureSlot(index + 1);
        MetaData.Slots[index] = SlotMeta.Default;
        CurrentData = SaveData.Default;
        _currentSlot = index;
        await SaveMetaAsync();
        string path = GetPath(index);
        DeleteFileIfExists(path);
        Log.System(LocalizationKey.Log_Save_ClearSuccess, index);
    }

    public void Newgame(int slotIndex)
    {
        _currentSlot = slotIndex;
        CurrentData = SaveData.Default;
        SyncMeta();
        Log.System(LocalizationKey.Log_Save_NewGameStarted, slotIndex);
    }

    public void SetDebugDefaultData()
    {
        _currentSlot = -1;
        CurrentData = SaveData.Default;
        Log.System(LocalizationKey.Log_Save_NewGameStarted, -1);
    }

    public void Sync()
    {
        if (CurrentData == null)
            CurrentData = SaveData.Default;

        if (Managers.Scene.CurrentSceneID >= SceneID.Hospital1)
            CurrentData.CurrentSceneID = Managers.Scene.CurrentSceneID;

        var player = Managers.Game.Character;

        if (player != null)
        {
            if (player.Attributes != null)
                CurrentData.SavedAttributes = player.Attributes.ExportSaveData();

            CurrentData.PlayerPosition = player.transform.position;
            CurrentData.PlayerRotation = player.transform.rotation.z;
        }
    }

    private string GetPath(int slot)
    {
        string dir = Literal.Folders.Saves.GetDirectory();
        string fileName = (slot == 0) ? ZString.Concat(Literal.Files.Save, "_", Literal.Files.Auto, Literal.Extensions.Data) : ZString.Concat(Literal.Files.Save, "_", slot, Literal.Extensions.Data);
        return Path.Combine(dir, fileName);
    }

    private string GetBackupPath(int slot)
    {
        string dir = Literal.Folders.Saves.GetDirectory();
        string backup = Path.Combine(dir, Literal.Folders.Backups).GetDirectory();
        string fileName = (slot == 0) ? ZString.Concat(Literal.Files.Save, "_", Literal.Files.Auto, Literal.Extensions.Backup) : ZString.Concat(Literal.Files.Save, "_", slot,  Literal.Extensions.Backup);
        return Path.Combine(backup, fileName);
    }

    private string GetMetaPath()
    {
        string dir = Literal.Folders.Saves.GetDirectory();
        return Path.Combine(dir, ZString.Concat(Literal.Files.Meta, Literal.Extensions.Data));
    }

    private string GetMetaBackupPath()
    {
        string dir = Literal.Folders.Saves.GetDirectory();
        string backup = Path.Combine(dir, Literal.Folders.Backups).GetDirectory();
        return Path.Combine(backup, ZString.Concat(Literal.Files.Meta, Literal.Extensions.Backup));
    }

    public async UniTask SwapSlotOrderAsync(int indexA, int indexB)
    {
        if (MetaData.SlotOrder == null)
            return;

        int posA = MetaData.SlotOrder.IndexOf(indexA);
        int posB = MetaData.SlotOrder.IndexOf(indexB);

        if (posA < 0 || posB < 0)
            return;

        int temp = MetaData.SlotOrder[posA];
        MetaData.SlotOrder[posA] = MetaData.SlotOrder[posB];
        MetaData.SlotOrder[posB] = temp;
        await SaveMetaAsync();
    }

    private bool FileExists(string path)
        => File.Exists(path);

    private bool IsSlotInvalid()
        => _currentSlot < 0;

    private void EnsureDirectoryExists(string dir)
    {
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    private void BackupExistingFile(string path, string backupPath)
    {
        if (File.Exists(path))
            File.Copy(path, backupPath, true);
    }

    private void DeleteFileIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    private void RestoreBackupIfNeeded(string path, string backupPath, int index = -1)
    {
        if (!File.Exists(path) && File.Exists(backupPath))
        {
            File.Copy(backupPath, path, true);

            if (index >= 0)
                Log.Warning(LocalizationKey.Log_Save_RestoredFromBackup, index);
        }
    }
}
