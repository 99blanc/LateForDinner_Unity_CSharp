using R3;
using System;
using UnityEngine;

public class DashBehavior<T> : IAgentBehavior<T> where T : class, IDashData
{
    private IAgentControl agent;
    private T config;
    private bool isCoolingDown;
    private Vector2 startPos;
    private Vector2 targetPos;
    public float duration { get; private set; }
    public float direction { get; private set; }

    public void Setup(IAgentControl control, T data)
    {
        agent = control;
        config = data;
    }

    public void Prepare(BehaviorContext context)
    {
        agent.tBody.gravityScale = 0;
        agent.tBody.linearVelocity = Vector2.zero;
        float dashDist = agent.tView.dashDistance.CurrentValue;
        float dashSpeed = agent.tView.moveSpeed.CurrentValue * config.dashSpeed;
        duration = dashDist / dashSpeed;
        direction = Mathf.Sign(agent.lookAt.x);
        int isDown = agent.lookAt.y < 0 ? 1 : 0;
        Vector2 dashDir = new(direction * (1f - isDown), -1f * isDown);
        startPos = agent.tBody.position;
        targetPos = startPos + (dashDir * dashDist);
        Use();
    }

    public void Execute(BehaviorContext context)
    {
        Vector2 nextPos = Vector2.Lerp(startPos, targetPos, context.scala);
        Vector2 currentPos = agent.tBody.position;
        Vector2 moveDir = (nextPos - currentPos).normalized;
        Vector2 castSize = agent.tCollider.bounds.size * Define.Physics.LIMIT;
        RaycastHit2D hit = Physics2D.BoxCast(currentPos, castSize, 0, moveDir, Vector2.Distance(currentPos, nextPos), Define.Layer.GROUND_MASKS);

        if (hit.collider == null || hit.collider.isTrigger)
            agent.tBody.MovePosition(nextPos);
        else
        {
            agent.tBody.angularVelocity = 0;
            agent.tBody.linearVelocity = Vector2.zero;
            Vector2 snapPos = currentPos + moveDir * Mathf.Max(0, hit.distance - Define.Physics.DEADZONE);
            startPos = targetPos = snapPos;
            agent.tBody.MovePosition(snapPos);
        }
    }

    public void Terminate(BehaviorContext context)
    {
        agent.tBody.gravityScale = Define.Physics.FULL;
        agent.tBody.linearVelocity = Vector2.zero;
    }

    public bool CanDash(bool dashRequested, Vector2 input)
    {
        if (agent is not IDash dash) 
            return false;

        bool hasCount = dash.currentDashCount.Value < agent.tView.dashCount.CurrentValue;
        bool statReady = !isCoolingDown && hasCount;
        bool isForbidden = (agent.isGrounded && input.y < 0) || input.y > 0;
        return dashRequested && statReady && !isForbidden;
    }

    public bool IsFinished(float scala) => scala >= Define.Physics.FULL || startPos == targetPos;

    public bool IsCanceled(Vector2 moveInput)
    {
        if (moveInput.x == 0 || !agent.IsOppositeInput(moveInput.x, direction)) 
            return false;

        float inputDir = moveInput.x > 0 ? 1f : -1f;
        float dashDir = direction > 0 ? 1f : -1f;
        return inputDir != dashDir;
    }

    private void Use()
    {
        if (agent is not IDash dash)
            return;

        ++dash.currentDashCount.Value;

        if (dash.currentDashCount.Value < agent.tView.dashCount.CurrentValue || isCoolingDown)
            return;

        isCoolingDown = true;
        Observable.Timer(TimeSpan.FromSeconds(agent.tView.dashCooltime.CurrentValue))
            .Subscribe(_ =>
            {
                dash.currentDashCount.Value = 0;
                isCoolingDown = false;
            }).AddTo(agent.tBody.gameObject);
    }
}