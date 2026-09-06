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
        InitDashSlots();
        InitHealthSlots();
        InitTemporaryHealthSlots();
    }

    private void InitQuickSlots()
    {
        var content = GetRectTransform(RectTransforms.SlotContent).transform;
        var quickSlotsData = Managers.Inventory?.GetQuickSlots();

        for (int index = 0; index < Define.Amount.MaxQuickSlot; index++)
        {
            var (slot, _) = Managers.Pool.Pop<UIQuickSlot>(content);

            if (slot != null)
            {
                InventorySlot slotData = (quickSlotsData != null && index < quickSlotsData.Count) ? quickSlotsData[index] : null;
                slot.Setup(index, slotData);
                _quickSlots.Add(slot);
            }
        }
    }

    private void InitDashSlots()
    {
        var player = Managers.Game.Player;

        if (player == null)
            return;

        var dashAttribute = player.Attributes.GetBase<int>(AttributeType.DashCount);
        UpdateDashSlots(dashAttribute.CurrentValue, player);
        dashAttribute.AsObservable()
        .Skip(1)
        .Subscribe(this, (maxCount, hud) =>
        {
            hud.UpdateDashSlots(maxCount, player);
        }).RegisterToPool(this);
    }

    private void InitHealthSlots()
    {
        var player = Managers.Game.Player;
        if (player == null) return;

        var maxHealthAttribute = player.Attributes.GetBase<int>(AttributeType.Health);
        int initialSlotCount = Mathf.CeilToInt(maxHealthAttribute.CurrentValue / 2f);
        UpdateHealthSlots(initialSlotCount, player);
        maxHealthAttribute.AsObservable()
        .Skip(1)
        .Subscribe(this, (maxHealth, hud) =>
        {
            int totalSlotCount = Mathf.CeilToInt(maxHealth / 2f);
            hud.UpdateHealthSlots(totalSlotCount, player);
        }).RegisterToPool(this);
    }

    private void InitTemporaryHealthSlots()
    {
        var player = Managers.Game.Player;

        if (player == null) 
            return;

        var maxTempHealthAttribute = player.Attributes.GetBase<int>(AttributeType.TemporaryHealth);
        int initialSlotCount = Mathf.CeilToInt(maxTempHealthAttribute.CurrentValue / 2f);
        UpdateTemporaryHealthSlots(initialSlotCount, player);
        maxTempHealthAttribute.AsObservable()
        .Skip(1)
        .Subscribe(this, (maxTempHealth, hud) =>
        {
            int totalSlotCount = Mathf.CeilToInt(maxTempHealth / 2f);
            hud.UpdateTemporaryHealthSlots(totalSlotCount, player);
        }).RegisterToPool(this);
    }

    private void UpdateDashSlots(int maxDashCount, PlayableCharacter player)
    {
        var content = GetRectTransform(RectTransforms.DashContent).transform;

        while (_dashSlots.Count < maxDashCount)
        {
            var (slot, _) = Managers.Pool.Pop<UIDashCountSlot>(content);

            if (slot != null)
            {
                slot.InitDashSlot(player, _dashSlots.Count);
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

            if (slot != null)
                Managers.Pool.Push(slot);
        }
    }

    private void UpdateHealthSlots(int maxHealthCount, PlayableCharacter player)
    {
        var content = GetRectTransform(RectTransforms.HealthContent).transform;

        while (_healthSlots.Count < maxHealthCount)
        {
            var (slot, _) = Managers.Pool.Pop<UIRemainHealthSlot>(content);

            if (slot != null)
            {
                slot.InitHealthSlot(player, _healthSlots.Count, UIRemainHealthSlot.UI_HealthSlotType.Normal);
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

            if (slot != null)
                Managers.Pool.Push(slot);
        }

        GetRectTransform(RectTransforms.TemporaryHealthContent).SetAsLastSibling();
    }

    private void UpdateTemporaryHealthSlots(int maxTemporaryHealthCount, PlayableCharacter player)
    {
        var content = GetRectTransform(RectTransforms.TemporaryHealthContent).transform;

        while (_temporaryHealthSlots.Count < maxTemporaryHealthCount)
        {
            var (slot, _) = Managers.Pool.Pop<UIRemainHealthSlot>(content);

            if (slot != null)
            {
                slot.InitHealthSlot(player, _temporaryHealthSlots.Count, UIRemainHealthSlot.UI_HealthSlotType.Temporary);
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

            if (slot != null)
                Managers.Pool.Push(slot);
        }
    }
}
