using Token.PRIORITY;

public class TwoWayPlatform : Platform
{
    public override PropPriority priority => PropPriority.TWOWAY_PLATFORM;

    public override void OnTick(IAgentControl agent) => Toggle(Evaluate(agent, true));
}
