using R3;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UIHeadUpDisplay : UIDisplay
{
    private enum RectTransforms
    {
        SlotContent,
        DashContent,
        HealthContent
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

    private UIQuickSlot[] _quickSlots;
    private List<UIDashCountSlot> _dashSlots = new List<UIDashCountSlot>();
    private List<UIRemainHealthSlot> _healthSlots = new List<UIRemainHealthSlot>();
    private List<UIRemainHealthSlot> _temporaryHealthSlots = new List<UIRemainHealthSlot>();

    public override void OnInit()
    {
        base.OnInit();
        BindRectTransform(typeof(RectTransforms));
        BindImage(typeof(Images));
        BindPanel(typeof(Panels));
        GetPanel(Panels.BossPanel).SetActivePanel(false);
        GetPanel(Panels.SlotPanel).SetActivePanel(true);
        GetPanel(Panels.AttributePanel).SetActivePanel(true);
        InitQuickSlots();
    }

    public override void OnGet()
    {
        base.OnGet();
        Refresh();
    }

    public override void Refresh()
    {
        base.Refresh();
        InitDashSlots();
        InitHealthSlots();
    }

    private void InitQuickSlots()
    {
        int maxQuickSlots = Define.Amount.MaxQuickSlot;
        _quickSlots = new UIQuickSlot[maxQuickSlots];
        var content = GetRectTransform(RectTransforms.SlotContent).transform;

        for (int index = 0; index < maxQuickSlots; index++)
        {
            var (slot, _) = Managers.Pool.Pop<UIQuickSlot>(content);
            _quickSlots[index] = slot;
        }
    }

    private void InitDashSlots()
    {
        var character = Managers.Game.Character;

        if (character == null || character.Attributes == null)
            return;

        var attributes = character.Attributes;
        int maxDashCount = attributes.GetBase<int>(AttributeType.DashCount).CurrentValue;
        var dashStream = attributes.Stream<int>(AttributeType.DashCount);
        var content = GetRectTransform(RectTransforms.DashContent).transform;

        foreach (var slot in _dashSlots)
        {
            if (slot != null)
                Managers.Pool.Push(slot);
        }

        _dashSlots.Clear();

        for (int index = 0; index < maxDashCount; index++)
        {
            var (slot, _) = Managers.Pool.Pop<UIDashCountSlot>(content);

            if (slot != null)
            {
                _dashSlots.Add(slot);
                slot.SetIndex(index);
                slot.ForceSetState(dashStream.CurrentValue);
            }
        }

        dashStream
        .Subscribe(currentDash =>
        {
            for (int index = 0; index < _dashSlots.Count; index++)
            {
                if (_dashSlots[index] != null)
                    _dashSlots[index].UpdateState(currentDash);
            }
        }).RegisterToPool(this);
    }

    private void InitHealthSlots()
    {
        var character = Managers.Game.Character;

        if (character == null || character.Attributes == null)
            return;

        var attributes = character.Attributes;
        int maxHealth = attributes.GetBase<int>(AttributeType.Health).CurrentValue;
        int maxTempHealth = attributes.GetBase<int>(AttributeType.TemporaryHealth).CurrentValue;
        int normalSlotCount = Mathf.CeilToInt(maxHealth / 2f);
        int tempSlotCount = Mathf.CeilToInt(maxTempHealth / 2f);
        var healthStream = attributes.Stream<int>(AttributeType.Health);
        var tempHealthStream = attributes.Stream<int>(AttributeType.TemporaryHealth);
        var content = GetRectTransform(RectTransforms.HealthContent).transform;

        foreach (var slot in _healthSlots)
        {
            if (slot != null)
                Managers.Pool.Push(slot);
        }

        _healthSlots.Clear();

        foreach (var slot in _temporaryHealthSlots)
        {
            if (slot != null)
                Managers.Pool.Push(slot);
        }

        _temporaryHealthSlots.Clear();

        for (int index = 0; index < normalSlotCount; index++)
        {
            var (slot, _) = Managers.Pool.Pop<UIRemainHealthSlot>(content);

            if (slot != null)
            {
                _healthSlots.Add(slot);
                slot.SetIndex(index);
                slot.UpdateHealthState(healthStream.CurrentValue);
            }
        }

        for (int index = 0; index < tempSlotCount; index++)
        {
            var (slot, _) = Managers.Pool.Pop<UIRemainHealthSlot>(content);

            if (slot != null)
            {
                _temporaryHealthSlots.Add(slot);
                slot.SetIndex(normalSlotCount + index);
                slot.UpdateTempHealthState(tempHealthStream.CurrentValue);
            }
        }

        healthStream
        .Subscribe(currentHealth =>
        {
            for (int index = 0; index < _healthSlots.Count; index++)
            {
                if (_healthSlots[index] != null)
                    _healthSlots[index].UpdateHealthState(currentHealth);
            }
        }).RegisterToPool(this);
        tempHealthStream
        .Subscribe(currentTempHealth =>
        {
            for (int index = 0; index < _temporaryHealthSlots.Count; index++)
            {
                if (_temporaryHealthSlots[index] != null)
                    _temporaryHealthSlots[index].UpdateTempHealthState(currentTempHealth);
            }
        }).RegisterToPool(this);
    }

    public void BindBossHealth(Character bossCharacter)
    {
        if (bossCharacter == null)
        {
            SetBossHealthActive(false);
            CancelToken("BossHealth");
            return;
        }

        SetBossHealthActive(true);
        var attributes = bossCharacter.Attributes;
        var currentHealthStream = attributes.Stream<int>(AttributeType.Health);
        var maxHealth = attributes.GetBase<int>(AttributeType.Health).CurrentValue;
        var bossHealthImg = GetImage(Images.BossHealthImage);

        if (bossHealthImg != null && maxHealth > 0)
            bossHealthImg.fillAmount = Mathf.Clamp01((float)currentHealthStream.CurrentValue / maxHealth);

        currentHealthStream
        .Subscribe(currentHealth =>
        {
            UpdateBossHealthBarAsync(currentHealth, maxHealth, GetToken("BossHealth")).Forget();
        })
        .RegisterToPool(this);
    }

    private async UniTaskVoid UpdateBossHealthBarAsync(int currentHealth, int maxHealth, CancellationToken token)
    {
        if (maxHealth <= 0)
            return;

        var bossHealthImg = GetImage(Images.BossHealthImage);

        if (bossHealthImg == null)
            return;

        float targetFill = Mathf.Clamp01((float)currentHealth / maxHealth);
        await bossHealthImg.SmoothDampFillAmountAsync(targetFill, smoothTime: 0.25f, delay: 0.1f, token: token);
    }

    public void SetBossHealthActive(bool isActive)
        => GetPanel(Panels.BossPanel).SetActivePanel(isActive);

    public void SetPlayerAttributeActive(bool isActive)
        => GetPanel(Panels.AttributePanel).SetActivePanel(isActive);

    public void SetPlayerQuickSlotActive(bool isActive)
        => GetPanel(Panels.SlotPanel).SetActivePanel(isActive);
}
