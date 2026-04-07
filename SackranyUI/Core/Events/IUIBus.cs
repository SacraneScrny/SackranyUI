using System;

namespace SackranyUI.Core.Events
{
    public interface IUIBusListener
    {
        IDisposable Subscribe<E>(Action callback) where E : IUIEvent;
        IDisposable Subscribe<E, T>(Action<T> callback) where E : IUIEvent;
        void Unsubscribe<E>(Action callback) where E : IUIEvent;
        void Unsubscribe<E, T>(Action<T> callback) where E : IUIEvent;
    }    
    public interface IUIBusPublisher
    {
        public bool Publish<E>() where E : IUIEvent;
        public bool Publish<E,T>(T data, bool includeNoDataChannel = false) where E : IUIEvent;
    }
}