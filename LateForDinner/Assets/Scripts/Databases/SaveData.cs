using MemoryPack;
using System.Collections.Generic;
using UnityEngine;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class Slot
    {
        public int Day;
        public int Hour;
        public int Minute;
        public int Second;
        public MealTime Meal;
        public int Year;
        public int Month;
        public int Date;

        [MemoryPackIgnore]
        public static Slot Default => new Slot()
        {
            Day = 1,
            Hour = 0,
            Minute = 0,
            Second = 0,
            Meal = MealTime.Breakfast,
            Year = 1,
            Month = 1,
            Date = 1
        };
    }

    [MemoryPackable]
    public partial class SaveMeta
    {
        public List<SlotMeta> Slots;
        public List<int> SlotOrder;
        public List<CharacterID> UnlockedCharacters;

        [MemoryPackIgnore]
        public static SaveMeta Default => new SaveMeta()
        {
            Slots = new List<SlotMeta>(),
            SlotOrder = new List<int>(),
            UnlockedCharacters = new List<CharacterID>() { CharacterID.Protagonist }
        };
    }

    [MemoryPackable]
    public partial class SlotMeta : Slot
    {
        public bool IsActive;

        [MemoryPackIgnore]
        public new static SlotMeta Default => new SlotMeta()
        {
            Day = 1,
            Hour = 0,
            Minute = 0,
            Second = 0,
            Meal = MealTime.Breakfast,
            Year = 1,
            Month = 1,
            Date = 1,
            IsActive = false
        };
    }

    [MemoryPackable]
    public partial class SaveData : Slot
    {
        // DESC ::: 플레이어 위치, 퀘스트 목록, 인벤토리 등 게임 플레이 데이터 적재
        public CharacterID SelectedPlayerID;
        public SceneID CurrentSceneID;
        public Dictionary<string, bool> InteractableStates;
        public List<AttributeSaveData> SavedAttributes;
        public Vector2 PlayerPosition;
        public float PlayerRotation;

        [MemoryPackIgnore]
        public new static SaveData Default => new SaveData()
        {
            Day = 1,
            Hour = 0,
            Minute = 0,
            Second = 0,
            Meal = MealTime.Breakfast,
            Year = 1,
            Month = 1,
            Date = 1,
            SelectedPlayerID = CharacterID.Protagonist,
            CurrentSceneID = SceneID.Hospital1,
            InteractableStates = new Dictionary<string, bool>(),
            SavedAttributes= new List<AttributeSaveData>(),
            PlayerPosition = Vector2.zero,
            PlayerRotation = 0f
        };
    }

    [MemoryPackable]
    public partial class AttributeSaveData
    {
        public string Key;
        public string DataType;
        public double Value;
    }
}
