using UnityEngine;

public class UIDashCountSlot : UISlot
{
    private enum Images
    {
        DashCountImage
    }

    public enum UI_DashState
    {
        Charged,
        Used
    }

    private Animator _animator { get; set; }
    private int _index;
    private UI_DashState _currentState = UI_DashState.Charged;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        _animator = GetImage(Images.DashCountImage).AddAnimator();
    }

    public void SetIndex(int index)
        => _index = index;

    public void UpdateState(int currentDashCount)
    {
        bool isFilled = _index < currentDashCount;
        UI_DashState nextState = isFilled ? UI_DashState.Charged : UI_DashState.Used;

        if (_currentState == nextState)
            return;

        _currentState = nextState;
        PlayDashAnimation(_currentState);
    }

    public void ForceSetState(int currentDashCount)
    {
        bool isFilled = _index < currentDashCount;
        _currentState = isFilled ? UI_DashState.Charged : UI_DashState.Used;
        PlayDashAnimation(_currentState);
    }

    private void PlayDashAnimation(UI_DashState state)
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
