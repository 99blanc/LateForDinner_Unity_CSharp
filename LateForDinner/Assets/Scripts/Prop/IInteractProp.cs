using System;
using System.Collections.Generic;
using Token.PRIORITY;
using UnityEngine;

public interface IPropHolder
{
    BoxCollider2D pProp { get; set; }
    void HandleProp(Action<HashSet<Prop>> action);
}

public interface IInteractProp
{
    PropPriority priority { get; }
    void OnTick(IAgentControl agent);
    void OnInteract(IAgentControl agent);
    void OnDetach(IAgentControl agent);
}