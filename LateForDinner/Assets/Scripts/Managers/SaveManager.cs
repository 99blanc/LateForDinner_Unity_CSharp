using Cysharp.Threading.Tasks;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.IO;

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

        if (!File.Exists(path) && File.Exists(backupPath))
        {
            File.Copy(backupPath, path, true);
            Log.Warning(LocalizationKey.Log_Save_RestoredFromBackup, index);
        }

        try
        {
            if (!File.Exists(path))
            {
                NewGame(index);
                return;
            }

            byte[] bytes = await File.ReadAllBytesAsync(path);
            CurrentData = MemoryPackSerializer.Deserialize<SaveData>(bytes) ?? new SaveData();
            _currentSlot = index;
            Log.System(LocalizationKey.Log_Save_LoadSuccess, index);
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Save_LoadFailed, index);
            NewGame(index);
        }
    }

    public async UniTask SaveAsync()
    {
        if (_currentSlot < 0)
            return;

        string path = GetPath(_currentSlot);
        string backupPath = GetBackupPath(_currentSlot);

        try
        {
            SyncMeta();
            byte[] bytes = MemoryPackSerializer.Serialize(CurrentData);
            string dir = Path.GetDirectoryName(path);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(path))
                File.Copy(path, backupPath, true);

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

        if (!File.Exists(path) && File.Exists(backupPath))
            File.Copy(backupPath, path, true);

        try
        {
            if (!File.Exists(path))
            {
                MetaData = new SaveMeta();
                ValidateMeta();
                return;
            }

            byte[] bytes = await File.ReadAllBytesAsync(path);
            MetaData = MemoryPackSerializer.Deserialize<SaveMeta>(bytes) ?? new SaveMeta();
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Save_MetaLoadFailed);
            MetaData = new SaveMeta();
        }

        ValidateMeta();
    }

    public void EnsureSlot(int count)
    {
        InitMetaCollectionsIfNeeded();

        while (MetaData.Slots.Count < count)
            MetaData.Slots.Add(new SlotMeta());

        if (MetaData.SlotOrder.Count == MetaData.Slots.Count)
            return;

        MetaData.SlotOrder.Clear();

        for (int ndex = 0; ndex < MetaData.Slots.Count; ndex++)
            MetaData.SlotOrder.Add(ndex);
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
                MetaData.Slots[index] = new SlotMeta();
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
            MetaData.Slots[_currentSlot] = new SlotMeta();

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

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(path))
                File.Copy(path, backupPath, true);

            await File.WriteAllBytesAsync(path, bytes);
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Save_MetaSaveFailed);
        }
    }

    public void Select(int index) =>
        _currentSlot = index;

    public async UniTask ClearAsync(int index)
    {
        EnsureSlot(index + 1);
        MetaData.Slots[index] = new SlotMeta();
        CurrentData = new SaveData();
        _currentSlot = index;
        await SaveMetaAsync();
        string path = GetPath(index);

        if (File.Exists(path))
            File.Delete(path);

        Log.System(LocalizationKey.Log_Save_ClearSuccess, index);
    }

    public void NewGame(int slotIndex)
    {
        _currentSlot = slotIndex;
        CurrentData = new SaveData();
        SyncMeta();
        Log.System(LocalizationKey.Log_Save_NewGameStarted, slotIndex);
    }

    private string GetPath(int slot)
    {
        string dir = Literal.Folders.Saves.GetDirectory();
        string fileName = (slot == 0) ? $"{Literal.Files.Save}_{Literal.Files.Auto}{Literal.Extensions.Data}" : $"{Literal.Files.Save}_{slot}{Literal.Extensions.Data}";
        return Path.Combine(dir, fileName);
    }

    private string GetBackupPath(int slot)
    {
        string dir = Literal.Folders.Saves.GetDirectory();
        string backup = Path.Combine(dir, Literal.Folders.Backups).GetDirectory();
        string fileName = (slot == 0) ? $"{Literal.Files.Save}_{Literal.Files.Auto}{Literal.Extensions.Backup}" : $"{Literal.Files.Save}_{slot}{Literal.Extensions.Backup}";
        return Path.Combine(backup, fileName);
    }

    private string GetMetaPath()
    {
        string dir = Literal.Folders.Saves.GetDirectory();
        return Path.Combine(dir, $"{Literal.Files.Meta}{Literal.Extensions.Data}");
    }

    private string GetMetaBackupPath()
    {
        string dir = Literal.Folders.Saves.GetDirectory();
        string backup = Path.Combine(dir, Literal.Folders.Backups).GetDirectory();
        return Path.Combine(backup, $"{Literal.Files.Meta}{Literal.Extensions.Backup}");
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
}
