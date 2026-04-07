using System;

using UnityEngine.Events;
using UnityEngine.UI;

namespace SackranyUI.Core.Entities.Binders
{
    internal class ActionBinder : IBinder
    {
        readonly Action _subscribe;
        readonly Action _unsubscribe;
        readonly UnityAction _cachedAction;

        public ActionBinder(Button button, Action onEvent)
        {
            _cachedAction = new UnityAction(onEvent.Invoke);
            _subscribe = () => button.onClick.AddListener(_cachedAction);
            _unsubscribe = () => button.onClick.RemoveListener(_cachedAction);
        }

        public void Bind() { Unbind(); _subscribe(); }
        public void Unbind() => _unsubscribe();
    }
}