using Cysharp.Threading.Tasks;
using Token.PRIORITY;

public interface IAgentModule<TView, TData, TKey> where TView : class, IViewProvider where TData : IData<TKey>
{
    abstract ModulePrority priority { get; }
    StateMachine sMachine { get; }
    UniTask Setup(TData data, TView view, StateMachine machine);
}
