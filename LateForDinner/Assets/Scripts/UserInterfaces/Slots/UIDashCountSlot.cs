using R3;
using UnityEngine;

public class UIDashCountSlot : UISlot
{
    private enum Images
    {
        DashCountImage
    }

    private enum UI_DashState
    {
        Charged,
        Used
    }

    private int _index;
    private Animator _animator;
    private UI_DashState _currentState = UI_DashState.Charged;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        _animator = GetImage(Images.DashCountImage).AddAnimator();
    }

    public void SetIndex(int index, ReadOnlyReactiveProperty<int> dashCountStream)
    {
        _index = index;
        int initialDash = dashCountStream.CurrentValue;
        bool isFilled = _index < initialDash;
        _currentState = isFilled ? UI_DashState.Charged : UI_DashState.Used;
        PlayDashAnimation(_currentState, isInit: false);
        dashCountStream
        .Subscribe(currentDash =>
        {
            UpdateDashState(currentDash);
        }).RegisterToPool(this);
    }

    private void UpdateDashState(int currentDashCount)
    {
        bool isFilled = _index < currentDashCount;
        UI_DashState nextState = isFilled ? UI_DashState.Charged : UI_DashState.Used;

        if (_currentState == nextState)
            return;

        _currentState = nextState;
        PlayDashAnimation(_currentState, isInit: false);
    }

    private void PlayDashAnimation(UI_DashState state, bool isInit)
    {
        switch (state)
        {
            case UI_DashState.Charged:
                _animator.Play(Define.Animation.HeadUpDashCharge);
                break;

            case UI_DashState.Used:
                _animator.Play(Define.Animation.HeadUpDashUse);
                break;
        }
    }
}
