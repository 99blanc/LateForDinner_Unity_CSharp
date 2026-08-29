public interface IPoolable
{
    bool IsPooled => this.IsPooled();

    public void ProtectedInit()
    {
        this.SetPooled(false);
        LoadState();
        OnInit();
    }

    public void ProtectedRelease()
    {
        this.SetPooled(true);
        PoolDisposableRegistry.Clear(this);
        OnRelease();
    }

    public void ProtectedGet()
    {
        this.SetPooled(false);
        LoadState();
        OnGet();
    }

    virtual void OnInit() { }

    virtual void OnGet() { }

    virtual void OnRelease() { }

    virtual void LoadState() { }
}