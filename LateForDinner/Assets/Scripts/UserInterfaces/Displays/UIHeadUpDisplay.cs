using R3;
using System.Threading;
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
    private UIDashCountSlot[] _dashSlots;
    private UIRemainHealthSlot[] _healthSlots;

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
        InitDashSlots();
        InitHealthSlots();
    }

    private void InitQuickSlots()
    {
        int maxQuickSlots = Define.Amount.QuickSlot;
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
        var attributes = Managers.Game.Character.Attributes;
        int maxDashCount = attributes.GetBase<int>(AttributeType.DashCount).CurrentValue;
        var dashStream = attributes.Stream<int>(AttributeType.DashCount);

        _dashSlots = new UIDashCountSlot[maxDashCount];
        var content = GetRectTransform(RectTransforms.DashContent).transform;

        for (int index = 0; index < maxDashCount; index++)
        {
            var (slot, _) = Managers.Pool.Pop<UIDashCountSlot>(content);
            _dashSlots[index] = slot;
            slot.SetIndex(index, dashStream);
        }
    }

    private void InitHealthSlots()
    {
        var attributes = Managers.Game.Character.Attributes;
        int totalHealth = attributes.GetBase<int>(AttributeType.Health).CurrentValue;
        int maxHealthSlots = Mathf.CeilToInt(totalHealth / 2f);
        var healthStream = attributes.Stream<int>(AttributeType.Health);
        _healthSlots = new UIRemainHealthSlot[maxHealthSlots];
        var content = GetRectTransform(RectTransforms.HealthContent).transform;

        for (int index = 0; index < maxHealthSlots; index++)
        {
            var (slot, _) = Managers.Pool.Pop<UIRemainHealthSlot>(content);
            _healthSlots[index] = slot;
            slot.SetIndex(index, healthStream, onSlotBecomeEmpty: (emptyIndex) =>
            {
                // TODO ::: 플레이어 사망 연출 또는 매니저 호출 로직
            });
        }
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
