using UnityEngine;

public class UIRemainHealthSlot : UISlot
{
    private enum Images
    {
        RemainHealthImage
    }

    public enum UI_HealthState
    {
        Full,
        Half,
        Empty,
        TemporaryFull,
        TemporaryHalf,
        TemporaryEmpty
    }

    private Animator _animator;
    private int _index;
    private UI_HealthState _currentState;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
        _animator = GetImage(Images.RemainHealthImage).AddAnimator();
    }

    public void SetIndex(int index)
        => _index = index;

    public void UpdateHealthState(int currentHealth)
    {
        int slotHealthValue = currentHealth - (_index * 2);
        UI_HealthState nextState;

        if (slotHealthValue >= 2)
            nextState = UI_HealthState.Full;
        else if (slotHealthValue == 1)
            nextState = UI_HealthState.Half;
        else
            nextState = UI_HealthState.Empty;

        ApplyState(nextState);
    }

    public void UpdateTempHealthState(int currentTempHealth)
    {
        int slotHealthValue = currentTempHealth - (_index * 2);
        UI_HealthState nextState;

        if (slotHealthValue >= 2)
            nextState = UI_HealthState.TemporaryFull;
        else if (slotHealthValue == 1)
            nextState = UI_HealthState.TemporaryHalf;
        else
            nextState = UI_HealthState.TemporaryEmpty;

        ApplyState(nextState);
    }

    public void ForceSetState(UI_HealthState nextState)
        => ApplyState(nextState);

    private void ApplyState(UI_HealthState nextState)
    {
        if (_currentState == nextState)
            return;

        _currentState = nextState;
        ChangeSprite(_currentState);
        PlayHealthAnimation(_currentState);
    }

    private void ChangeSprite(UI_HealthState state)
    {
        string spriteName = state switch
        {
            UI_HealthState.Full => Define.Sprite.HUD_PlayerHealth_Full,
            UI_HealthState.Half => Define.Sprite.HUD_PlayerHealth_Half,
            UI_HealthState.Empty => Define.Sprite.HUD_PlayerHealth_Empty,
            UI_HealthState.TemporaryFull => Define.Sprite.HUD_PlayerTemporaryHealth_Full,
            UI_HealthState.TemporaryHalf => Define.Sprite.HUD_PlayerTemporaryHealth_Half,
            UI_HealthState.TemporaryEmpty => Define.Sprite.HUD_PlayerHealth_Empty,
            _ => Define.Sprite.HUD_PlayerHealth_Empty
        };
        GetImage(Images.RemainHealthImage).sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, spriteName);
    }

    private void PlayHealthAnimation(UI_HealthState state)
    {
        switch (state)
        {
            case UI_HealthState.Full:
                _animator.Play(Define.Animation.None);
                break;
            case UI_HealthState.Half:
                _animator.Play(Define.Animation.HeadUpHealthFull);
                break;
            case UI_HealthState.Empty:
                _animator.Play(Define.Animation.HeadUpHealthHalf);
                break;
            case UI_HealthState.TemporaryFull:
                _animator.Play(Define.Animation.None);
                break;
            case UI_HealthState.TemporaryHalf:
                _animator.Play(Define.Animation.HeadUpTemporaryHealthFull);
                break;
            case UI_HealthState.TemporaryEmpty:
                _animator.Play(Define.Animation.HeadUpTemporaryHealthHalf);
                break;
        }
    }
}
