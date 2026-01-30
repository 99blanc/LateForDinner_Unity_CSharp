using UnityEngine;

public class Ladder : Prop
{
    public override void OnInteract(IAgentControl agent)
    {
        if (agent is IUseLadder target && Mathf.Abs(agent.moveInput.y) > 0.1f)
            target.UseLadder();
    }
}
