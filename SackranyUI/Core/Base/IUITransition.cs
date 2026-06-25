using System.Threading;

using Cysharp.Threading.Tasks;

namespace SackranyUI.Core.Base
{
    /// <summary>An animated show/hide transition played by a view model during async open/close.</summary>
    public interface IUITransition
    {
        /// <summary>Plays the show (open) animation.</summary>
        UniTask Show(CancellationToken ct);
        /// <summary>Plays the hide (close) animation.</summary>
        UniTask Hide(CancellationToken ct);
    }
}
