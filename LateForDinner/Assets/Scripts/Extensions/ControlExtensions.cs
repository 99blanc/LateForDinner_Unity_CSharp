using System;

public static class ControlExtensions
{
    public static IDisposable BindKey(this IPoolable owner, string actionName, InputEventType inputType = InputEventType.Triggered, Action onPerformed = null, float optionValue = Define.Scaler.Threshold)
        => Managers.Control.Subscribe(owner, actionName, inputType, onPerformed, optionValue);

    public static bool IsKeyPressed(this IPoolable owner, string actionName)
        => Managers.Control.IsPressed(owner, actionName);

    public static bool IsKeyTriggered(this IPoolable owner, string actionName)
        => Managers.Control.IsTriggered(owner, actionName);

    public static bool IsKeyDoubleTriggered(this IPoolable owner, string actionName, float threshold = Define.Scaler.Threshold)
        => Managers.Control.IsDoubleTriggered(owner, actionName, threshold);

    public static bool IsKeyHoldRepeated(this IPoolable owner, string actionName, float interval = Define.Scaler.Threshold)
        => Managers.Control.IsHoldRepeated(owner, actionName, interval);
}
