using Cysharp.Threading.Tasks;
using MemoryPack;
using System.Collections.Generic;
using System.IO;

public class SaveManager
{
    private int _currentSlot = -1;
    public int CurrentSlot => _currentSlot;
    public SaveMeta MetaData { get; private set; } = new SaveMeta();
    public Save CurrentData { get; private set; } = new Save();

    public async UniTask InitAsync()
        => await MetaAsync();

    public async UniTask LoadAsync(int index)
    {
        string path = GetPath(index);
        string backupPath = GetBackupPath(index);

        if (!File.Exists(path) && File.Exists(backupPath))
            File.Copy(backupPath, path, true);

        try
        {
            if (File.Exists(path))
            {
                byte[] bytes = await File.ReadAllBytesAsync(path);
                CurrentData = MemoryPackSerializer.Deserialize<Save>(bytes) ?? new Save();
                _currentSlot = index;
            }
            else
                NewGame(index);
        }
        catch
        {
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
        }
        catch
        {
            // DESC ::: 슬롯 저장에 실패한 경우
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
            if (File.Exists(path))
            {
                byte[] bytes = await File.ReadAllBytesAsync(path);
                MetaData = MemoryPackSerializer.Deserialize<SaveMeta>(bytes) ?? new SaveMeta();
            }
            else
                MetaData = new SaveMeta();
        }
        catch
        {
            MetaData = new SaveMeta();
        }

        ValidateMeta();
    }

    public void EnsureSlot(int count)
    {
        if (MetaData.Slots == null)
            MetaData.Slots = new List<SlotMeta>();

        if (MetaData.SlotOrder == null)
            MetaData.SlotOrder = new List<int>();

        while (MetaData.Slots.Count < count)
            MetaData.Slots.Add(new SlotMeta());

        if (MetaData.SlotOrder.Count != MetaData.Slots.Count)
        {
            MetaData.SlotOrder.Clear();

            for (int index = 0; index < MetaData.Slots.Count; index++)
                MetaData.SlotOrder.Add(index);
        }
    }

    private void ValidateMeta()
    {
        if (MetaData.Slots == null)
            MetaData.Slots = new List<SlotMeta>();

        if (MetaData.SlotOrder == null)
            MetaData.SlotOrder = new List<int>();

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

    private void SyncMeta()
    {
        if (MetaData.Slots == null)
            MetaData.Slots = new List<SlotMeta>();

        if (MetaData.Slots[_currentSlot] == null)
            MetaData.Slots[_currentSlot] = new SlotMeta();

        var meta = MetaData.Slots[_currentSlot];
        meta.Day = CurrentData.Day;
        meta.Hour = CurrentData.Hour;
        meta.Minute = CurrentData.Minute;
        meta.Second = CurrentData.Second;
        meta.Meal = CurrentData.Meal;
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
            /// DESC ::: 예외 발생 시 무시
        }
    }

    public void Select(int index)
        => _currentSlot = index;

    public void Clear(int index)
    {
        EnsureSlot(index + 1);
        MetaData.Slots[index] = new SlotMeta();
        CurrentData = new Save();
        _currentSlot = index;
        SaveAsync().Forget();
    }

    public void NewGame(int slotIndex)
    {
        _currentSlot = slotIndex;
        CurrentData = new Save();
        SyncMeta();
    }

    private string GetPath(int slot)
    {
        string dir = Literal.Folders.Saves.GetDirectory();

        return Path.Combine(dir, $"{Literal.Files.Save}_{slot}{Literal.Extensions.Data}");
    }

    private string GetBackupPath(int slot)
    {
        string dir = Literal.Folders.Saves.GetDirectory();
        string backup = Path.Combine(dir, Literal.Folders.Backups).GetDirectory();

        return Path.Combine(backup, $"{Literal.Files.Save}_{slot}{Literal.Extensions.Backup}");
    }

    private string GetMetaPath()
    {
        string dir = Literal.Folders.Saves.GetDirectory();

        return Path.Combine(dir, $"{Literal.Files.Meta}_{Literal.Extensions.Data}");
    }

    private string GetMetaBackupPath()
    {
        string dir = Literal.Folders.Saves.GetDirectory();
        string backup = Path.Combine(dir, Literal.Folders.Backups).GetDirectory();

        return Path.Combine(backup, $"{Literal.Files.Meta}_{Literal.Extensions.Backup}");
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
