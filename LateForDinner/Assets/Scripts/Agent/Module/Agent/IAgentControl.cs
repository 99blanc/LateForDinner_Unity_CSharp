using UnityEngine;

public interface IAgentControl
{
    StateMachine machine { get; }
    Rigidbody2D tBody { get; }
    CapsuleCollider2D tCollider { get; }
    Prop hProp { get; set; }
    IActionView tView { get; }
    Vector2 moveInput { get; }
    Vector2 lookAt { get; }
    bool isGrounded { get; set; }
    bool isFalling { get; set; }
    short currentJumpCount { get; set; }
    T GetBehavior<T>() where T : IAgentBehavior;
}
