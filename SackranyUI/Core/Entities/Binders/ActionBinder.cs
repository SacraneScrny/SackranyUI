using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SackranyUI.Core.Entities.Binders
{
    internal sealed class ActionBinder : IBinder
    {
        readonly Action _subscribe;
        readonly Action _unsubscribe;
        readonly UnityAction _cachedAction;

        public ActionBinder(Button button, Action onEvent)
        {
            _cachedAction = () =>
            {
                try
                {
                    onEvent();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            };
            _subscribe = () => button.onClick.AddListener(_cachedAction);
            _unsubscribe = () => button.onClick.RemoveListener(_cachedAction);
        }

        public void Bind() { Unbind(); _subscribe(); }
        public void Unbind() => _unsubscribe();
    }
}
