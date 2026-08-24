using System;
using UnityEngine;
using UnityHFSM;

public abstract class PlayableCharacter : Character, IIdleable, IMovable
{
    public Rigidbody2D Rigidbody { get; private set; }

    public override void Init()
    {
        base.Init();
        CacheComponents();
        InitAttributes();
        InitStateMachine();
    }

    private void CacheComponents()
        => Rigidbody = this.GetComponentAssert<Rigidbody2D>();

    private void InitStateMachine()
    {
        StateMachine = new StateMachine<CharacterState>();
        StateMachine.AddState(CharacterState.Idle, new IdleState(this));
        StateMachine.AddState(CharacterState.Move, new MoveState(this, GetPlayerMoveInput));
        StateMachine.AddTransition(new Transition<CharacterState>(
            from: CharacterState.Idle,
            to: CharacterState.Move,
            condition: _ => IsPlayerTryingToMove()
        ));
        StateMachine.AddTransition(new Transition<CharacterState>(
            from: CharacterState.Move,
            to: CharacterState.Idle,
            condition: _ => HasPlayerStoppedMoving()
        ));
        StateMachine.SetStartState(CharacterState.Idle);
        StateMachine.Init();
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
}