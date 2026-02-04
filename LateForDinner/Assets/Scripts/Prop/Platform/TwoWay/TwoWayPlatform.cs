using Token.PRIORITY;

public class TwoWayPlatform : Platform
{
    public override bool dropable => true;
    public override PropPriority priority => PropPriority.TWOWAY_PLATFORM;

    public override void OnTick(IAgentControl agent) => SetIgnore(agent, !Evaluate(agent));
}
