using R3;
using UnityEngine;

public struct BehaviorContext
{
    public Vector2 input;
    public float scala;

    public static BehaviorContext Default => new() { input = new(), scala = 0 };
}

public interface IAgentBehavior
{
    void Prepare(BehaviorContext context = default);
    void Execute(BehaviorContext context = default);
    void Terminate(BehaviorContext context = default);
}

public interface IAgentBehavior<in T> : IAgentBehavior where T : class, IData
{
    void Setup(IAgentControl control, T data);
}

public interface IMove
{
    PlayerMoveState moveState { get; }
    bool isMoving { get; set; }
    void ExecuteClimb();
}

public interface IJump : IFall
{
    PlayerJumpState jumpState { get; }
    bool isJumping { get; set; }
    ReactiveProperty<short> currentJumpCount { get; set; }
    void ExecuteJump();
}

public interface IFall
{
    PlayerFallState fallState { get; }
    bool isFalling { get; set; }
    void ExecuteFall(bool tumble);
}

public interface IDash
{
    PlayerDashState dashState { get; }
    bool isDashing { get; set; }
    ReactiveProperty<short> currentDashCount { get; set; }
    void ExecuteDash(float percent);
}

public interface IClimb
{
    PlayerClimbState climbState { get; }
    Vector2 moveInput { get; set; }
    bool isClimbing { get; set; }
    void ExecuteClimb();
}

public interface ISneak
{
    PlayerSneakState sneakState { get; }
    bool isSneaking { get; set; }
    void ExecuteSneak();
}

public interface ITumble : IJump, ISneak
{
    bool isTumbling { get; set; }
}
