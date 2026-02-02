using R3;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHelper
{
    public static bool IsOppositeInput(IAgentControl agent, float inputX, float currentDir) => inputX != 0 && Mathf.Sign(inputX) != currentDir;

    public static void BindInputEvent(InputAction action, Action<InputAction.CallbackContext> performed, Component component) => action.OnPerformedAsObservable().Subscribe(performed).AddTo(component);

    public static void BindInputEvent(InputAction action, Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled, Component component)
    {
        action.OnPerformedAsObservable().Subscribe(performed).AddTo(component);
        action.OnCanceledAsObservable().Subscribe(canceled).AddTo(component);
    }
}