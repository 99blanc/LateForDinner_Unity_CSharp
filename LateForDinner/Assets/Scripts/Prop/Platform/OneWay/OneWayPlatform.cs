using UnityEngine;

public class OneWayPlatform : Platform
{
    public override void OnTick(IAgentControl agent) => Toggle(Evaluate(agent, false));
}
