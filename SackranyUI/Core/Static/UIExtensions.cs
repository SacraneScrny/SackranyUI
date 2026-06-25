using System;
using System.Linq;

using R3;

using UnityEngine.Localization;

namespace SackranyUI.Core.Static
{
    /// <summary>Convenience extensions for working with localized strings in view models.</summary>
    public static class UIExtensions
    {
        /// <summary>Subscribes to a <see cref="LocalizedString"/>, pushing the current value immediately and on every change. Returns null for empty strings (after invoking <paramref name="fallback"/>).</summary>
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
    
    /// <summary>Helper for building a <see cref="CompositeDisposable"/> from a set of disposables.</summary>
    public static class CompositeDisposableHelper
    {
        /// <summary>Creates a composite from the non-null disposables.</summary>
        public static CompositeDisposable Create(params IDisposable[] disposables)
            => new CompositeDisposable(disposables.OfType<IDisposable>());
    }
}