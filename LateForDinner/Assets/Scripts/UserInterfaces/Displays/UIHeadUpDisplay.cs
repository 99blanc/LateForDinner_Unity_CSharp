using R3;
using System.Collections.Generic;

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

    private readonly List<UIDashCountSlot> _dashSlots = new List<UIDashCountSlot>();
    private readonly List<UIRemainHealthSlot> _healthSlots = new List<UIRemainHealthSlot>();
    private readonly List<UIRemainHealthSlot> _temporaryHealthSlots = new List<UIRemainHealthSlot>();

    public override void OnInit()
    {
        base.OnInit();
        BindRectTransform(typeof(RectTransforms));
        BindImage(typeof(Images));
        BindPanel(typeof(Panels));
        InitDashSlots();
        InitHealthSlots();
        InitTempHealthSlots();
    }

    private void InitDashSlots()
    {
        var player = Managers.Game.Player;

        if (player == null)
            return;

        var dashAttribute = player.Attributes.Get<int>(AttributeType.DashCount);
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
        int initialSlotCount = maxHealthAttribute.CurrentValue / 2;
        UpdateHealthSlots(initialSlotCount, player);
        maxHealthAttribute.AsObservable()
        .Skip(1)
        .Subscribe(this, (maxHealth, hud) =>
        {
            int totalSlotCount = maxHealth / 2;
            hud.UpdateHealthSlots(totalSlotCount, player);
        }).RegisterToPool(this);
    }

    private void InitTempHealthSlots()
    {
        var player = Managers.Game.Player;

        if (player == null) 
            return;

        var maxTempHealthAttribute = player.Attributes.GetBase<int>(AttributeType.TemporaryHealth);
        int initialSlotCount = maxTempHealthAttribute.CurrentValue / 2;
        UpdateTempHealthSlots(initialSlotCount, player);
        maxTempHealthAttribute.AsObservable()
        .Skip(1)
        .Subscribe(this, (maxTempHealth, hud) =>
        {
            int totalSlotCount = maxTempHealth / 2;
            hud.UpdateTempHealthSlots(totalSlotCount, player);
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
            {
                Managers.Pool.Push(slot);
            }
        }
    }

    private void UpdateHealthSlots(int maxHealthCount, PlayableCharacter player)
    {
        var content = GetRectTransform(RectTransforms.HealthContent).transform;

        while (_healthSlots.Count < maxHealthCount)
        {
            var (slot, _) = Managers.Pool.Pop<UIRemainHealthSlot>(content);

            if (slot != null)
                slot.InitHealthSlot(player, _healthSlots.Count, UIRemainHealthSlot.UI_HealthSlotType.Normal);
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

    private void UpdateTempHealthSlots(int maxTemporaryHealthCount, PlayableCharacter player)
    {
        var content = GetRectTransform(RectTransforms.TemporaryHealthContent).transform;

        while (_temporaryHealthSlots.Count < maxTemporaryHealthCount)
        {
            var (slot, _) = Managers.Pool.Pop<UIRemainHealthSlot>(content);

            if (slot != null)
                slot.InitHealthSlot(player, _temporaryHealthSlots.Count, UIRemainHealthSlot.UI_HealthSlotType.Temporary);
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
