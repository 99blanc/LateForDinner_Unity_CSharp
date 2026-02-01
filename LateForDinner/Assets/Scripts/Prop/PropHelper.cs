using System.Collections.Generic;

public class PropHelper
{
    public static void InProp(HashSet<Prop> props, Prop target, IPropHolder agent)
    {
        if (props.Add(target)) 
            Refresh(props, agent);
    }

    public static void OutProp(HashSet<Prop> props, Prop target, IPropHolder agent)
    {
        if (props.Remove(target)) 
            Refresh(props, agent);
    }

    private static void Refresh(HashSet<Prop> props, IPropHolder agent)
    {
        Prop best = GetProp(props);
        agent.pProp = best is not null ? best : null;
    }

    private static Prop GetProp(HashSet<Prop> props)
    {
        if (props.Count == 0) 
            return null;

        Prop prop = null;
        int priority = int.MaxValue;

        foreach (var set in props)
        {
            if ((int)set.priority < priority)
            {
                priority = (int)set.priority;
                prop = set;
            }
        }

        return prop;
    }
}
