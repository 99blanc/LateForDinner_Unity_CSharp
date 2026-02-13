using Token.PRIORITY;

public class PropHelper
{
    public static Prop Occupy(IPropHolder holder, Prop target)
    {
        if (target is null) 
            return holder.props.Value.active;

        var current = holder.props.Value;
        int p = (int)target.priority;
        current = p switch
        {
            >= (int)PropPriority._GROUP_INTERACTION => SetInteraction(current, target),
            >= (int)PropPriority._GROUP_OBJECTIVE => SetObjective(current, target),
            >= (int)PropPriority._GROUP_ENVIRONMENT => SetEnvironment(current, target),
            _ => current
        };
        holder.props.Value = current;
        return current.active;
    }

    public static Prop Release(IPropHolder holder, Prop target)
    {
        if (target is null) 
            return holder.props.Value.active;

        var current = holder.props.Value;
        current = target switch
        {
            var t when current.environment == t => SetEnvironment(current, null),
            var t when current.objective == t => SetObjective(current, null),
            var t when current.interaction == t => SetInteraction(current, null),
            _ => current
        };
        holder.props.Value = current;
        return current.active;
    }

    private static PropContext SetEnvironment(PropContext ctx, Prop target) { ctx.environment = target; return ctx; }

    private static PropContext SetObjective(PropContext ctx, Prop target) { ctx.objective = target; return ctx; }

    private static PropContext SetInteraction(PropContext ctx, Prop target) { ctx.interaction = target; return ctx; }

}