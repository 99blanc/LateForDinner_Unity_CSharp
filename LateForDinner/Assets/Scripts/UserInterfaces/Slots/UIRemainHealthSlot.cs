using R3;
using System;
using UnityEngine;

public class UIRemainHealthSlot : UISlot
{
    private enum Images
    {
        RemainHealthImage
    }

    private enum UI_RemainHealthState
    {
        Help,
        Half,
        Full
    }

    private int _index;
    private UI_RemainHealthState _currentState = UI_RemainHealthState.Full;
    private Action<int> _onSlotBecomeEmpty;
    [HideInInspector] public Animator Animator { get; set; }

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        Animator = GetImage(Images.RemainHealthImage).GetComponentAssert<Animator>();
    }

    public void SetIndex(int index, ReadOnlyReactiveProperty<int> healthStream, Action<int> onSlotBecomeEmpty = null)
    {
        _index = index;
        _onSlotBecomeEmpty = onSlotBecomeEmpty;
        healthStream
        .Subscribe(currentHealth =>
        {
            UpdateHealthState(currentHealth);
        })
        .RegisterToPool(this);
        UpdateHealthState(healthStream.CurrentValue);
    }

    private void UpdateHealthState(int currentHealth)
    {
        int slotMinHealth = _index * 2;
        int remainingInThisSlot = Mathf.Clamp(currentHealth - slotMinHealth, 0, 2);
        UI_RemainHealthState nextState = (UI_RemainHealthState)remainingInThisSlot;

        if (_currentState != nextState)
        {
            if (nextState == UI_RemainHealthState.Help && _index == 0)
            {
                // TODO ::: 플레이어 사망 상태 알림 콜백 혹은 매니저 호출
                // Managers.Game.Player.NotifyDeath(); 또는 _onSlotBecomeEmpty?.Invoke(_index);
            }

            _currentState = nextState;
            PlayHealthAnimation(_currentState);
            RefreshVisual(_currentState);
        }
    }

    private void PlayHealthAnimation(UI_RemainHealthState state)
    {
        switch (state)
        {
            case UI_RemainHealthState.Help:
                Animator.Play(Define.Animation.HeadUpHealthHelp);
                break;
            case UI_RemainHealthState.Half:
                Animator.Play(Define.Animation.HeadUpHealthHalf);
                break;
            case UI_RemainHealthState.Full:
                Animator.Play(Define.Animation.HeadUpHealthFull);
                break;
        }
    }

    private void RefreshVisual(UI_RemainHealthState state)
    {
        // TODO ::: Managers.Resource.GetSprite(...) 등을 통해 Full / Half 스프라이트로 교체
    }
}
