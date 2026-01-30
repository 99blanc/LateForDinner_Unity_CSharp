using UnityEngine;

public interface IAgentControl
{
    StateMachine machine { get; }
    Rigidbody2D tBody { get; }
    CapsuleCollider2D tCollider { get; }
    public Collider2D actCollider { get; set; }
    IActionView tView { get; }
    Vector2 moveInput { get; }
    Vector2 lookAt { get; }
    bool isNearGround { get; set; }
    short currentJumpCount { get; set; }
    T GetBehavior<T>() where T : IAgentBehavior;
}