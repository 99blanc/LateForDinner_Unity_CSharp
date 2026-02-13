using UnityEngine;
using Token.PRIORITY;

public interface IProp
{
    abstract PropPriority priority { get; }
    Collider2D sensor { get; }
    Transform rTransform { get; }
    GameObject rGameObject { get; }
    void OnTick(IAgentControl agent);
    void OnInteract(IAgentControl agent);
    void OnDetach(IAgentControl agent);
    void SetActive(bool active);
}

public interface IClimbProp : IProp
{
    float centerX { get; }
    Bounds bounds { get; }
}

public interface IPlatformProp : IProp 
{
    bool dropable { get; }
}

public interface IBoxProp : IProp { }

public interface IPickupProp : IProp 
{
    bool CanPickup(IAgentControl agent);
}

public interface  IThrowProp : IPickupProp
{
    bool CanThrow(IAgentControl agent);
}
