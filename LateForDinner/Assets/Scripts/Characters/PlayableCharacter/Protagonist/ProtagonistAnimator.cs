public class ProtagonistAnimator : CharacterAnimator
{
    protected override int GetStateHash(CharacterStateType state)
    {
        return state switch
        {
            CharacterStateType.Idle => Define.Animation.Idle,
            CharacterStateType.Move => Define.Animation.Move,
            CharacterStateType.Jump => Define.Animation.Jump,
            CharacterStateType.DoubleJump => Define.Animation.DoubleJump,
            CharacterStateType.Roll => Define.Animation.Roll,
            _ => Define.Animation.Idle
        };
    }

    public void PlayDoubleJump() 
        => Play(Define.Animation.DoubleJump);
    public void PlayRoll() 
        => Play(Define.Animation.Roll);
}
