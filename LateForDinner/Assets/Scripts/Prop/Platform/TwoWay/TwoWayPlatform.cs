using UnityEngine;

public class TwoWayPlatform : Platform
{
    public override void OnTick(IAgentControl agent) => Toggle(Evaluate(agent, true));
}
