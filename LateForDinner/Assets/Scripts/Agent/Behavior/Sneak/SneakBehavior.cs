using UnityEngine;

public class SneakBehavior<T> : IAgentBehavior<T> where T : class, ISneakData
{
    private T config;

    public void Setup(IAgentControl control, T data) => config = data;

    public void Execute(BehaviorContext context = default)
    {
        throw new System.NotImplementedException();
    }
}
