using System.Threading;

using Cysharp.Threading.Tasks;

namespace SackranyUI.Core.Base
{
    public interface IUITransition
    {
        UniTask Show(CancellationToken ct);
        UniTask Hide(CancellationToken ct);
    }
}
