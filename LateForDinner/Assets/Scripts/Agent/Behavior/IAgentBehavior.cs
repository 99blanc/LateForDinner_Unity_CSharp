using UnityEngine;

public struct BehaviorContext
{
    public Vector2 input;
    public float bias;
    public float value;

    public static BehaviorContext Default => new BehaviorContext { input = Vector2.zero, bias = 0f };
}

public interface IAgentBehavior
{
    void Execute(BehaviorContext context = default);
}

public interface IAgentBehavior<in T> : IAgentBehavior where T : class, IData
{
    void Setup(IAgentControl control, T data);
}