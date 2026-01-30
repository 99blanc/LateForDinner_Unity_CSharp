using UnityEngine;

public interface IAgentBehavior
{
    void Execute(Vector2 input = default);
}

public interface IAgentBehavior<in T> : IAgentBehavior where T : class, IData
{
    void Setup(IAgentControl control, T data);
}