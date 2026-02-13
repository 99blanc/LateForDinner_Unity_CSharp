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
    void Prepare();
    void Execute(BehaviorContext context = default);
    void Terminate();
}

public interface IAgentBehavior<in T> : IAgentBehavior where T : class, IData
{
    void Setup(IAgentControl control, T data);
}

public interface IMove
{
    bool isMoving { get; set; }
    void ExecuteClimb();
}

public interface IJump : IFall
{
    bool isJumping { get; set; }
    ReactiveProperty<short> currentJumpCount { get; set; }
    void ExecuteJump();
}

public interface IFall
{
    bool isFalling { get; set; }
    void ExecuteFall();
}

public interface IDash
{
    bool isDashing { get; set; }
    ReactiveProperty<short> currentDashCount { get; set; }
    void ExecuteDash(float percent);
}

public interface IClimb
{
    Vector2 moveInput { get; set; }
    bool isClimbing { get; set; }
    void ExecuteClimb();
}

public interface ISneak
{
    bool isSneaking { get; set; }
    void ExecuteSneak();
}

public interface ITumble : IJump, ISneak
{
    bool isTumbling { get; set; }
}

public interface IPickup
{
    bool isPickuping { get; set; }
    void ExecutePickup();
}

public interface IThrow
{
    bool isThrowing { get; set; }
    void ExecuteThrow();
}
