public interface IPoolable
{
    bool IsPooled => this.IsPooled();

    virtual void Init()
        => this.SetPooled(false);

    virtual void Get() 
        => this.SetPooled(false);

    virtual void Release()
    {
        this.SetPooled(true);
        PoolDisposableRegistry.Clear(this);
    }

    virtual void OnDestroy()
        => PoolDisposableRegistry.Clear(this);
}