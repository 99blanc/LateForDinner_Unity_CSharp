using UnityEngine;

public interface IAgentAnimator
{
    Rigidbody2D tBody { get; }
    CapsuleCollider2D tCollider { get; }
    Vector2 lookAt { get; }
    IAgentControl control { get; }
    IAgentView aView { get; }
    abstract string cPath { get; }
    void Play(int hash, float transition = Define.Physics.BUFFER);
    void SetParam(int hash, float value);
    void SetParam(int hash, bool value);
    void SetParam(int hash, int value);
}
