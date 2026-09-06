using LateForDinner.Data;
using System.Collections.Generic;
using System.Linq;
using ZLinq;

public class InventoryManager
{
    private List<InventorySlot> _slots = new List<InventorySlot>(Define.Amount.MaxInventorySlot);
    private List<InventorySlot> _equipmentSlots = new List<InventorySlot>(Define.Amount.MaxEquipmentSlot);
    private List<InventorySlot> _quickSlots = new List<InventorySlot>(Define.Amount.MaxQuickSlot);

    public void InitInventory(List<InventorySlot> savedSlots, List<InventorySlot> savedQuickSlots, List<InventorySlot> savedEquipmentSlots = null)
    {
        _slots = savedSlots ?? new List<InventorySlot>();

        while (_slots.Count < Define.Amount.MaxInventorySlot)
        {
            int index = _slots.Count;
            _slots.Add(new InventorySlot { SlotIndex = index, ItemID = 0, Quantity = 0 });
        }

        _equipmentSlots = savedEquipmentSlots ?? new List<InventorySlot>();

        while (_equipmentSlots.Count < Define.Amount.MaxEquipmentSlot)
        {
            int index = _equipmentSlots.Count;
            _equipmentSlots.Add(new InventorySlot { SlotIndex = index, ItemID = 0, Quantity = 0 });
        }

        _quickSlots = savedQuickSlots ?? new List<InventorySlot>();

        while (_quickSlots.Count < Define.Amount.MaxQuickSlot)
        {
            int index = _quickSlots.Count;
            _quickSlots.Add(new InventorySlot { SlotIndex = index, ItemID = 0, Quantity = 0 });
        }
    }

    public IEnumerable<InventorySlot> GetSlotsByType(ItemType? type)
    {
        if (!type.HasValue)
            return _slots;

        int tabSize = Define.Amount.InventoryTabSize;
        int startIndex = type.Value switch
        {
            ItemType.Equipment => tabSize * 0,
            ItemType.Consumption => tabSize * 1,
            ItemType.Etc => tabSize * 2,
            _ => 0
        };

        return _slots.Skip(startIndex).Take(20);
    }

    public IReadOnlyList<InventorySlot> GetQuickSlots() 
        => _quickSlots;

    public IReadOnlyList<InventorySlot> GetEquipmentSlots()
        => _equipmentSlots;

    public List<InventorySlot> ExportSaveData()
        => _slots;

    public List<InventorySlot> ExportQuickSlotSaveData()
        => _quickSlots;

    public List<InventorySlot> ExportEquipmentSlotSaveData()
        => _equipmentSlots;
}
