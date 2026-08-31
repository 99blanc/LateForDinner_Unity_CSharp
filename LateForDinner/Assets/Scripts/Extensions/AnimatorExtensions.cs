using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public static class AnimatorExtensions
{
    public static float GetCurrentAnimatorNormalizedTime(this Animator animator, int layerIndex = 0)
    {
        if (animator == null)
            return 0f;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
        return stateInfo.normalizedTime;
    }

    public static float GetCurrentAnimatorNormalizedTime(this CharacterAnimator animator, int layerIndex = 0)
        => animator.Animator.GetCurrentAnimatorNormalizedTime(layerIndex);

    public static void SetAnimatorSpeed(this Animator animator, float speed)
    {
        if (animator != null)
            animator.speed = speed;
    }

    public static void SetAnimatorSpeed(this CharacterAnimator animator, float speed)
        => animator.Animator.SetAnimatorSpeed(speed);

    public static Animator AddAnimator(this GameObject gameObject)
    {
        if (gameObject == null)
            return null;

        if (!gameObject.TryGetComponent<Animator>(out var animator))
            animator = gameObject.AddComponent<Animator>();

        animator.runtimeAnimatorController = Managers.Resource.GetAnimatorController(Define.Animator.UIAnimator);
        return animator;
    }

    public static Animator AddAnimator(this Image component)
        => AddAnimator(component.gameObject);

    public static async UniTask AwaitForComplete(this Animator animator, int stateNameHash, int layerIndex = 0, CancellationToken cancellationToken = default)
    {
        if (animator == null) 
            return;

        animator.Play(stateNameHash, layerIndex, 0f);
        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        await UniTask.WaitUntil(() =>
        {
            if (animator == null) 
                return true;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            return stateInfo.shortNameHash != stateNameHash || stateInfo.normalizedTime >= 1.0f;
        }, PlayerLoopTiming.Update, cancellationToken);
    }
}
