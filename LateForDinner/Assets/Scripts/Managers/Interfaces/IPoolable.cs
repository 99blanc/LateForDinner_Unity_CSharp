public interface IPoolable
{
    virtual void Init() { }

    virtual void Get() { }

    virtual void Release() { }
}
