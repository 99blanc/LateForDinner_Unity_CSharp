public abstract class PlayableCharacterAnimator : CharacterAnimator
{
    protected override int GetDefaultStateHash(CharacterStateType state)
    {
        return state switch
        {
            CharacterStateType.Idle => Define.Animation.Idle,
            CharacterStateType.Move => Define.Animation.Move,
            CharacterStateType.Fall => Define.Animation.Fall,
            CharacterStateType.Crouch => Define.Animation.Crouch,
            CharacterStateType.Jump => Define.Animation.Jump,
            CharacterStateType.DoubleJump => Define.Animation.DoubleJump,
            CharacterStateType.Roll => Define.Animation.Roll,
            CharacterStateType.Dash => Define.Animation.Dash,
            CharacterStateType.DownDash => Define.Animation.DownDash,
            CharacterStateType.Climb => Define.Animation.Climb,
            _ => Define.Animation.Idle
        };
    }
}
