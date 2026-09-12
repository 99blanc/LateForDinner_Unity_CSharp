using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;

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

    public void SetDashSlot(int index)
    {
        var player = Managers.Game.Player;
        var dashAttribute = player.Attributes.Get<int>(AttributeType.DashCount);
        int initialCount = dashAttribute.CurrentValue;
        _slotIndex = index;
        _currentState = GetStateFromDash(initialCount, _slotIndex);
        dashAttribute.AsObservable()
        .Skip(1)
        .Subscribe(this, (currentCount, slot) =>
        {
            slot.UpdateDashState(currentCount);
        }).RegisterToPool(this);
        ApplyStaticState(_currentState); 
        Refresh();

    }

    public override void Refresh()
    {
        base.Refresh();
        var player = Managers.Game.Player;
        var dashAttribute = player.Attributes.Get<int>(AttributeType.DashCount);

        if (dashAttribute != null)
        {
            UI_DashState realState = GetStateFromDash(dashAttribute.CurrentValue, _slotIndex);
            _currentState = realState;
            ApplyStaticState(_currentState);
        }
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

    private async UniTask PlayDashTransitionAsync(UI_DashState oldState, UI_DashState newState)
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
