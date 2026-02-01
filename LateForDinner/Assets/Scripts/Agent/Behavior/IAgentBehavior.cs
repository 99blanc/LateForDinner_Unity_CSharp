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