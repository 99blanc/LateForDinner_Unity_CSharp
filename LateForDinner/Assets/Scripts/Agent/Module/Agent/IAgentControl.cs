using UnityEngine;

public interface IAgentControl : IPropHolder
{
    Rigidbody2D tBody { get; }
    CapsuleCollider2D tCollider { get; }
    Transform itemSocket { get; }
    IActionView tView { get; }
    Vector2 moveInput { get; }
    bool isIdling { get; }
    bool isGrounded { get; }
    T GetBehavior<T>() where T : IAgentBehavior;
}
