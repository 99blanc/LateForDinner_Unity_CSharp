public interface IData { }

public interface IData<TKey> : IData
{
    public TKey id { get; }
}
