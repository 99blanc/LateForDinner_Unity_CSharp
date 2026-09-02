using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Threading;
using UnityEngine;

public class UIRemainHealthSlot : UISlot, IAnimatableUI
{
    private enum Images
    {
        RemainHealthImage
    }

    private enum UI_HealthState
    {
        Empty,
        Half,
        Full
    }

    public enum UI_HealthSlotType
    {
        Normal,
        Temporary
    }

    private int _slotIndex;
    private UI_HealthSlotType _slotType;
    private UI_HealthState _currentState = UI_HealthState.Full;

    public override void OnInit()
    {
        base.OnInit();
        BindImage(typeof(Images));
    }

    public void InitHealthSlot(PlayableCharacter player, int index, UI_HealthSlotType slotType)
    {
        _slotIndex = index;
        _slotType = slotType;
        var image = GetImage(Images.RemainHealthImage);

        if (_slotType == UI_HealthSlotType.Normal)
            image.sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, Define.Sprite.HealthFull);
        else
            image.sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, Define.Sprite.TemporaryHealthFull);

        AttributeType currentAttrType = (_slotType == UI_HealthSlotType.Temporary) ? AttributeType.TemporaryHealth : AttributeType.Health;
        var healthAttr = player.Attributes.Get<int>(currentAttrType);
        var maxHealthAttr = player.Attributes.GetBase<int>(currentAttrType);
        UpdateHealthState(healthAttr.CurrentValue, maxHealthAttr.CurrentValue);
        Observable.CombineLatest(healthAttr.AsObservable(), maxHealthAttr.AsObservable(), (health, maxHealth) => (health, maxHealth))
        .Skip(1)
        .Subscribe(this, (tuple, slot) =>
        {
            slot.UpdateHealthState(tuple.health, tuple.maxHealth);
        })
        .RegisterToPool(this);
    }

    private void UpdateHealthState(int currentHealth, int maxHealth)
    {
        int slotThreshold = _slotIndex * 2;
        UI_HealthState targetState = GetStateFromHealth(currentHealth, slotThreshold);

        if (_currentState == targetState)
            return;

        var oldState = _currentState;
        _currentState = targetState;
        PlayHealthTransitionAsync(oldState, targetState).Forget();
    }

    private UI_HealthState GetStateFromHealth(int health, int slotThreshold)
    {
        int slotHealth = health - slotThreshold;

        if (slotHealth >= 2)
            return UI_HealthState.Full;
        if (slotHealth == 1)
            return UI_HealthState.Half;

        return UI_HealthState.Empty;
    }

    private async UniTaskVoid PlayHealthTransitionAsync(UI_HealthState oldState, UI_HealthState newState)
    {
        int hash = 0;

        if (_slotType == UI_HealthSlotType.Normal)
        {
            switch ((oldState, newState))
            {
                case (UI_HealthState.Empty, UI_HealthState.Half):
                    hash = Define.Animation.HealthHalfCharge;
                    break;
                case (UI_HealthState.Half, UI_HealthState.Full):
                    hash = Define.Animation.HealthFullCharge;
                    break;
                case (UI_HealthState.Full, UI_HealthState.Half):
                    hash = Define.Animation.HealthFull;
                    break;
                case (UI_HealthState.Half, UI_HealthState.Empty):
                    hash = Define.Animation.HealthHalf;
                    break;
                case (UI_HealthState.Full, UI_HealthState.Empty):
                    hash = Define.Animation.HealthAllEmpty;
                    break;
                case (UI_HealthState.Empty, UI_HealthState.Full):
                    hash = Define.Animation.HealthAllFullCharge;
                    break;
            }
        }
        else
        {
            switch ((oldState, newState))
            {
                case (UI_HealthState.Empty, UI_HealthState.Half):
                    hash = Define.Animation.TemporaryHealthHalfCharge;
                    break;
                case (UI_HealthState.Half, UI_HealthState.Full):
                    hash = Define.Animation.TemporaryHealthFullCharge;
                    break;
                case (UI_HealthState.Full, UI_HealthState.Half):
                    hash = Define.Animation.TemporaryHealthFull;
                    break;
                case (UI_HealthState.Half, UI_HealthState.Empty):
                    hash = Define.Animation.TemporaryHealthHalf;
                    break;
                case (UI_HealthState.Full, UI_HealthState.Empty):
                    hash = Define.Animation.TemporaryHealthAllEmpty;
                    break;
                case (UI_HealthState.Empty, UI_HealthState.Full):
                    hash = Define.Animation.TemporaryHealthAllFullCharge;
                    break;
            }
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
                // DESC ::: 연속 데미지나 힐로 인해 기존 애니메이션이 끊겼을 경우 (정상)
            }
        }

        ApplyStaticState(oldState, newState);
    }

    private void ApplyStaticState(UI_HealthState oldState, UI_HealthState newState)
    {
        var image = GetImage(Images.RemainHealthImage);

        if (_slotType == UI_HealthSlotType.Normal)
        {
            switch ((oldState, newState))
            {
                case (UI_HealthState.Empty, UI_HealthState.Half):
                    image.sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, Define.Sprite.HealthHalf);
                    break;
                case (UI_HealthState.Half, UI_HealthState.Full):
                    image.sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, Define.Sprite.HealthFull);
                    break;
                case (UI_HealthState.Full, UI_HealthState.Half):
                    image.sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, Define.Sprite.HealthHalf);
                    break;
                case (UI_HealthState.Half, UI_HealthState.Empty):
                    image.sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, Define.Sprite.HealthEmpty);
                    break;
                case (UI_HealthState.Full, UI_HealthState.Empty):
                    image.sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, Define.Sprite.HealthEmpty);
                    break;
                case (UI_HealthState.Empty, UI_HealthState.Full):
                    image.sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, Define.Sprite.HealthFull);
                    break;
            }
        }
        else
        {
            switch ((oldState, newState))
            {
                case (UI_HealthState.Empty, UI_HealthState.Half):
                    image.sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, Define.Sprite.TemporaryHealthHalf);
                    break;
                case (UI_HealthState.Half, UI_HealthState.Full):
                    image.sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, Define.Sprite.TemporaryHealthFull);
                    break;
                case (UI_HealthState.Full, UI_HealthState.Half):
                    image.sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, Define.Sprite.TemporaryHealthHalf);
                    break;
                case (UI_HealthState.Half, UI_HealthState.Empty):
                    image.sprite = Managers.Resource.GetSprite(Define.Atlas.Common, Define.Sprite.Empty);
                    break;
                case (UI_HealthState.Full, UI_HealthState.Empty):
                    image.sprite = Managers.Resource.GetSprite(Define.Atlas.Common, Define.Sprite.Empty);
                    break;
                case (UI_HealthState.Empty, UI_HealthState.Full):
                    image.sprite = Managers.Resource.GetSprite(Define.Atlas.HeadUp, Define.Sprite.TemporaryHealthFull);
                    break;
            }
        }
    }
}
