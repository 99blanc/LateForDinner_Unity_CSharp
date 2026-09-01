using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using UnityEngine;
using UnityHFSM;
using ZLinq;

public abstract class PlayableCharacter : Character, IIdleableCharacter, IMovableCharacter, IFallableCharacter, ICrouchableCharacter, IJumpableCharacter, IRollableCharacter, IDashableCharacter, IClimbableCharacter, ICarriableCharacter
{
    public Transform CameraTransform { get; private set; }
    public Transform BackTransform { get; private set; }
    public Transform FrontTransform { get; private set; }
    public Transform HitboxTransform { get; private set; }

    public override async UniTask InitAsync()
    {
        await base.InitAsync();
        var data = Managers.Save.CurrentData;
        var attributes = data.SavedAttributes;

        if (attributes != null && attributes.Count > 0)
            Attributes.ImportSaveData(attributes);
        else
            InitAttributes();

        Rigidbody.position = data.PlayerPosition;
        Rigidbody.rotation = data.PlayerRotation;
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
            Attributes.SetBaseParsedValue(attributeType, template.Value);
        }
    }

    protected override void RegisterStates(StateMachine<CharacterStateType> fsm)
    {
        base.RegisterStates(fsm);

        fsm.AddState(CharacterStateType.Move, new MoveState(this, GetPlayerMoveInput));
        fsm.AddState(CharacterStateType.Fall, new FallState(this, GetPlayerMoveInput));
        fsm.AddState(CharacterStateType.Crouch, new PlayableCharacterCrouchState(this));
        fsm.AddState(CharacterStateType.Jump, new PlayableCharacterJumpState(this, GetPlayerMoveInput));
        fsm.AddState(CharacterStateType.Dash, new PlayableCharacterDashState(this, GetPlayerDashInput));
        fsm.AddState(CharacterStateType.Climb, new ClimbState(this, GetPlayerClimbInput));
        fsm.AddState(CharacterStateType.Roll, new PlayableCharacterRollState(this, GetPlayerMoveInput));
        fsm.AddState(CharacterStateType.Throw, new ThrowState(this, GetPlayerThrowInput));
    }

    protected override void RegisterTransitions(StateMachine<CharacterStateType> fsm)
    {
        base.RegisterTransitions(fsm);
        Func<float> moveInput = GetPlayerMoveInput;
        Func<bool> jumpInput = IsPlayerJumpInput;
        Func<bool> crouchInput = IsPlayerCrouchInput;
        Func<float> climbInput = GetPlayerClimbInput;
        Func<bool> dashInput = IsPlayerDashInput;
        // DESC ::: Idle 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Move,
            condition: _ => this.IsTryingToMove(moveInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Fall,
            condition: _ => this.ShouldFallFromIdle(fsm, Rigidbody)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Crouch,
            condition: _ => this.IsTryingToCrouch(crouchInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Jump,
            condition: _ => this.IsTryingToJump(jumpInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Dash,
            condition: _ => this.IsTryingToDash(dashInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Climb,
            condition: _ => this.IsTryingToClimb(climbInput, crouchInput)
        ));
        // DESC ::: Move 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Idle,
            condition: _ => this.HasStoppedMoving(moveInput, Rigidbody)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Fall,
            condition: _ => this.ShouldFallFromAirborne(Rigidbody)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Crouch,
            condition: _ => this.IsTryingToCrouch(crouchInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Jump,
            condition: _ => this.IsTryingToJump(jumpInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Dash,
            condition: _ => this.IsTryingToDash(dashInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Climb,
            condition: _ => this.IsTryingToClimb(climbInput, crouchInput)
        ));
        // DESC ::: Fall 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Fall,
            to: CharacterStateType.Idle,
            condition: _ => this.IsLandingToIdle(moveInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Fall,
            to: CharacterStateType.Move,
            condition: _ => this.IsLandingToMove(moveInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Fall,
            to: CharacterStateType.Jump,
            condition: _ => this.IsTryingToJump(jumpInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Fall,
            to: CharacterStateType.Dash,
            condition: _ => this.IsTryingToDash(dashInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Fall,
            to: CharacterStateType.Climb,
            condition: _ => this.IsTryingToClimb(climbInput, crouchInput)
        ));
        // DESC ::: Crouch 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Crouch,
            to: CharacterStateType.Idle,
            condition: _ => this.IsCrouchToIdle(moveInput, crouchInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Crouch,
            to: CharacterStateType.Move,
            condition: _ => this.IsCrouchToMove(moveInput, crouchInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Crouch,
            to: CharacterStateType.Fall,
            condition: _ => this.ShouldFallFromAirborne(Rigidbody)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Crouch,
            to: CharacterStateType.Climb,
            condition: _ => this.IsTryingToClimb(climbInput, crouchInput)
        ));
        // DESC ::: Jump 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Jump,
            to: CharacterStateType.Jump,
            condition: _ => this.IsTryingToJump(jumpInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Jump,
            to: CharacterStateType.Idle,
            condition: _ => this.IsLandingToIdle(moveInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Jump,
            to: CharacterStateType.Move,
            condition: _ => this.IsLandingToMove(moveInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Jump,
            to: CharacterStateType.Fall,
            condition: _ => this.ShouldFallFromAirborne(Rigidbody)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Jump,
            to: CharacterStateType.Roll,
            condition: _ => this.IsPlayerReadyToRoll()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Jump,
            to: CharacterStateType.Dash,
            condition: _ => this.IsTryingToDash(dashInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Jump,
            to: CharacterStateType.Climb,
            condition: _ => this.IsTryingToClimb(climbInput, crouchInput),
            onTransition: _ => this.AddJumpActionForClimb()
        ));
        // DESC ::: Roll 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Roll,
            to: CharacterStateType.Idle,
            condition: _ => this.IsRollToIdle(moveInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Roll,
            to: CharacterStateType.Move,
            condition: _ => this.IsRollToMove(moveInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Roll,
            to: CharacterStateType.Fall,
            condition: _ => this.IsRollFinishedAndAirborne()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Roll,
            to: CharacterStateType.Dash,
            condition: _ => this.IsTryingToDash(dashInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Roll,
            to: CharacterStateType.Climb,
            condition: _ => this.IsTryingToClimb(climbInput, crouchInput),
            onTransition: _ => this.AddJumpActionForClimb()
        ));
        // DESC ::: Dash 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Dash,
            to: CharacterStateType.Idle,
            condition: _ => this.IsDashFinishedAndGrounded()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Dash,
            to: CharacterStateType.Fall,
            condition: _ => this.IsDashFinishedAndAirborne()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Dash,
            to: CharacterStateType.Climb,
            condition: _ => this.IsTryingToClimb(climbInput, crouchInput),
            onTransition: _ => this.AddDashActionForClimb()
        ));
        // DESC ::: Climb 상태에서의 전환 조건
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Climb,
            to: CharacterStateType.Idle,
            condition: _ => this.IsClimbToIdle(moveInput, climbInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Climb,
            to: CharacterStateType.Move,
            condition: _ => this.IsTryingToLeaveClimb(moveInput, climbInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Climb,
            to: CharacterStateType.Idle,
            condition: _ => this.IsClimbToGroundIdle(moveInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Climb,
            to: CharacterStateType.Move,
            condition: _ => this.IsClimbToGroundMove(moveInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Climb,
            to: CharacterStateType.Jump,
            condition: _ => this.IsTryingToJump(jumpInput)
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Climb,
            to: CharacterStateType.Fall,
            condition: _ => this.IsClimbToFall()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Climb,
            to: CharacterStateType.Dash,
            condition: _ => this.IsTryingToDash(dashInput)
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

        if (input == Vector2.zero)
            input.x = Renderer.GetLookDirectionX();

        return input;
    }

    protected float GetPlayerClimbInput()
    {
        if (Managers.Control.IsPressed(Literal.Hotkeys.UpUtility))
            return 1f;

        if (Managers.Control.IsPressed(Literal.Hotkeys.DownUtility))
            return -1f;

        return 0f;
    }

    protected Vector2 GetPlayerThrowInput()
    {
        Vector2 input = Vector2.zero;

        if (Managers.Control.IsPressed(Literal.Hotkeys.UpUtility))
            input.y += 1f;

        if (Managers.Control.IsPressed(Literal.Hotkeys.DownUtility))
            input.y -= 1f;

        return input;
    }

    public bool IsPlayerJumpInput()
        => Managers.Control.IsTriggered(Literal.Hotkeys.Jump);

    public bool IsPlayerDashInput()
        => CheckDashInput(Managers.Config.Option.Access.modifierDash, !this.IsGrounded());

    protected bool IsPlayerCrouchInput()
        => Managers.Control.IsPressed(Literal.Hotkeys.DownUtility);

    private bool CheckDashInput(bool isModifierDash, bool allowDownUtility)
    {
        if (isModifierDash)
        {
            bool isDashPressed = Managers.Control.IsPressed(Literal.Hotkeys.Dash);

            if (!isDashPressed)
                return false;

            bool isLeftPressed = Managers.Control.IsPressed(Literal.Hotkeys.Left);
            bool isRightPressed = Managers.Control.IsPressed(Literal.Hotkeys.Right);
            bool isDownPressed = allowDownUtility && Managers.Control.IsPressed(Literal.Hotkeys.DownUtility);

            if (isLeftPressed || isRightPressed || isDownPressed)
                return Managers.Control.IsHoldRepeated(Literal.Hotkeys.Dash);

            return false;
        }
        else
        {
            bool triggered = Managers.Control.IsDoubleTriggered(Literal.Hotkeys.Left) || Managers.Control.IsDoubleTriggered(Literal.Hotkeys.Right);

            if (allowDownUtility)
                triggered |= Managers.Control.IsDoubleTriggered(Literal.Hotkeys.DownUtility);

            return triggered;
        }
    }

    private void TryExecuteInteraction()
    {
        if (this is ICarriableCharacter carriable && carriable.IsHoldingProp)
        {
            bool isDownPressed = Managers.Control.IsPressed(Literal.Hotkeys.DownUtility);

            if (isDownPressed)
            {
                carriable.DropPropDirectly();
                return;
            }

            StateMachine.RequestStateChange(CharacterStateType.Throw, forceInstantly: true);
            return;
        }

        if (CurrentInteractable != null && CurrentInteractable.RequireKeyInput && CurrentInteractable.CanInteract.Value)
            CurrentInteractable.ProtectedInteract(this);
    }
}
