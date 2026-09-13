public interface IPoolable
{
    bool IsPooled => this.IsPooled();

    public void ProtectedInit()
    {
        this.SetPooled(false);
        LoadState();
        OnInit();

        if (this is IAnimatableUI animatable)
        {
            animatable.InitAnimatorController();
            animatable.InitUpdateLoop();
        }
    }

    public void ProtectedGet()
    {
        this.SetPooled(false);
        LoadState();
        OnGet();
    }

    public void ProtectedRelease()
    {
        this.SetPooled(true);
        PoolDisposableRegistry.Clear(this);
        OnRelease();

        if (this is IAnimatableUI animatable)
            animatable.Reset();

        if (this is IDraggablePopup draggablePopup)
            draggablePopup.Reset();

        if (this is IDraggableSlot draggableSlot)
            draggableSlot.Reset();

        if (this is IInteractable interactable)
            interactable.Reset();

        if (this is ICrouchableCharacter crouchable)
            crouchable.Reset();

        if (this is IDashableCharacter dashable)
            dashable.Reset();

        if (this is IClimbableCharacter climbable)
            climbable.Reset();

        if (this is ICarriableCharacter carriable)
            carriable.Reset();
    }

    virtual void OnInit() { }

    virtual void OnGet() { }

    virtual void OnRelease() { }

    virtual void LoadState() { }
}