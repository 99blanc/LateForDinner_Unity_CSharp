using UnityEngine;

public struct InputContext
{
    public Vector2 moveInput;
    public bool isTap;
    public bool doMove;
    public bool doJump;
    public bool canDash;
    public bool doInteract;
    public bool doSneak;
}

public class InputHelper
{
    public static bool IsOppositeInput(IAgentControl agent, float inputX, float currentDirection) => inputX != 0 && Mathf.Sign(inputX) != currentDirection;

    public static bool CheckTap(Vector2 input, ref Vector2 lastDirection, ref float lastInputTime)
    {
        if (input == Vector2.zero || input == lastDirection)
            return false;

        bool isWithinTime = (Time.time - lastInputTime) <= Define.Physics.INTERVAL;
        lastInputTime = Time.time;
        lastDirection = input;
        return isWithinTime;
    }
}
