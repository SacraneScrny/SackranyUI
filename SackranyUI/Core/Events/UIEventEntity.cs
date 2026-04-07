namespace SackranyUI.Core.Events
{
    public interface IUIEvent
    {
        int Id { get; }
        IUIEvent Instance { get; }
    }

    public abstract class AUIEvent<TSelf> : IUIEvent
        where TSelf : AUIEvent<TSelf>
    {
        public int Id => UIEventRegistry.GetId<TSelf>();
        public IUIEvent Instance => UIEventRegistry.GetInstance<TSelf>();
        
        protected AUIEvent() { }
    }

    public readonly struct UIEvent<T> where T : IUIEvent
    {
        public static readonly int Id = UIEventRegistry.GetId<T>();
        public static readonly IUIEvent Instance = UIEventRegistry.GetInstance<T>();
    }
}