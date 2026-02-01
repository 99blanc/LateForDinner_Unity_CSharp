using R3;
using System;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Component = UnityEngine.Component;
using Token.EVENT;

public static class Extensions
{
    public static void InProp(this IPropHolder agent, Prop prop) => agent.HandleProp(props => PropHelper.InProp(props, prop, agent));

    public static void OutProp(this IPropHolder agent, Prop prop) => agent.HandleProp(props => PropHelper.OutProp(props, prop, agent));

    public static Prop GetProp(this IPropHolder agent)
    {
        Prop result = null;
        agent.HandleProp(props => result = PropHelper.GetProp(props));
        return result;
    }
    public static void BindInputEvent(this InputAction action, Action<InputAction.CallbackContext> performed, Component component) => InputHelper.BindInputEvent(action, performed, component);

    public static void BindInputEvent(this InputAction action, Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled, Component component) => InputHelper.BindInputEvent(action, performed, canceled, component);

    public static void BindViewEvent(this UIBehaviour view, Action<PointerEventData> action, ViewEvent type, Component component) => UIHelper.BindViewEvent(view, action, type, component);

    public static void BindModelEvent<T>(this ReactiveProperty<T> model, Action<T> action, Component component) => UIHelper.BindModelEvent(model, action, component);
}
