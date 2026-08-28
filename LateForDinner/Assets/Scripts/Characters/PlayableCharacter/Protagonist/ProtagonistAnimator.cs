public class ProtagonistAnimator : CharacterAnimator
{
    protected override int GetStateHash(CharacterStateType state)
    {
        return state switch
        {
            CharacterStateType.Idle => Define.Animation.Idle,
            CharacterStateType.Move => Define.Animation.Move,
            CharacterStateType.Fall => Define.Animation.Fall,
            CharacterStateType.Jump => Define.Animation.Jump,
            CharacterStateType.DoubleJump => Define.Animation.DoubleJump,
            CharacterStateType.Roll => Define.Animation.Roll,
            CharacterStateType.Dash => Define.Animation.Dash,
            CharacterStateType.DownDash => Define.Animation.DownDash,
            _ => Define.Animation.Idle
        };
    }

    public void PlayDoubleJump() 
        => Play(Define.Animation.DoubleJump);
    public void PlayDownDash()
        => Play(Define.Animation.DownDash);
}
