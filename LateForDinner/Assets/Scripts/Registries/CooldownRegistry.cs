using System;

public class CooldownRegistry : ICooldownable
{
    private Action _onComplete;
    public float CooldownTime { get; set; }
    public float CurrentCooldown { get; set; }
    public bool IsOnCooldown { get; set; }

    public CooldownRegistry(Action onComplete = null)
        => _onComplete = onComplete;

    public void OnCooldownComplete()
    {
        IsOnCooldown = false;
        _onComplete?.Invoke();
    }
}
