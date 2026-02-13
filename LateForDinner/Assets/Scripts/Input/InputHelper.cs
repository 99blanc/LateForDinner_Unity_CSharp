using R3;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public struct InputContext
{
    public Vector2 moveInput;
    public bool isTap;
    public bool doMove;
    public bool doJump;
    public bool canDash;
    public bool doClimb;
    public bool doSneak;
    public bool doTumble;
    public bool doInteract;
}

public class InputHelper
{
    public static bool IsOppositeInput(IAgentControl agent, float inputX, float currentDirection) => inputX != 0 && Mathf.Sign(inputX) != currentDirection;

    public static bool CheckTap(Vector2 input, Vector2 lastDirection, float lastTime, float interval)
    {
        if (input == Vector2.zero || lastDirection == Vector2.zero) 
            return false;

        return Time.time - lastTime > Define.Physics.DEADZONE && Time.time - lastTime <= interval && input == lastDirection;
    }

    public static void BindActionMap(InputActionMap map, Component owner, Func<InputContext> ctx, Action<InputContext> action)
    {
        var streams = map.actions.Select(action =>
            Observable.Merge(action.OnPerformedAsObservable(), action.OnCanceledAsObservable())
        );
        Observable.Merge(streams).Select(_ => ctx()).Subscribe(ctx => action(ctx));
    }
}