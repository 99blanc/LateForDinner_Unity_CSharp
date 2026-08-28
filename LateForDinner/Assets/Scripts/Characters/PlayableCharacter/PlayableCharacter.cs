using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityHFSM;

public abstract class PlayableCharacter : Character, IIdleableCharacter, IMovableCharacter, IFallableCharacter, IJumpableCharacter
{
    public Rigidbody2D Rigidbody { get; private set; }
    public Transform BackTransform { get; private set; }
    public Transform FrontTransform { get; private set; }
    public Transform HitboxTransform { get; private set; }

    public override async UniTask InitAsync()
    {
        await base.InitAsync();
        InitAttributes();
    }

    private void InitAttributes()
    {
        if (Managers.Data.PlayableCharacterTemplates.TryGetValue((int)CharacterID, out var templateAttributes) == false)
            return;

        foreach (var (key, value) in templateAttributes)
        {
            if (Enum.TryParse<AttributeType>(key, out var attributeType) == false)
                continue;

            Attributes.SetParsedValue(attributeType, value);
        }
    }

    protected override void RegisterStates(StateMachine<CharacterStateType> fsm)
    {
        base.RegisterStates(fsm);
        fsm.AddState(CharacterStateType.Move, new MoveState(this, GetPlayerMoveInput));
        fsm.AddState(CharacterStateType.Fall, new FallState(this, GetPlayerMoveInput));
    }

    protected override void RegisterTransitions(StateMachine<CharacterStateType> fsm)
    {
        base.RegisterTransitions(fsm);
        fsm
        .AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Move,
            condition: _ => IsPlayerTryingToMove()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Idle,
            condition: _ => HasPlayerStoppedMoving()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Fall,
            condition: _ => !this.IsGrounded() && Rigidbody.linearVelocity.y < -0.1f
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Fall,
            condition: _ => !this.IsGrounded() && Rigidbody.linearVelocity.y < -0.1f
        ));
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
            condition: _ => IsPlayerTryingToJump() && (this as IJumpableCharacter).RemainingJumpCount > 0
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Jump,
            condition: _ => IsPlayerTryingToJump()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Jump,
            condition: _ => IsPlayerTryingToJump()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Jump,
            to: CharacterStateType.Jump,
            condition: _ => IsPlayerTryingToJump() && (this as IJumpableCharacter).RemainingJumpCount > 0
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
    }

    protected override void CacheComponents()
    {
        base.CacheComponents();
        Rigidbody = this.FindChildAssert<Rigidbody2D>(recursive: true);
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

    private bool IsPlayerTryingToMove()
        => Mathf.Abs(GetPlayerMoveInput()) > 0.01f;
    private bool IsPlayerTryingToJump()
        => Managers.Control.IsTriggered(Literal.Hotkeys.Jump);

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

    public bool IsJumpKeyTriggered()
        => Managers.Control.IsTriggered(Literal.Hotkeys.Jump);
}
