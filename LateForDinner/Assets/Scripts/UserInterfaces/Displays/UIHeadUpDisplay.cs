using LateForDinner.Data;
using R3;
using System.Collections.Generic;
using UnityEngine;

public class UIHeadUpDisplay : UIDisplay
{
    private enum RectTransforms
    {
        SlotContent,
        DashContent,
        HealthContent,
        TemporaryHealthContent
    }

    private enum Images
    {
        BossHealthImage,
        WeaponSlotImage
    }

    private enum Panels
    {
        BossPanel,
        SlotPanel,
        AttributePanel
    }

    private readonly List<UIQuickSlot> _quickSlots = new List<UIQuickSlot>();
    private readonly List<UIDashCountSlot> _dashSlots = new List<UIDashCountSlot>();
    private readonly List<UIRemainHealthSlot> _healthSlots = new List<UIRemainHealthSlot>();
    private readonly List<UIRemainHealthSlot> _temporaryHealthSlots = new List<UIRemainHealthSlot>();

    public override void OnInit()
    {
        base.OnInit();
        BindRectTransform(typeof(RectTransforms));
        BindImage(typeof(Images));
        BindPanel(typeof(Panels));
        InitQuickSlots();
    }

    private void InitQuickSlots()
    {
        var content = GetRectTransform(RectTransforms.SlotContent).transform;

        for (int index = 0; index < Define.Amount.MaxQuickSlot; index++)
        {
            var (slot, _) = Managers.Pool.Pop<UIQuickSlot>(content);

            if (slot != null)
                _quickSlots.Add(slot);
        }
    }

    public override void OnGet()
    {
        base.OnGet();
        SetQuickSlots();
        RegisterQuickSlotHotkeys();
        UpdateWeaponSlot();
        GetDashSlots();
        GetHealthSlots();
        GetTemporaryHealthSlots();
        Managers.Inventory.OnInventoryChanged
        .Subscribe(_ => 
        { 
            SetQuickSlots(); 
            UpdateWeaponSlot();
        }).RegisterToPool(this);
        var player = Managers.Game.Player;
        var dashAttribute = player.Attributes.GetBase<int>(AttributeType.DashCount);
        dashAttribute.AsObservable()
        .Skip(1)
        .Subscribe(this, (maxCount, hud) =>
        {
            hud.UpdateDashSlots(maxCount);
        }).RegisterToPool(this);
        var healthAttribute = player.Attributes.Get<int>(AttributeType.Health);
        var maxHealthAttribute = player.Attributes.GetBase<int>(AttributeType.Health);
        var temporaryHealthAttribute = player.Attributes.Get<int>(AttributeType.TemporaryHealth);
        var maxTemporaryHealthAttribute = player.Attributes.GetBase<int>(AttributeType.TemporaryHealth);
        Observable.CombineLatest(healthAttribute.AsObservable(), maxHealthAttribute.AsObservable(), temporaryHealthAttribute.AsObservable(), maxTemporaryHealthAttribute.AsObservable(),
        (health, maxHealth, temporaryHealth, maxTemporaryHealth) => (health, maxHealth, temporaryHealth, maxTemporaryHealth))
        .Skip(1)
        .Subscribe(this, (tuple, hud) =>
        {
            int healthSlotCount = Mathf.CeilToInt(tuple.maxHealth / 2f);
            hud.UpdateHealthSlots(healthSlotCount);
            int tempHealthSlotCount = Mathf.CeilToInt(tuple.maxTemporaryHealth / 2f);
            hud.UpdateTemporaryHealthSlots(tempHealthSlotCount);
        }).RegisterToPool(this);
    }

    private void SetQuickSlots()
    {
        var quickSlotsData = Managers.Inventory?.GetQuickSlots();

        for (int index = 0; index < _quickSlots.Count; index++)
        {
            InventorySlot slotData = (quickSlotsData != null && index < quickSlotsData.Count) ? quickSlotsData[index] : null;
            _quickSlots[index].Setup(index, slotData);
        }
    }

    private void RegisterQuickSlotHotkeys()
    {
        for (int index = 0; index < Define.Amount.MaxQuickSlot; index++)
        {
            int slotIndex = index;
            string hotkeyName = index switch
            {
                0 => Literal.Hotkeys.QuickSlot1,
                1 => Literal.Hotkeys.QuickSlot2,
                2 => Literal.Hotkeys.QuickSlot3,
                3 => Literal.Hotkeys.QuickSlot4,
                _ => string.Empty
            };

            if (string.IsNullOrEmpty(hotkeyName))
                continue;

            Managers.Control.Subscribe(this, hotkeyName, InputEventType.Triggered, () =>
            {
                Managers.Inventory.UseQuickSlot(slotIndex);
            }).RegisterToPool(this);
        }
    }

    private void UpdateWeaponSlot()
    {
        var weaponSlot = Managers.Inventory?.GetEquipmentSlotByType(EquipmentSlotType.Weapon);

        if (weaponSlot == null || weaponSlot.ItemID <= 0 || !weaponSlot.ItemID.TryGetValidItemData(out var itemData, out _))
        {
            GetImage(Images.WeaponSlotImage).SetActive(false);
            GetImage(Images.WeaponSlotImage).sprite = null;
            return;
        }

        GetImage(Images.WeaponSlotImage).SetActive(true);
        GetImage(Images.WeaponSlotImage).sprite = Managers.Resource.GetSprite(Define.Atlas.Item, itemData.AddressableKey);
    }

    private void GetDashSlots()
    {
        var player = Managers.Game.Player;
        var dashAttribute = player.Attributes.GetBase<int>(AttributeType.DashCount);
        UpdateDashSlots(dashAttribute.CurrentValue);
    }

    private void GetHealthSlots()
    {
        var player = Managers.Game.Player;
        var maxHealthAttribute = player.Attributes.GetBase<int>(AttributeType.Health);
        int initialSlotCount = Mathf.CeilToInt(maxHealthAttribute.CurrentValue / 2f);
        UpdateHealthSlots(initialSlotCount);
    }

    private void GetTemporaryHealthSlots()
    {
        var player = Managers.Game.Player;
        var maxTemporaryHealthAttribute = player.Attributes.GetBase<int>(AttributeType.TemporaryHealth);
        int initialSlotCount = Mathf.CeilToInt(maxTemporaryHealthAttribute.CurrentValue / 2f);
        UpdateTemporaryHealthSlots(initialSlotCount);
    }

    public override void Refresh()
    {
        base.Refresh();
        var player = Managers.Game.Player;
        var dashAttribute = player.Attributes.GetBase<int>(AttributeType.DashCount);
        UpdateDashSlots(dashAttribute.CurrentValue);
        var maxHealthAttribute = player.Attributes.GetBase<int>(AttributeType.Health);
        int initialHealthSlotCount = Mathf.CeilToInt(maxHealthAttribute.CurrentValue / 2f);
        UpdateHealthSlots(initialHealthSlotCount);
        var maxTemporaryHealthAttribute = player.Attributes.GetBase<int>(AttributeType.TemporaryHealth);
        int initialTemporaryHealthSlotCount = Mathf.CeilToInt(maxTemporaryHealthAttribute.CurrentValue / 2f);
        UpdateTemporaryHealthSlots(initialTemporaryHealthSlotCount);
    }

    private void UpdateDashSlots(int maxDashCount)
    {
        var content = GetRectTransform(RectTransforms.DashContent).transform;

        while (_dashSlots.Count < maxDashCount)
        {
            var (slot, _) = Managers.Pool.Pop<UIDashCountSlot>(content);

            if (slot != null)
            {
                slot.SetDashSlot(_dashSlots.Count);
                _dashSlots.Add(slot);
            }
            else 
                break;
        }

        while (_dashSlots.Count > maxDashCount)
        {
            int lastIndex = _dashSlots.Count - 1;
            var slot = _dashSlots[lastIndex];
            _dashSlots.RemoveAt(lastIndex);
            Managers.Pool.Push(slot);
        }
    }

    private void UpdateHealthSlots(int maxHealthCount)
    {
        var content = GetRectTransform(RectTransforms.HealthContent).transform;

        while (_healthSlots.Count < maxHealthCount)
        {
            var (slot, _) = Managers.Pool.Pop<UIRemainHealthSlot>(content);

            if (slot != null)
            {
                slot.SetHealthSlot(_healthSlots.Count, UIRemainHealthSlot.UI_HealthSlotType.Normal);
                _healthSlots.Add(slot);
            }
            else
                break;
        }

        while (_healthSlots.Count > maxHealthCount)
        {
            int lastIndex = _healthSlots.Count - 1;
            var slot = _healthSlots[lastIndex];
            _healthSlots.RemoveAt(lastIndex);
            Managers.Pool.Push(slot);
        }

        GetRectTransform(RectTransforms.TemporaryHealthContent).SetAsLastSibling();
    }

    private void UpdateTemporaryHealthSlots(int maxTemporaryHealthCount)
    {
        var content = GetRectTransform(RectTransforms.TemporaryHealthContent).transform;

        while (_temporaryHealthSlots.Count < maxTemporaryHealthCount)
        {
            var (slot, _) = Managers.Pool.Pop<UIRemainHealthSlot>(content);

            if (slot != null)
            {
                slot.SetHealthSlot(_temporaryHealthSlots.Count, UIRemainHealthSlot.UI_HealthSlotType.Temporary);
                _temporaryHealthSlots.Add(slot);
            }
            else
                break;
        }

        while (_temporaryHealthSlots.Count > maxTemporaryHealthCount)
        {
            int lastIndex = _temporaryHealthSlots.Count - 1;
            var slot = _temporaryHealthSlots[lastIndex];
            _temporaryHealthSlots.RemoveAt(lastIndex);
            Managers.Pool.Push(slot);
        }
    }
}
