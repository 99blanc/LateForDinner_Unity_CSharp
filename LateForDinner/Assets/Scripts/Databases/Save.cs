using MemoryPack;
using System.Collections.Generic;

[MemoryPackable]
public partial class Slot
{
    public int Day = 1;
    public int Hour;
    public int Minute;
    public int Second;
    public MealTime Meal = MealTime.Breakfast;
    public int Year;
    public int Month;
    public int Date;
}

[MemoryPackable]
public partial class SaveMeta
{
    public List<SlotMeta> Slots = new List<SlotMeta>();
    public List<int> SlotOrder = new List<int>();
}

[MemoryPackable]
public partial class SlotMeta : Slot
{
    public bool IsActive = false;
}

[MemoryPackable]
public partial class Save : Slot
{
    // TODO ::: 플레이어 위치, 퀘스트 목록, 인벤토리 등 게임 플레이 데이터 적재
}
