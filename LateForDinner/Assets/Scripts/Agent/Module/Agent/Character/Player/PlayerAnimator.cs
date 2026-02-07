using Cysharp.Threading.Tasks;
using UnityEngine;
using Token.ID;
using Token.PRIORITY;

public class PlayerAnimator : AgentAnimator<IPlayerView, PlayerData, PlayerID>
{
    private static readonly int Anime_MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int Anime_JumpCount = Animator.StringToHash("JumpCount");
    private static readonly int Anime_DashY = Animator.StringToHash("DashY");
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

        if (moveRef.isMoving)
        {
            float speedRatio = tBody.linearVelocity.magnitude / aView.moveSpeed.CurrentValue;
            SetParam(Anime_MoveSpeed, Mathf.Clamp(speedRatio, Define.Physics.HALF, Define.Physics.DOUBLE));
        }
        else
            SetParam(Anime_MoveSpeed, Define.Physics.FULL);

        if (jumpRef.isJumping)
            SetParam(Anime_JumpCount, (float)jumpRef.currentJumpCount.Value);

        if (dashRef.isDashing)
            SetParam(Anime_DashY, control.moveInput.y);
    }
}
