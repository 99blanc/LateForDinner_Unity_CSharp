public interface ICooldownable
{
    float CooldownTime { get; set; }
    float CurrentCooldown { get; set; }
    bool IsOnCooldown { get; set; }

    public void TickCooldown(float deltaTime)
    {
        if (!IsOnCooldown) 
            return;

        CurrentCooldown -= deltaTime;

        if (CurrentCooldown <= 0f)
        {
            CurrentCooldown = 0f;
            IsOnCooldown = false;
            OnCooldownComplete();
            Managers.Cooldown.Unregister(this);
        }
    }

    void OnCooldownComplete();
}