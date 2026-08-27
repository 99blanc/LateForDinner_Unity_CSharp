using System;
using UnityEngine;
using UnityHFSM;

public abstract class PlayableCharacter : Character, IIdleableCharacter, IMovableCharacter
{
    public Rigidbody2D Rigidbody { get; private set; }
    public Transform BackTransform { get; private set; }
    public Transform FrontTransform { get; private set; }
    public Transform HitboxTransform { get; private set; }

    public override void Init()
    {
        base.Init();
        InitAttributes();
        CacheComponents();
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
        fsm.AddState(CharacterStateType.Idle, new IdleState(this));
        fsm.AddState(CharacterStateType.Move, new MoveState(this, GetPlayerMoveInput));
    }

    protected override void RegisterTransitions(StateMachine<CharacterStateType> fsm)
    {
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Idle,
            to: CharacterStateType.Move,
            condition: _ => IsPlayerTryingToMove()
        ));
        fsm.AddTransition(new Transition<CharacterStateType>(
            from: CharacterStateType.Move,
            to: CharacterStateType.Idle,
            condition: _ => HasPlayerStoppedMoving()
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

    private float GetPlayerMoveInput()
    {
        if (Managers.Control.IsPressed(Literal.Hotkeys.Right)) 
            return 1f;

        if (Managers.Control.IsPressed(Literal.Hotkeys.Left)) 
            return -1f;

        return 0f;
    }

    private bool IsPlayerTryingToMove()
        => Mathf.Abs(GetPlayerMoveInput()) > 0.01f;

    private bool HasPlayerStoppedMoving()
    {
        bool hasNoInput = Mathf.Abs(GetPlayerMoveInput()) <= 0.01f;
        bool hasNoVelocity = Mathf.Abs(Rigidbody.linearVelocity.x) < 0.1f;
        return hasNoInput && hasNoVelocity;
    }
}