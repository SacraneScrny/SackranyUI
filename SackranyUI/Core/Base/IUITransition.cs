using System.Threading;

using Cysharp.Threading.Tasks;

namespace SackranyUI.Core.Base
{
    /// <summary>
    /// Анимация показа/скрытия. Компоненты на префабе, реализующие интерфейс,
    /// проигрываются при <see cref="ViewModel.OpenAsync"/> / <see cref="ViewModel.CloseAsync"/>.
    /// </summary>
    public interface IUITransition
    {
        UniTask Show(CancellationToken ct);
        UniTask Hide(CancellationToken ct);
    }
}
