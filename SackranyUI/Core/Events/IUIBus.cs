using System;

namespace SackranyUI.Core.Events
{
    /// <summary>Subscribe side of the event bus.</summary>
    public interface IUIBusListener
    {
        /// <summary>Subscribes to a parameterless event; dispose the result to unsubscribe.</summary>
        IDisposable Subscribe<E>(Action callback) where E : IUIEvent;
        /// <summary>Subscribes to an event carrying a <typeparamref name="T"/> payload; dispose the result to unsubscribe.</summary>
        IDisposable Subscribe<E, T>(Action<T> callback) where E : IUIEvent;
        /// <summary>Removes a parameterless subscription.</summary>
        void Unsubscribe<E>(Action callback) where E : IUIEvent;
        /// <summary>Removes a payload subscription.</summary>
        void Unsubscribe<E, T>(Action<T> callback) where E : IUIEvent;
    }
    /// <summary>Publish side of the event bus.</summary>
    public interface IUIBusPublisher
    {
        /// <summary>Publishes a parameterless event. Returns true if it was handled.</summary>
        public bool Publish<E>() where E : IUIEvent;
        /// <summary>Publishes an event with a payload, optionally also notifying parameterless listeners. Returns true if it was handled.</summary>
        public bool Publish<E,T>(T data, bool includeNoDataChannel = false) where E : IUIEvent;
    }
}