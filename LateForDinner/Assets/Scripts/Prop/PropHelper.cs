public class PropHelper
{
    public static Prop InProp(Prop current, Prop target)
    {
        if (current is null) 
            return target;

        return (int)target.priority <= (int)current.priority ? target : current;
    }

    public static Prop OutProp(Prop current, Prop target) => current == target ? null : current;
}
