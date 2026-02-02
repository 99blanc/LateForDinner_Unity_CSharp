using Token.PRIORITY;

public class OneWayPlatform : Platform
{
    public override PropPriority priority => PropPriority.ONEWAY_PLATFORM;

    public override void OnTick(IAgentControl agent) => Toggle(Evaluate(agent, false));
}
