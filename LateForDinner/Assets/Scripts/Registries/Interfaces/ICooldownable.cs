public interface ICooldownable
{
    string ID { get; set; }
    float CooldownTime { get; set; }
    float CurrentCooldown { get; set; }
    bool IsOnCooldown { get; set; }

    void TickCooldown(float deltaTime);

    void OnCooldownComplete();
}
