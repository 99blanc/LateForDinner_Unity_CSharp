using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityHFSM;

public abstract class PlayableCharacter : Character, IIdleableCharacter, IMovableCharacter, IFallableCharacter, ICrouchableCharacter, IJumpableCharacter, IRollableCharacter, IDashableCharacter, IClimbableCharacter
{
    private readonly HashSet<IInteractable> _interactables = new HashSet<IInteractable>();
    public IInteractable CurrentInteractable
    {
        get
        {
            if (_interactables.Count == 0)
                return null;

            return _interactables
            .OrderBy(x => x.Priority)
            .FirstOrDefault();
        }
    }
    public Rigidbody2D Rigidbody { get; private set; }
    public Transform CameraTransform { get; private set; }
    public Transform BackTransform { get; private set; }
    public Transform FrontTransform { get; private set; }
    public Transform HitboxTransform { get; private set; }

    public override async UniTask InitAsync()
    {
        await base.InitAsync();
        InitAttributes();
        Managers.Control.Subscribe(Literal.Hotkeys.Interact, () => TryExecuteInteraction()).RegisterToPool(this);
    }

    private void InitAttributes()
    {
        var templateGroup = Managers.Data.PlayableCharacterTemplates[(int)CharacterID];

        if (templateGroup == null || !templateGroup.Any())
            return;

        foreach (var template in templateGroup)
        {
            if (Enum.TryParse<AttributeType>(template.AttributeKey, out var attributeType) == false)
                continue;

            Attributes.SetParsedValue(attributeType, template.Value);
        }
    }

    protected override void RegisterStates(StateMachine<CharacterStateType> fsm)
    {
        base.RegisterStates(fsm);
        fsm.AddState(CharacterStateType.Move, new MoveState(this, GetPlayerMoveInput));
        fsm.AddState(CharacterStateType.Fall, new FallState(this, GetPlayerMoveInput));
        fsm.AddState(CharacterStateType.Jump, new JumpState(this, GetPlayerMoveInput));
        fsm.AddState(CharacterStateType.Dash, new DashState(this, GetPlayerDashInput));
        fsm.AddState(CharacterStateType.Climb, new ClimbState(this, GetPlayerVerticalInput));
    }

    protected override void RegisterTransitions(StateMachine<CharacterStateType> fsm)
    {
        base.RegisterTransitions(fsm);
        // DESC ::: Idle 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Move,
            condition: _ => IsPlayerTryingToMove()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Fall,
            condition: _ => fsm.ActiveStateName != CharacterStateType.Dash && !this.IsGrounded() && Rigidbody.linearVelocity.y < -0.1f
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Crouch,
            condition: _ => IsPlayerTryingToCrouch()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Jump,
            condition: _ => IsPlayerTryingToJump()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Dash,
            condition: _ => IsPlayerTryingToDash()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Climb,
            condition: _ => IsPlayerTryingToClimb(),
            onTransition: _ => ResetAirActionsForLadder()
        ));
        // DESC ::: Move 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Idle,
            condition: _ => HasPlayerStoppedMoving()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Fall,
            condition: _ => !this.IsGrounded() && Rigidbody.linearVelocity.y < -0.1f
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Crouch,
            condition: _ => IsPlayerTryingToCrouch()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Jump,
            condition: _ => IsPlayerTryingToJump()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Dash,
            condition: _ => IsPlayerTryingToDash()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Climb,
            condition: _ => IsPlayerTryingToClimb(),
            onTransition: _ => ResetAirActionsForLadder()
        ));
        // DESC ::: Fall 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Fall,
            to: CharacterStateType.Idle,
            condition: _ => CheckLandingAndResetJump(out bool isStationary) && isStationary
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Fall,
            to: CharacterStateType.Move,
            condition: _ => CheckLandingAndResetJump(out bool isStationary) && !isStationary
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Fall,
            to: CharacterStateType.Jump,
            condition: _ => IsPlayerTryingToJump()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Fall,
            to: CharacterStateType.Dash,
            condition: _ => IsPlayerTryingToDash()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Fall,
            to: CharacterStateType.Climb,
            condition: _ => IsPlayerTryingToClimb(),
            onTransition: _ => ResetAirActionsForLadder()
        ));
        // DESC ::: Crouch 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Crouch,
            to: CharacterStateType.Idle,
            condition: _ => !Managers.Control.IsPressed(Literal.Hotkeys.DownUtility) && Mathf.Abs(GetPlayerMoveInput()) <= 0.01f
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Crouch,
            to: CharacterStateType.Move,
            condition: _ => !Managers.Control.IsPressed(Literal.Hotkeys.DownUtility) && Mathf.Abs(GetPlayerMoveInput()) > 0.01f
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Crouch,
            to: CharacterStateType.Fall,
            condition: _ => !this.IsGrounded() && Rigidbody.linearVelocity.y < -0.1f
        ));
        // DESC ::: Jump 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Jump,
            to: CharacterStateType.Jump,
            condition: _ => IsPlayerTryingToJump()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Jump,
            to: CharacterStateType.Idle,
            condition: _ => CheckLandingAndResetJump(out bool isStationary) && isStationary
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Jump,
            to: CharacterStateType.Move,
            condition: _ => CheckLandingAndResetJump(out bool isStationary) && !isStationary
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Jump,
            to: CharacterStateType.Fall,
            condition: _ => Rigidbody.linearVelocity.y < -0.1f && !this.IsGrounded()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Jump,
            to: CharacterStateType.Roll,
            condition: _ => IsPlayerReadyToRoll()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Jump,
            to: CharacterStateType.Dash,
            condition: _ => IsPlayerTryingToDash()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Jump,
            to: CharacterStateType.Climb,
            condition: _ => IsPlayerTryingToClimb(),
            onTransition: _ => ResetAirActionsForLadder()
        ));
        // DESC ::: Roll 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Roll,
            to: CharacterStateType.Idle,
            condition: _ => CharacterAnimator.GetCurrentAnimatorNormalizedTime() >= 1f && this.IsGrounded() && Mathf.Abs(GetPlayerMoveInput()) <= 0.01f
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Roll,
            to: CharacterStateType.Move,
            condition: _ => CharacterAnimator.GetCurrentAnimatorNormalizedTime() >= 1f && this.IsGrounded() && Mathf.Abs(GetPlayerMoveInput()) > 0.01f
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Roll,
            to: CharacterStateType.Fall,
            condition: _ => CharacterAnimator.GetCurrentAnimatorNormalizedTime() >= 1f && !this.IsGrounded()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Roll,
            to: CharacterStateType.Dash,
            condition: _ => IsPlayerTryingToDash()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Roll,
            to: CharacterStateType.Climb,
            condition: _ => IsPlayerTryingToClimb(),
            onTransition: _ => ResetAirActionsForLadder()
        ));
        // DESC ::: Dash 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Dash,
            to: CharacterStateType.Idle,
            condition: _ => ((IDashableCharacter)this).IsDurationEnded == true && this.IsGrounded()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Dash,
            to: CharacterStateType.Fall,
            condition: _ => ((IDashableCharacter)this).IsDurationEnded == true && !this.IsGrounded()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Dash,
            to: CharacterStateType.Climb,
            condition: _ => IsPlayerTryingToClimb(),
            onTransition: _ => ResetAirActionsForLadder()
        ));
        // DESC ::: Climb 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Climb,
            to: CharacterStateType.Idle,
            condition: _ => CurrentInteractable == null || (!Managers.Control.IsPressed(Literal.Hotkeys.UpUtility) && !Managers.Control.IsPressed(Literal.Hotkeys.DownUtility) && Mathf.Abs(GetPlayerMoveInput()) <= 0.01f && this.IsGrounded())
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Climb,
            to: CharacterStateType.Move,
            condition: _ => CurrentInteractable != null && Mathf.Abs(GetPlayerMoveInput()) > 0.01f
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Climb,
            to: CharacterStateType.Jump,
            condition: _ => IsPlayerTryingToJump()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Climb,
            to: CharacterStateType.Fall,
            condition: _ => CurrentInteractable == null && !this.IsGrounded()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Climb,
            to: CharacterStateType.Dash,
            condition: _ => IsPlayerTryingToDash()
        ));
    }

    protected override void CacheComponents()
    {
        base.CacheComponents();
        Rigidbody = this.FindChildAssert<Rigidbody2D>(recursive: true);
        CameraTransform = this.FindChildAssert<Transform>(Literal.Objects.CameraTransform, recursive: true);
        BackTransform = this.FindChildAssert<Transform>(Literal.Objects.BackTransform, recursive: true);
        FrontTransform = this.FindChildAssert<Transform>(Literal.Objects.FrontTransform, recursive: true);
        HitboxTransform = this.FindChildAssert<Transform>(Literal.Objects.HitboxTransform, recursive: true);
    }

    protected float GetPlayerMoveInput()
    {
        if (Managers.Control.IsPressed(Literal.Hotkeys.Right)) 
            return 1f;

        if (Managers.Control.IsPressed(Literal.Hotkeys.Left)) 
            return -1f;

        return 0f;
    }

    protected Vector2 GetPlayerDashInput()
    {
        Vector2 input = Vector2.zero;

        if (Managers.Control.IsPressed(Literal.Hotkeys.Right))
            input.x += 1f;
        if (Managers.Control.IsPressed(Literal.Hotkeys.Left))
            input.x -= 1f;
        if (Managers.Control.IsPressed(Literal.Hotkeys.DownUtility))
            input.y -= 1f;

        return input;
    }

    protected float GetPlayerVerticalInput()
    {
        if (Managers.Control.IsPressed(Literal.Hotkeys.UpUtility))
            return 1f;

        if (Managers.Control.IsPressed(Literal.Hotkeys.DownUtility))
            return -1f;

        return 0f;
    }

    private bool IsPlayerTryingToMove()
        => Mathf.Abs(GetPlayerMoveInput()) > 0.01f;
    private bool IsPlayerTryingToCrouch()
        => this.IsGrounded() && Managers.Control.IsPressed(Literal.Hotkeys.DownUtility);
    private bool IsPlayerTryingToJump()
    {
        bool isKeyTriggered = Managers.Control.IsTriggered(Literal.Hotkeys.Jump);
        bool hasJumpCount = (this as IJumpableCharacter).RemainingJumpCount > 0;
        return isKeyTriggered && hasJumpCount;
    }
    private bool IsPlayerTryingToDash()
    {
        if (this is IDashableCharacter dashable)
        {
            if (dashable.IsOnCooldown)
                return false;
        }

        bool isModifierDash = Managers.Config.Option.Access.modifierDash;
        return this.IsGrounded() ? CheckDashInput(isModifierDash, allowDownUtility: false) : CheckDashInput(isModifierDash, allowDownUtility: true);
    }
    private bool IsPlayerReadyToRoll()
    {
        bool isLastJump = this is IJumpableCharacter jumpable && jumpable.RemainingJumpCount == 0;
        bool isAnimationReady = CharacterAnimator is ProtagonistAnimator protagonistAnimator && protagonistAnimator.GetCurrentAnimatorNormalizedTime() >= Define.Animation.NormalizedTime;
        return isLastJump && isAnimationReady;
    }
    private bool IsPlayerTryingToClimb()
    {
        if (this is IClimbableCharacter climbable && !climbable.CanClimb)
            return false;

        if (!Managers.Control.IsPressed(Literal.Hotkeys.UpUtility) && !Managers.Control.IsPressed(Literal.Hotkeys.DownUtility))
            return false;

        if (CurrentInteractable != null && CurrentInteractable is IInteractable interactable && interactable.PropKey == PropKey.Ladder)
            return true;

        return false;
    }

    private bool HasPlayerStoppedMoving()
    {
        bool hasNoInput = Mathf.Abs(GetPlayerMoveInput()) <= 0.01f;
        bool hasNoVelocity = Mathf.Abs(Rigidbody.linearVelocity.x) < 0.1f;
        return hasNoInput && hasNoVelocity;
    }

    private bool CheckLandingAndResetJump(out bool isStationary)
    {
        isStationary = Mathf.Abs(GetPlayerMoveInput()) <= 0.01f;

        if (this.IsGrounded())
        {
            if (this is IJumpableCharacter jumpable)
                jumpable.RemainingJumpCount = jumpable.MaxJumpCount;

            return true;
        }

        return false;
    }

    private bool CheckDashInput(bool isModifierDash, bool allowDownUtility)
    {
        if (isModifierDash)
        {
            bool triggered = Managers.Control.IsModifierTriggered(Literal.Hotkeys.Dash, Literal.Hotkeys.Left) || Managers.Control.IsModifierTriggered(Literal.Hotkeys.Dash, Literal.Hotkeys.Right);

            if (allowDownUtility)
                triggered |= Managers.Control.IsModifierTriggered(Literal.Hotkeys.Dash, Literal.Hotkeys.DownUtility);

            return triggered;
        }
        else
        {
            bool triggered = Managers.Control.IsDoubleTriggered(Literal.Hotkeys.Left) || Managers.Control.IsDoubleTriggered(Literal.Hotkeys.Right);

            if (allowDownUtility)
                triggered |= Managers.Control.IsDoubleTriggered(Literal.Hotkeys.DownUtility);

            return triggered;
        }
    }

    private void ResetAirActionsForLadder()
    {
        if (this is IJumpableCharacter jumpable)
            jumpable.RemainingJumpCount = jumpable.MaxJumpCount;

        if (this is IDashableCharacter dashable)
            dashable.RemainingDashCount = dashable.MaxDashCount;
    }

    public bool IsJumpKeyTriggered()
        => Managers.Control.IsTriggered(Literal.Hotkeys.Jump);

    private void TryExecuteInteraction()
    {
        var target = CurrentInteractable;

        if (target != null && target.RequireKeyInput && target.CanInteract.Value)
            target.ProtectedInteract(this);
    }

    private void OnTriggerEnter2D(Collider2D target)
    {
        if (target.GetComponentInParent<IInteractable>() is IInteractable interactable)
        {
            _interactables.Add(interactable);
            interactable.CanInteract.Value = true;

            if (!interactable.TriggerOnProximity)
                return;

            if (interactable == CurrentInteractable)
                interactable.ProtectedInteract(this);
        }
    }

    private void OnTriggerExit2D(Collider2D target)
    {
        if (target.GetComponentInParent<IInteractable>() is IInteractable interactable)
        {
            _interactables.Remove(interactable);
            interactable.CanInteract.Value = false;
        }
    }
}
