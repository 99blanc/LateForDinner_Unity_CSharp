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
    private Animator _animator;
    private UI_RemainHealthState _currentState = UI_RemainHealthState.Full;
    private Action<int> _onSlotBecomeEmpty;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        _animator = GetImage(Images.RemainHealthImage).GetComponentAssert<Animator>();
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
                _animator?.Play(Define.Animation.HeadUpHealthHelp, 0, 0f);
                break;
            case UI_RemainHealthState.Half:
                _animator?.Play(Define.Animation.HeadUpHealthHalf, 0, 0f);
                break;
            case UI_RemainHealthState.Full:
                _animator?.Play(Define.Animation.HeadUpHealthFull, 0, 0f);
                break;
        }
    }

    private void RefreshVisual(UI_RemainHealthState state)
    {
        // TODO ::: Managers.Resource.GetSprite(...) 등을 통해 Full / Half 스프라이트로 교체
    }
}
