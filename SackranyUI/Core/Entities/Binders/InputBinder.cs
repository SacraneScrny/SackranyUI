using System;

using UnityEngine;
using UnityEngine.Events;

namespace SackranyUI.Core.Entities.Binders
{
    internal sealed class InputBinder<T> : IBinder
    {
        readonly Action _subscribe;
        readonly Action _unsubscribe;
        readonly UnityAction<T> _cachedAction;

        public InputBinder(Action<T> onValue,
            Action<UnityAction<T>> addListener,
            Action<UnityAction<T>> removeListener)
        {
            _cachedAction = value =>
            {
                try
                {
                    onValue(value);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            };
            _subscribe = () => addListener(_cachedAction);
            _unsubscribe = () => removeListener(_cachedAction);
        }

        public void Bind() { Unbind(); _subscribe(); }
        public void Unbind() => _unsubscribe();
    }
}
