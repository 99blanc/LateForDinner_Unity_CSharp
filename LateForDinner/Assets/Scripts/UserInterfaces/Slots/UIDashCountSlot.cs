using Cysharp.Threading.Tasks;
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
        None,
        Charged,
        Used
    }

    private int _index;
    private Animator _animator;
    private Sprite _sprite;
    private UI_DashState _currentState = UI_DashState.None;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        _animator = GetImage(Images.DashCountImage).AddAnimator();
        _sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, Define.Sprite.HUD_PlayerDashCount);
    }

    public void SetIndex(int index, ReadOnlyReactiveProperty<int> dashCountStream)
    {
        _index = index;
        int initialDash = dashCountStream.CurrentValue;
        bool isFilled = _index < initialDash;
        _currentState = isFilled ? UI_DashState.Charged : UI_DashState.Used;
        PlayDashAnimation(_currentState, isInit: false).Forget();
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
        PlayDashAnimation(_currentState, isInit: false).Forget();
    }

    private async UniTask PlayDashAnimation(UI_DashState state, bool isInit)
    {
        switch (state)
        {
            case UI_DashState.Charged:
                gameObject.SetActive(true);
                await _animator.AwaitForComplete(Define.Animation.HeadUpDashCharge);
                break;

            case UI_DashState.Used:
                gameObject.SetActive(true);
                await _animator.AwaitForComplete(Define.Animation.HeadUpDashUse);
                gameObject.SetActive(false);
                break;
        }
    }
}
