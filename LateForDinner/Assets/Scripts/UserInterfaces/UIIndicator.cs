using UnityEngine;

public abstract class UIIndicator : UserInterface
{
    public virtual Vector3? GetTargetWorldPosition() 
        => null;
}
