using System;
using UnityEngine;
using UnityHFSM;

public static class CharacterTransitionExtensions
{
    private static bool IsHoldingProp(this Character character)
        => character is ICarriableCharacter carriable && carriable.IsHoldingProp;

    public static bool IsTryingToMove(this Character character, Func<float> getMoveInput)
        => Mathf.Abs(getMoveInput()) > 0.01f;

    public static bool HasStoppedMoving(this Character character, Func<float> getMoveInput, Rigidbody2D rb)
        => Mathf.Abs(getMoveInput()) <= 0.01f && Mathf.Abs(rb.linearVelocity.x) < 0.1f;

    public static bool ShouldFallFromIdle(this Character character, StateMachine<CharacterStateType> fsm, Rigidbody2D rb)
        => fsm.ActiveStateName != CharacterStateType.Dash && !character.IsGrounded() && rb.linearVelocity.y < -0.1f;

    public static bool ShouldFallFromAirborne(this Character character, Rigidbody2D rb)
        => !character.IsGrounded() && rb.linearVelocity.y < -0.1f;

    public static bool IsTryingToCrouch(this Character character, Func<bool> getDownInput)
    {
        if (character.IsHoldingProp())
            return false;

        return character.IsGrounded() && getDownInput();
    }

    public static bool IsCrouchToIdle(this Character character, Func<float> getMoveInput, Func<bool> getDownInput)
        => !getDownInput() && Mathf.Abs(getMoveInput()) <= 0.01f;

    public static bool IsCrouchToMove(this Character character, Func<float> getMoveInput, Func<bool> getDownInput)
        => !getDownInput() && Mathf.Abs(getMoveInput()) > 0.01f;

    public static bool IsTryingToJump(this Character character, Func<bool> getJumpInput)
    {
        if (!getJumpInput())
            return false;

        bool hasJumpCount = character is IJumpableCharacter jumpable && jumpable.RemainJumpCount > 0;
        return hasJumpCount;
    }

    public static bool IsLandingToIdle(this Character character, Func<float> getMoveInput)
    {
        if (character.IsGrounded())
        {
            if (character is IJumpableCharacter jumpable)
                jumpable.RemainJumpCount = jumpable.MaxJumpCount;

            return Mathf.Abs(getMoveInput()) <= 0.01f;
        }

        return false;
    }

    public static bool IsLandingToMove(this Character character, Func<float> getMoveInput)
    {
        if (character.IsGrounded())
        {
            if (character is IJumpableCharacter jumpable)
                jumpable.RemainJumpCount = jumpable.MaxJumpCount;

            return Mathf.Abs(getMoveInput()) > 0.01f;
        }

        return false;
    }

    public static bool IsPlayerReadyToRoll(this Character character)
    {
        if (character.IsHoldingProp())
            return false;

        bool isLastJump = character is IJumpableCharacter jumpable && jumpable.RemainJumpCount == 0;
        bool isAnimationReady = character.CharacterAnimator is PlayableCharacterAnimator playableAnimator && playableAnimator.GetCurrentAnimatorNormalizedTime() >= Define.Animation.NormalizedTime;
        return isLastJump && isAnimationReady;
    }

    public static bool IsRollToIdle(this Character character, Func<float> getMoveInput)
        => character.IsRollFinishedAndGrounded() && Mathf.Abs(getMoveInput()) <= 0.01f;

    public static bool IsRollToMove(this Character character, Func<float> getMoveInput)
        => character.IsRollFinishedAndGrounded() && Mathf.Abs(getMoveInput()) > 0.01f;

    public static bool IsRollFinishedAndGrounded(this Character character)
        => character.CharacterAnimator.GetCurrentAnimatorNormalizedTime() >= 1f && character.IsGrounded();

    public static bool IsRollFinishedAndAirborne(this Character character)
        => character.CharacterAnimator.GetCurrentAnimatorNormalizedTime() >= 1f && !character.IsGrounded();

    public static bool IsTryingToDash(this Character character, Func<bool> getDashInput)
    {
        if (character.IsHoldingProp())
            return false;

        if (character is IDashableCharacter dashable && dashable.IsOnCooldown)
            return false;

        return getDashInput();
    }

    public static bool IsDashFinishedAndGrounded(this Character character)
        => character is IDashableCharacter dashable && dashable.IsDurationEnded && character.IsGrounded();

    public static bool IsDashFinishedAndAirborne(this Character character)
        => character is IDashableCharacter dashable && dashable.IsDurationEnded && !character.IsGrounded();

    public static bool IsTryingToClimb(this Character character, Func<float> getVerticalInput, Func<bool> getDownInput)
    {
        if (character.IsHoldingProp() || character.CurrentInteractable?.PropKey != PropKey.Ladder)
            return false;

        float vertical = getVerticalInput();

        if (Mathf.Abs(vertical) <= 0.01f)
            return false;

        return vertical > 0f || (character.IsGrounded() && getDownInput());
    }

    public static bool IsTryingToLeaveClimb(this Character character, Func<float> getMoveInput, Func<float> getVerticalInput)
    {
        if (character is IClimbableCharacter climbable && climbable.IsExitLocked)
            return false;

        bool isTryingToMoveHorizontally = Mathf.Abs(getMoveInput()) > 0.01f;
        bool isNotPressingVertical = Mathf.Abs(getVerticalInput()) <= 0.01f;
        return isTryingToMoveHorizontally && isNotPressingVertical;
    }

    public static bool IsClimbToIdle(this Character character, Func<float> getMoveInput, Func<float> getVerticalInput)
    {
        if (character is IClimbableCharacter climbable && climbable.IsExitLocked)
            return false;

        bool isNotPressingVertical = Mathf.Abs(getVerticalInput()) <= 0.01f;
        bool isNoMoveInput = Mathf.Abs(getMoveInput()) <= 0.01f;
        return character.CurrentInteractable == null || (isNotPressingVertical && isNoMoveInput && character.IsGrounded());
    }

    public static bool IsClimbToFall(this Character character)
    {
        if (character is IClimbableCharacter climbable && climbable.IsExitLocked)
            return false;

        return character.CurrentInteractable == null && !character.IsGrounded();
    }

    public static bool IsClimbToGroundIdle(this Character character, Func<float> getMoveInput)
    {
        if (character is IClimbableCharacter climbable)
        {
            if (climbable.IsExitLocked)
                return false;

            return climbable.CurrentLadder == null && Mathf.Abs(getMoveInput()) <= 0.01f && character.IsGrounded();
        }

        return false;
    }

    public static bool IsClimbToGroundMove(this Character character, Func<float> getMoveInput)
    {
        if (character is IClimbableCharacter climbable)
        {
            if (climbable.IsExitLocked)
                return false;

            return climbable.CurrentLadder == null && Mathf.Abs(getMoveInput()) > 0.01f && character.IsGrounded();
        }

        return false;
    }

    public static void AddAirActionsForLadder(this Character character)
    {
        if (character is IJumpableCharacter jumpable)
            jumpable.RemainJumpCount = Mathf.Min(jumpable.RemainJumpCount + 1, jumpable.MaxJumpCount);

        if (character is IDashableCharacter dashable)
            dashable.RemainDashCount = Mathf.Min(dashable.RemainDashCount + 1, dashable.MaxDashCount);
    }
}
