using R3;

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

    private int _currentCreatedDashSlots = 0;
    private int _currentCreatedHealthSlots = 0;
    private int _currentCreatedTempHealthSlots = 0;

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

        while (_currentCreatedDashSlots < maxDashCount)
        {
            var (slot, _) = Managers.Pool.Pop<UIDashCountSlot>(content);

            if (slot != null)
                slot.InitDashSlot(player, _currentCreatedDashSlots);

            _currentCreatedDashSlots++;
        }
    }

    private void UpdateHealthSlots(int maxHealthCount, PlayableCharacter player)
    {
        var content = GetRectTransform(RectTransforms.HealthContent).transform;

        while (_currentCreatedHealthSlots < maxHealthCount)
        {
            var (slot, _) = Managers.Pool.Pop<UIRemainHealthSlot>(content);

            if (slot != null)
                slot.InitHealthSlot(player, _currentCreatedHealthSlots, UIRemainHealthSlot.UI_HealthSlotType.Normal);

            _currentCreatedHealthSlots++;
        }

        GetRectTransform(RectTransforms.TemporaryHealthContent).SetAsLastSibling();
    }

    private void UpdateTempHealthSlots(int maxTempHealthCount, PlayableCharacter player)
    {
        var content = GetRectTransform(RectTransforms.TemporaryHealthContent).transform;

        while (_currentCreatedTempHealthSlots < maxTempHealthCount)
        {
            var (slot, _) = Managers.Pool.Pop<UIRemainHealthSlot>(content);

            if (slot != null)
                slot.InitHealthSlot(player, _currentCreatedTempHealthSlots, UIRemainHealthSlot.UI_HealthSlotType.Temporary);

            _currentCreatedTempHealthSlots++;
        }
    }
}
