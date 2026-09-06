using Cysharp.Text;
using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public interface IAnimatableUI
{
    private static readonly ConditionalWeakTable<IAnimatableUI, AnimatableValue> _animatableValue = new ConditionalWeakTable<IAnimatableUI, AnimatableValue>();
    private class AnimatableValue
    {
        public Animator Animator;
        public bool IsControllerLoaded = false;
        public CancellationTokenSource Token;
    }
    public Animator Animator
    {
        get
        {
            var val = _animatableValue.GetOrCreateValue(this);

            if (val.Animator == null && this is MonoBehaviour mono)
                val.Animator = mono.GetComponentAssert<Animator>();

            return val.Animator;
        }
        set
        {
            var val = _animatableValue.GetOrCreateValue(this);
            val.Animator = value;
        }
    }

    public CancellationToken GetNewCancellationToken()
    {
        var val = _animatableValue.GetOrCreateValue(this);
        val.Token?.Cancel();
        val.Token?.Dispose();
        val.Token = new CancellationTokenSource();
        return val.Token.Token;
    }

    public void InitAnimatorController()
    {
        var val = _animatableValue.GetOrCreateValue(this);

        if (Animator == null || val.IsControllerLoaded)
            return;

        if (this is MonoBehaviour mono)
        {
            string path = ZString.Concat(mono.GetType().Name, Literal.Assets.Animator);
            RuntimeAnimatorController controller = Managers.Resource.GetAnimatorController(path);

            if (controller != null)
            {
                Animator.runtimeAnimatorController = controller;
                Animator.enabled = false;
                val.IsControllerLoaded = true;
            }
        }
    }

    public virtual void InitUpdateLoop()
    {
        if (this is MonoBehaviour mono)
        {
            mono.UpdateAsObservable()
            .Subscribe(_ => OnUpdate())
            .RegisterToPool(mono as IPoolable);
        }
    }

    public void OnUpdate() { }

    public virtual async UniTask PlayAsync()
        => await UniTask.CompletedTask;

    public virtual async UniTask PlayClipAsync(int hash, int layer = 0, float normalizedTime = 0f)
    {
        if (Animator != null)
        {
            Animator.enabled = true;
            Animator.SetActive(true);
            CancellationToken cts = GetNewCancellationToken();
            Animator.Play(hash, layer, normalizedTime);

            try
            {
                await Animator.AwaitForComplete(hash, layer, cts);
            }
            catch (OperationCanceledException)
            {
                // DESC ::: 인터럽트 발생 시 정상 탈출
                Animator.Play(Define.Animation.None);
            }
            finally
            {
                Animator.Play(Define.Animation.None);
                Animator.enabled = false;
            }
        }
    }
}
