using Token.PRIORITY;

public interface IAgentModule<TView, TData, TKey> where TView : class, IViewProvider where TData : IData<TKey>
{
    ModulePrority priority { get; }
    void Setup(TData data, TView view);
}
