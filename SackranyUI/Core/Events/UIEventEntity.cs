namespace SackranyUI.Core.Events
{
    /// <summary>A typed event channel identified by a stable runtime id.</summary>
    public interface IUIEvent
    {
        /// <summary>Stable runtime id for this event type.</summary>
        int Id { get; }
        /// <summary>Shared cached instance of this event type.</summary>
        IUIEvent Instance { get; }
    }

    /// <summary>Base class for events. Derive as <c>class MyEvent : AUIEvent&lt;MyEvent&gt; { }</c>.</summary>
    public abstract class AUIEvent<TSelf> : IUIEvent
        where TSelf : AUIEvent<TSelf>
    {
        /// <inheritdoc/>
        public int Id => UIEventRegistry.GetId<TSelf>();
        /// <inheritdoc/>
        public IUIEvent Instance => UIEventRegistry.GetInstance<TSelf>();

        protected AUIEvent() { }
    }

    /// <summary>Cached id and instance lookup for the event type <typeparamref name="T"/>.</summary>
    public readonly struct UIEvent<T> where T : IUIEvent
    {
        /// <summary>Stable runtime id of <typeparamref name="T"/>.</summary>
        public static readonly int Id = UIEventRegistry.GetId<T>();
        /// <summary>Shared cached instance of <typeparamref name="T"/>.</summary>
        public static readonly IUIEvent Instance = UIEventRegistry.GetInstance<T>();
    }
}