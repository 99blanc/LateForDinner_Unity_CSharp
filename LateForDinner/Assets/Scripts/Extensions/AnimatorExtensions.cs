using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public static class AnimatorExtensions
{
    public static async UniTask<T> PlayAsync<T>(this UniTask<T> task) where T : UserInterface, IAnimatableUI
    {
        var display = await task;

        if (display != null)
            await display.PlayAsync();

        return display;
    }

    public static async UniTask<T> PlayClipAsync<T>(this UniTask<T> task, int hash, int layer = 0, float normalizedTime = 0f) where T : UserInterface, IAnimatableUI
    {
        var display = await task;

        if (display != null)
            await display.PlayClipAsync(hash, layer, normalizedTime);

        return display;
    }

    public static async UniTask PlayClipAsync(this IAnimatableUI animatable, int hash, int layer = 0, float normalizedTime = 0f)
        => await animatable.PlayClipAsync(hash, layer, normalizedTime);

    public static Animator GetAnimator(this IAnimatableUI animatable)
        => animatable.Animator;

    public static CancellationToken GetNewCancellationToken(this IAnimatableUI animatable)
        => animatable.GetNewCancellationToken();

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
