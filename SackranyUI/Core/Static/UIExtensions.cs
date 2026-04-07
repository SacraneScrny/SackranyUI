using System;
using System.Linq;

using R3;

using UnityEngine.Localization;

namespace SackranyUI.Core.Static
{
    public static class UIExtensions
    {
        public static IDisposable Subscribe(this LocalizedString str, LocalizedString.ChangeHandler handler, string fallback = "")
        {
            if (str.IsEmpty)
            {
                handler(fallback);
                return null;
            }
            str.StringChanged += handler;
            handler(str.GetLocalizedString());
            return Disposable.Create(() => str.StringChanged -= handler);
        }
    }
    
    public static class CompositeDisposableHelper
    {
        public static CompositeDisposable Create(params IDisposable[] disposables)
            => new CompositeDisposable(disposables.OfType<IDisposable>());
    }
}