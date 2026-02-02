using UnityEngine;

public struct BehaviorContext
{
    public Vector2 input;
    public float bias;
    public float value;

    public static BehaviorContext Default => new() { input = new(), bias = 0, value = 0 };
}

public interface IAgentBehavior
{
    void Execute(BehaviorContext context = default);
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

public interface IJump
{
    PlayerJumpState jumpState { get; }
    bool isJumping { get; set; }
    short currentJumpCount { get; set; }
    void ExecuteJump();
}

public interface IFall
{
    PlayerFallState fallState { get; }
    bool isFalling { get; set; }
    bool isGrounded { get; set; }
    void ExecuteFall();
}

public interface IDash
{
    PlayerDashState dashState { get; }
    bool isDashing { get; set; }
    void ExecuteDash(float percent);
}

public interface IClimb
{
    PlayerClimbState climbState { get; }
    bool isClimbing { get; set; }
    void ExecuteClimb();
}

public interface ISneak
{
    PlayerSneakState sneakState { get; }
    bool isSneaking { get; set; }
    void ExecuteSneak(float threshold);
}