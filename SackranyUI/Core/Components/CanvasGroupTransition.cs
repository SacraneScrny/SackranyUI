using System.Threading;

using Cysharp.Threading.Tasks;

using SackranyUI.Core.Base;

using UnityEngine;

namespace SackranyUI.Core.Components
{
    [AddComponentMenu("Sackrany/UI/CanvasGroupTransition")]
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupTransition : MonoBehaviour, IUITransition
    {
        [SerializeField] float _duration = 0.2f;
        [SerializeField] bool _unscaledTime = true;

        CanvasGroup _group;
        CanvasGroup Group => _group != null ? _group : _group = GetComponent<CanvasGroup>();

        public UniTask Show(CancellationToken ct) => Fade(0f, 1f, ct);
        public UniTask Hide(CancellationToken ct) => Fade(Group.alpha, 0f, ct);

        async UniTask Fade(float from, float to, CancellationToken ct)
        {
            var group = Group;
            if (_duration <= 0f)
            {
                group.alpha = to;
                return;
            }

            group.alpha = from;
            var elapsed = 0f;
            while (elapsed < _duration)
            {
                ct.ThrowIfCancellationRequested();
                elapsed += _unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / _duration));
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            group.alpha = to;
        }
    }
}
