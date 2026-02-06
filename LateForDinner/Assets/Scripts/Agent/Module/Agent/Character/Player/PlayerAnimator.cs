using Cysharp.Threading.Tasks;
using UnityEngine;
using Token.ID;
using Token.PRIORITY;

public class PlayerAnimator : AgentAnimator<IPlayerView, PlayerData, PlayerID>
{
    private static readonly int Anime_MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int Anime_IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int Anime_DashY = Animator.StringToHash("DashY");
    private static readonly int Anime_IsIdling = Animator.StringToHash("IsIdling");
    private static readonly int Anime_IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int Anime_IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int Anime_IsDashing = Animator.StringToHash("IsDashing");
    private IMove moveRef;
    private IJump jumpRef;
    private IDash dashRef;
    public override ModulePrority priority => ModulePrority.PLAYER_ANIMATOR;
    public override string cPath => Define.Asset.ANIMATOR_PLAYER;

    public override async UniTask Setup(PlayerData data, IPlayerView view, StateMachine machine)
    {
        await base.Setup(data, view, machine);
        moveRef = control as IMove;
        jumpRef = control as IJump;
        dashRef = control as IDash;
    }

    protected override void States()
    {
        RegisterState<PlayerIdleState>();
        RegisterState<PlayerMoveState>();
        RegisterState<PlayerJumpState>();
        RegisterState<PlayerFallState>();
        RegisterState<PlayerDashState>();
        RegisterState<PlayerClimbState>();
        RegisterState<PlayerSneakState>();
    }

    protected override void Parameters()
    {
        if (sMachine.curState is not null)
            PlayStateAnimation(sMachine.curState.GetType());

        lookAt = tBody.linearVelocity.ToLookAt(lookAt);
        Flip(lookAt.x);
        SetParam(Anime_IsIdling, control.isIdling);
        bool isGrounded = control.isGrounded;
        SetParam(Anime_IsGrounded, isGrounded);
        bool isMoving = moveRef.isMoving;
        SetParam(Anime_IsMoving, isMoving);
        float animeSpeed = (control.isGrounded && moveRef.isMoving && !dashRef.isDashing) ? tBody.linearVelocity.magnitude / aView.moveSpeed.CurrentValue : 0;

        if (control.isIdling || animeSpeed < Define.Physics.DEADZONE) 
            animeSpeed = 0;

        SetParam(Anime_MoveSpeed, animeSpeed);
        SetParam(Anime_MoveSpeed, animeSpeed);
        SetParam(Anime_IsJumping, jumpRef.isJumping);
        SetParam(Anime_IsDashing, dashRef.isDashing);

        if (dashRef.isDashing)
            SetParam(Anime_DashY, tBody.linearVelocity.normalized.y);
    }
}
