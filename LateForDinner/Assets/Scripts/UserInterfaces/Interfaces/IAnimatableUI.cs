using Cysharp.Threading.Tasks;

public interface IAnimatableUI
{
    public virtual async UniTask PlayAsync()
        => await UniTask.CompletedTask;
}
