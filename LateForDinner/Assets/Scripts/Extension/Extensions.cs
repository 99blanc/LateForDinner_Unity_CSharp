using R3;
using System;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Component = UnityEngine.Component;
using Token.EVENT;

public static class Extensions
{
    public static Prop InProp(this IAgentControl agent, Prop target) => agent.hProp = PropHelper.InProp(agent.hProp, target);

    public static Prop OutProp(this IAgentControl agent, Prop target) => agent.hProp = PropHelper.OutProp(agent.hProp, target);

    public static bool IsOppositeInput(this IAgentControl agent, float inputX, float currentDir) => InputHelper.IsOppositeInput(agent, inputX, currentDir);

    public static void BindInputEvent(this InputAction action, Action<InputAction.CallbackContext> performed, Component component) => InputHelper.BindInputEvent(action, performed, component);

    public static void BindInputEvent(this InputAction action, Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled, Component component) => InputHelper.BindInputEvent(action, performed, canceled, component);

    public static void BindViewEvent(this UIBehaviour view, Action<PointerEventData> action, ViewEvent type, Component component) => UIHelper.BindViewEvent(view, action, type, component);

    public static void BindModelEvent<T>(this ReactiveProperty<T> model, Action<T> action, Component component) => UIHelper.BindModelEvent(model, action, component);
}
