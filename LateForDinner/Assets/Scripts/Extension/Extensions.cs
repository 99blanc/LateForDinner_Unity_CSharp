using R3;
using System;
using Token.EVENT;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Component = UnityEngine.Component;

public static class Extensions
{
    public static bool IsGrounded(this IAgentControl agent) => PhysicsHelper.IsGrounded(agent.tCollider, agent.tBody);

    public static Vector2 ToLookAt(this Vector2 current, Vector2 target = default)  => PhysicsHelper.ToLookAt(current, target);

    public static bool CheckTap(this Vector2 input, Vector2 lastDirection, float lastTime, float interval) => InputHelper.CheckTap(input, lastDirection, lastTime, interval);

    public static bool IsOppositeInput(this IAgentControl agent, float inputX, float currentDir) => InputHelper.IsOppositeInput(agent, inputX, currentDir);

    public static Prop Occupy(this IAgentControl agent, Prop target) => PropHelper.Occupy(agent, target);

    public static Prop Release(this IAgentControl agent, Prop target) => PropHelper.Release(agent, target);

    public static void BindActionMap(this InputActionMap map, Component owner, Func<InputContext> ctx, Action<InputContext> action) => InputHelper.BindActionMap(map, owner, ctx, action);

    public static void BindViewEvent(this UIBehaviour view, Action<PointerEventData> action, ViewEvent type, Component component) => UIHelper.BindViewEvent(view, action, type, component);

    public static void BindModelEvent<T>(this ReactiveProperty<T> model, Action<T> action, Component component) => UIHelper.BindModelEvent(model, action, component);
}
