using Token.PRIORITY;

public class OneWayPlatform : Platform
{
    public override bool dropable => false;
    public override PropPriority priority => PropPriority.ONEWAY_PLATFORM;

    public override void OnTick(IAgentControl agent) => SetIgnore(agent, !Evaluate(agent));
}
