using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using UnityEngine;

public class UIDashCountSlot : UISlot, IAnimatableUI
{
    private enum Images
    {
        DashCountImage
    }

    private enum UI_DashState
    {
        Empty,
        Full
    }

    private int _slotIndex;
    private UI_DashState _currentState = UI_DashState.Full;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
    }

    public void InitDashSlot(PlayableCharacter player, int index)
    {
        _slotIndex = index;
        int initialCount = player.Attributes.Get<int>(AttributeType.DashCount).CurrentValue;

        _currentState = GetStateFromDash(initialCount, _slotIndex);
        ApplyStaticState(_currentState);

        player.Attributes.Get<int>(AttributeType.DashCount)
            .AsObservable()
            .Skip(1)
            .Subscribe(this, (currentCount, slot) =>
            {
                slot.UpdateDashState(currentCount);
            })
            .RegisterToPool(this);
    }

    private void UpdateDashState(int currentCount)
    {
        UI_DashState targetState = GetStateFromDash(currentCount, _slotIndex);

        if (_currentState == targetState)
            return;

        var oldState = _currentState;
        _currentState = targetState;
        PlayDashTransitionAsync(oldState, targetState).Forget();
    }

    private UI_DashState GetStateFromDash(int currentCount, int slotIndex)
        => slotIndex < currentCount ? UI_DashState.Full : UI_DashState.Empty;

    private async UniTaskVoid PlayDashTransitionAsync(UI_DashState oldState, UI_DashState newState)
    {
        int hash = 0;

        switch ((oldState, newState))
        {
            case (UI_DashState.Empty, UI_DashState.Full):
                hash = Define.Animation.DashCharge;
                break;
            case (UI_DashState.Full, UI_DashState.Empty):
                hash = Define.Animation.DashUse;
                break;
        }

        if (hash != 0)
        {
            try
            {
                CancellationToken cts = this.GetNewCancellationToken();
                await this.PlayClipAsync(hash);
            }
            catch (OperationCanceledException)
            {
                // DESC ::: 연속 대시로 인해 기존 애니메이션이 끊겼을 경우 (정상)
            }
        }

        ApplyStaticState(newState);
    }

    private void ApplyStaticState(UI_DashState state)
    {
        var image = GetImage(Images.DashCountImage);

        switch (state)
        {
            case UI_DashState.Empty:
                image.sprite = Managers.Resource.GetSprite(Define.Atlas.Common, Define.Sprite.Empty);
                break;
            case UI_DashState.Full:
                image.sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, Define.Sprite.DashCount);
                break;
        }
    }
}
