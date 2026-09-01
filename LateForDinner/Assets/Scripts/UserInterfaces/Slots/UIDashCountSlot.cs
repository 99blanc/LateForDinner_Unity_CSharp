using UnityEngine;

public class UIDashCountSlot : UISlot
{
    private enum Images
    {
        DashCountImage
    }

    private enum UI_DashState
    {
        None,
        Charged,
        Used
    }

    private Animator _animator;
    private int _index;
    private UI_DashState _currentState = UI_DashState.None;

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
        ApplyState(nextState);
    }

    public void ForceSetState(int currentDashCount)
    {
        bool isFilled = _index < currentDashCount;
        _currentState = isFilled ? UI_DashState.Charged : UI_DashState.Used;
        GetImage(Images.DashCountImage).sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, Define.Sprite.HUD_PlayerDashCount);
    }

    public void SetNoneState()
        => ApplyState(UI_DashState.None);

    private void ApplyState(UI_DashState nextState)
    {
        if (_currentState == nextState)
            return;

        _currentState = nextState;
        PlayDashAnimation(_currentState);
    }

    private void PlayDashAnimation(UI_DashState state)
    {
        switch (state)
        {
            case UI_DashState.None:
                _animator.Play(Define.Animation.None);
                break;
            case UI_DashState.Charged:
                _animator.Play(Define.Animation.HeadUpDashCharge);
                break;
            case UI_DashState.Used:
                _animator.Play(Define.Animation.HeadUpDashUse);
                break;
        }
    }
}
