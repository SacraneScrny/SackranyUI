using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using R3;

using SackranyUI.Core.Entities;
using SackranyUI.Core.Events;
using SackranyUI.Core.Static;

using UnityEngine;

namespace SackranyUI.Core.Base
{
    public abstract class ViewModel 
        : IDisposable, IEquatable<ViewModel>,
            IUIBusListener, IUIBusPublisher, IContextUser
    {
        static int _globalId;
        public readonly int Id = Interlocked.Increment(ref _globalId);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void ResetId() => _globalId = 0;
        
        public CancellationTokenSource CancellationTokenSource { get; private set; }
        public bool IsInitialized { get; private set; }

        public IContextUser Context { get; private set; }
        public IUIBusListener EventListener { get; private set; }
        public IUIBusPublisher EventPublisher { get; private set; }
        
        protected Transform Root { get; private set; }
        protected IReadOnlyDictionary<string, Transform> Anchors;
        
        public bool HasContext => Context != null;
        public bool HasEventListener => EventListener != null;
        public bool HasEventPublisher => EventPublisher != null;
        public bool IsOpened { get; private set; }
        
        public void Initialize(
            IContextUser context = null,
            IUIBusListener eventListener = null, 
            IUIBusPublisher eventPublisher = null, 
            Transform root = null,
            IReadOnlyDictionary<string, Transform> anchors = null,
            CancellationToken cancellationToken = default)
        {
            Context = context;
            this.EventListener = eventListener;
            this.EventPublisher = eventPublisher;
            Root = root;
            Anchors = anchors;
            CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            IsInitialized = true;
            OnInitialized();
        }
        protected abstract void OnInitialized();

        public void Open()
        {
            if (IsOpened) return;
            IsOpened = true;
            Opened?.Invoke(this);
            OnOpened();
        }
        protected virtual void OnOpened() { }
        public void Close()
        {
            if (!IsOpened) return;
            IsOpened = false;
            Closed?.Invoke(this);
            OnClosed();
        }
        protected virtual void OnClosed() { }

        #region DISPOSE
        readonly CompositeDisposable _disposables = new();
        bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            CancellationTokenSource?.Cancel();
            CancellationTokenSource?.Dispose();
            
            var meta = ViewModelReflectionCache.GetViewModelMetadata(GetType());
            foreach (var field in meta.FieldBindings)
                (field.Field.GetValue(this) as IDisposable)?.Dispose();
            DisposeTracked();
            
            OnDispose();
            Disposed?.Invoke(this);
            _disposed = true;
        }
        protected virtual void OnDispose() { }

        protected void Track(IDisposable disposable) => _disposables.Add(disposable);
        protected void Track(params IDisposable[] disposables)
        {
            foreach (var d in disposables)
                if (d != null) _disposables.Add(d);
        }
        protected void DisposeTracked()
        {
            _disposables?.Dispose();
            _disposables?.Clear();
        }
        #endregion

        public event Action<ViewModel> Disposed;
        public event Action<ViewModel> Opened;
        public event Action<ViewModel> Closed;
        public event Action<ViewModel> Reiniting; 
        
        public override int GetHashCode() => Id;
        public bool Equals(ViewModel other)
        {
            if (other is null) return false;
            if (_disposed) return false;
            if (other._disposed) return false;
            return Id == other.Id || ReferenceEquals(this, other);
        }
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (_disposed) return false;
            if (obj.GetType() != GetType()) return false;
            return Equals((ViewModel)obj);
        }
        
        public ViewModel[] Add(IEnumerable<IViewModelTemplate> viewModels, Transform root = null) => Context?.Add(viewModels, root);
        public ViewModel Add(IViewModelTemplate viewModelTemplate, Transform root = null) => Context?.Add(viewModelTemplate, root);
        public bool Has<T>(Func<T, bool> cond = null) where T : ViewModel => Context != null && Context.Has(cond);
        public T Get<T>(Func<T, bool> cond = null) where T : ViewModel => Context?.Get(cond);
        public T[] GetAll<T>(Func<T, bool> cond = null) where T : ViewModel => Context?.GetAll(cond);
        public bool TryGet<T>(out T result, Func<T, bool> cond = null) where T : ViewModel
        {
            result = null;
            return HasContext && Context.TryGet(out result, cond);
        }
        public bool TryGetAll<T>(out T[] result, Func<T, bool> cond = null) where T : ViewModel
        {
            result = Array.Empty<T>();
            return HasContext && Context.TryGetAll(out result, cond);
        }
        
        public IDisposable Subscribe<E>(Action callback) where E : IUIEvent => EventListener.Subscribe<E>(callback);
        public IDisposable Subscribe<E, T>(Action<T> callback) where E : IUIEvent => EventListener.Subscribe<E, T>(callback);
        public void Unsubscribe<E>(Action callback) where E : IUIEvent => EventListener.Unsubscribe<E>(callback);
        public void Unsubscribe<E, T>(Action<T> callback) where E : IUIEvent => EventListener.Unsubscribe<E, T>(callback);
        public bool Publish<E>() where E : IUIEvent 
            => EventPublisher.Publish<E>();
        public bool Publish<E, T>(T data, bool includeNoDataChannel = false) where E : IUIEvent 
            => EventPublisher.Publish<E, T>(data, includeNoDataChannel);

        public Transform GetAnchorOrDefault(string key) => Anchors.GetValueOrDefault(key) ?? Root;
        protected void Reinit() => Reiniting?.Invoke(this);
    }

    public abstract class ViewModel<TTemplate> : ViewModel
        where TTemplate : IViewModelTemplate
    {
        [UITemplate] protected TTemplate Template;
    }
}