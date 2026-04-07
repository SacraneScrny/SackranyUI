using System;
using System.Collections.Generic;

using R3;

namespace SackranyUI.Core.Events
{
    public class UIEventBus : IUIBusListener, IUIBusPublisher
    {
        readonly Dictionary<(int id, Type type), Delegate> _events = new();

        static readonly Type NoDataType = typeof(NoData);
        sealed class NoData { }

        public IDisposable Subscribe<E>(Action callback) where E : IUIEvent
        {
            SubscribeInternal((UIEvent<E>.Id, NoDataType), callback);
            return Disposable.Create(() => Unsubscribe<E>(callback));
        }

        public IDisposable Subscribe<E, T>(Action<T> callback) where E : IUIEvent
        {
            SubscribeInternal((UIEvent<E>.Id, typeof(T)), callback);
            return Disposable.Create(() => Unsubscribe<E, T>(callback));
        }
        
        void SubscribeInternal((int, Type) key, Delegate d)
        {
            if (_events.TryGetValue(key, out var existing))
            {
                _events[key] = Delegate.Combine(existing, d);
            }
            else
            {
                _events[key] = d;
            }
        }

        public void Unsubscribe<E>(Action callback) where E : IUIEvent => UnsubscribeInternal((UIEvent<E>.Id, NoDataType), callback);
        public void Unsubscribe<E, T>(Action<T> callback) where E : IUIEvent => UnsubscribeInternal((UIEvent<E>.Id, typeof(T)), callback);
        
        bool UnsubscribeInternal((int, Type) key, Delegate d)
        {
            if (!_events.TryGetValue(key, out var existing)) return false;

            var current = Delegate.Remove(existing, d);
            if (current == existing) return false;

            if (current == null)
                _events.Remove(key);
            else
                _events[key] = current;

            return true;
        }

        public bool Publish<E>() where E : IUIEvent => Publish(UIEvent<E>.Id);
        public bool Publish<E,T>(T data, bool includeNoDataChannel = false) where E : IUIEvent => Publish(UIEvent<E>.Id, data, includeNoDataChannel);
        
        public bool Publish(int id)
        {
            if (!_events.TryGetValue((id, NoDataType), out var d)) return false;

            ((Action)d)();
            return true;
        }
        public bool Publish<T>(int id, T data, bool includeNoDataChannel = false)
        {
            bool invoked = false;

            if (_events.TryGetValue((id, typeof(T)), out var d))
            {
                ((Action<T>)d)(data);
                invoked = true;
            }

            if (includeNoDataChannel &&
                _events.TryGetValue((id, NoDataType), out var dl))
            {
                ((Action)dl)();
                invoked = true;
            }

            return invoked;
        }

        public void Reset()
        {
            _events.Clear();
        }
    }
}