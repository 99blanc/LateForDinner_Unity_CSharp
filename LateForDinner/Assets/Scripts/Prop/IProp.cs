using System;
using System.Collections.Generic;
using UnityEngine;
using Token.PRIORITY;

public interface IPropHolder
{
    Prop pProp { get; set; }
    void HandleProp(Action<HashSet<Prop>> action);
}

public interface IProp
{
    PropPriority priority { get; }
    void OnTick(IAgentControl agent);
    void OnInteract(IAgentControl agent);
    void OnDetach(IAgentControl agent);
}

public interface IClimbProp : IProp
{
    float centerX { get; }
    Bounds bounds { get; }
}

public interface IPlatformProp : IProp { }

public interface IPushProp : IProp { }
