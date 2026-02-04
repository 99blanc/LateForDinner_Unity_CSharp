using UnityEngine;

public interface IAgentControl : IPropHolder
{
    StateMachine machine { get; }
    Rigidbody2D tBody { get; }
    CapsuleCollider2D tCollider { get; }
    IActionView tView { get; }
    Vector2 moveInput { get; }
    Vector2 lookAt { get; }
    bool isGrounded { get; }
    T GetBehavior<T>() where T : IAgentBehavior;
}
