using R3;

public struct PropContext
{
    public Prop environment;
    public Prop objective;
    public Prop interaction;
    public Prop active => interaction ?? (objective ?? environment);
}

public interface IPropHolder
{
    public ReactiveProperty<PropContext> props { get; }
    public Prop active => props.Value.active;
}
