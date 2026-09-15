using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZLinq;

namespace LateForDinner.Data
{
    [MemoryPackable]
    public partial class AttributeSaveData
    {
        public string Key;
        public string DataType;
        public string BaseValue;
        public string CurrentValue;
    }

    [MemoryPackable]
    public partial class EquipmentInstance
    {
        public string InstanceID;
        public int ItemID;
        public int UpgradeLevel;
        public int ExtraOptionValue;
        public bool Flag;

        public static EquipmentInstance Default => new EquipmentInstance()
        {
            InstanceID = Guid.NewGuid().ToString(),
            ItemID = 0,
            UpgradeLevel = 0,
            ExtraOptionValue = 0,
            Flag = false,
        };
    }

    [MemoryPackable]
    public partial class InventorySlot
    {
        public int SlotIndex;
        public int ItemID;
        public int Quantity;
        public string InstanceID;
    }

    [MemoryPackable]
    public partial class ActiveBuffSaveData
    {
        public int ItemID;
        public string AttributeKey;
        public string Value;
        public float RemainingTime;
        public int RemainingTicks;
    }

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
        public bool PlayerFlipX;
        public int InventoryTabCapacity;
        public List<InventorySlot> EquipmentTabSlots;
        public List<InventorySlot> ConsumptionTabSlots;
        public List<InventorySlot> EtcTabSlots;
        public List<InventorySlot> EquipmentSlots;
        public List<InventorySlot> QuickSlots;
        public List<EquipmentInstance> UnlockedEquipments;
        public HashSet<string> AppliedFlagItems;
        public List<ActiveBuffSaveData> ActiveBuffs;
        public float Gold;

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
            SavedAttributes = CharacterID.Protagonist.CreateDefaultAttributes(),
            PlayerPosition = Vector2.zero,
            PlayerFlipX = false,
            InventoryTabCapacity = Define.Amount.DefaultInventorySlot,
            EquipmentTabSlots = Enumerable.Range(0, Define.Amount.DefaultInventorySlot).Select(i => new InventorySlot { SlotIndex = i, ItemID = 0, Quantity = 0 }).ToList(),
            ConsumptionTabSlots = Enumerable.Range(0, Define.Amount.DefaultInventorySlot).Select(i => new InventorySlot { SlotIndex = i, ItemID = 0, Quantity = 0 }).ToList(),
            EtcTabSlots = Enumerable.Range(0, Define.Amount.DefaultInventorySlot).Select(i => new InventorySlot { SlotIndex = i, ItemID = 0, Quantity = 0 }).ToList(),
            EquipmentSlots = Enumerable.Range(0, Define.Amount.MaxEquipmentSlot).Select(i => new InventorySlot { SlotIndex = i, ItemID = 0, Quantity = 0 }).ToList(),
            QuickSlots = Enumerable.Range(0, Define.Amount.MaxQuickSlot).Select(i => new InventorySlot { SlotIndex = i, ItemID = 0, Quantity = 0 }).ToList(),
            UnlockedEquipments = new List<EquipmentInstance>(),
            AppliedFlagItems = new HashSet<string>(),
            ActiveBuffs = new List<ActiveBuffSaveData>(),
            Gold = 0f
        };
    }
}
