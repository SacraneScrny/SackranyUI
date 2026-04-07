using System;

namespace SackranyUI.Core.Events
{
    public interface IUIBusListener
    {
        public void Subscribe<E>(Action callback) where E : IUIEvent;
        public void Subscribe<E, T>(Action<T> callback) where E : IUIEvent;

        public void Unsubscribe<E>(Action callback) where E : IUIEvent;
        public void Unsubscribe<E, T>(Action<T> callback) where E : IUIEvent;
    }    
    public interface IUIBusPublisher
    {
        public bool Publish<E>() where E : IUIEvent;
        public bool Publish<E,T>(T data, bool includeNoDataChannel = false) where E : IUIEvent;
    }
}