using System;
using System.Collections.Generic;
using System.Threading;

using Cysharp.Threading.Tasks;

using R3;

using SackranyUI.Core.Entities;
using SackranyUI.Core.Events;
using SackranyUI.Core.Static;

using UnityEngine;

namespace SackranyUI.Core.Base
{
    /// <summary>
    /// Base class for all view models: holds reactive state, reacts to the event bus and
    /// owns the lifecycle (initialize, open/close, dispose) of a single bound prefab.
    /// </summary>
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
        IReadOnlyList<IUITransition> _transitions;

        public bool HasContext => Context != null;
        public bool HasEventListener => EventListener != null;
        public bool HasEventPublisher => EventPublisher != null;
        public bool IsOpened { get; private set; }

        /// <summary>Wires the view model to its context, event bus, prefab transform and transitions. Called once by the framework.</summary>
        public void Initialize(
            IContextUser context = null,
            IUIBusListener eventListener = null,
            IUIBusPublisher eventPublisher = null,
            Transform root = null,
            IReadOnlyDictionary<string, Transform> anchors = null,
            CancellationToken cancellationToken = default,
            IReadOnlyList<IUITransition> transitions = null)
        {
            if (IsInitialized)
            {
                Debug.LogWarning($"[SackranyUI] {GetType().Name} (Id={Id}) is already initialized — repeated Initialize ignored.");
                return;
            }

            Context = context;
            EventListener = eventListener;
            EventPublisher = eventPublisher;
            Root = root;
            Anchors = anchors;
            _transitions = transitions;
            CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            IsInitialized = true;
            OnInitialized();
        }
        /// <summary>Called once after initialization. Subscribe to reactive members and set initial state here.</summary>
        protected abstract void OnInitialized();

        #region OPEN / CLOSE
        /// <summary>Shows the prefab immediately and raises <see cref="Opened"/>.</summary>
        public void Open()
        {
            if (IsOpened) return;
            IsOpened = true;
            Opened?.Invoke(this);
            OnOpened();
        }
        protected virtual void OnOpened() { }
        /// <summary>Hides the prefab immediately and raises <see cref="Closed"/>.</summary>
        public void Close()
        {
            if (!IsOpened) return;
            IsOpened = false;
            Closed?.Invoke(this);
            OnClosed();
        }
        protected virtual void OnClosed() { }

        /// <summary>Opens the prefab, plays show transitions and awaits <see cref="OnOpenedAsync"/>.</summary>
        public async UniTask OpenAsync()
        {
            if (IsOpened) return;
            Open();

            var token = CancellationTokenSource?.Token ?? CancellationToken.None;
            await PlayTransitions(show: true, token);
            await SafeAsync(OnOpenedAsync(token));
        }
        /// <summary>Awaits <see cref="OnClosingAsync"/>, plays hide transitions, then closes the prefab.</summary>
        public async UniTask CloseAsync()
        {
            if (!IsOpened) return;

            var token = CancellationTokenSource?.Token ?? CancellationToken.None;
            await SafeAsync(OnClosingAsync(token));
            await PlayTransitions(show: false, token);
            Close();
        }
        /// <summary>Override to run async work (e.g. loading) after the view has opened.</summary>
        protected virtual UniTask OnOpenedAsync(CancellationToken ct) => UniTask.CompletedTask;
        /// <summary>Override to run async work before the view starts closing.</summary>
        protected virtual UniTask OnClosingAsync(CancellationToken ct) => UniTask.CompletedTask;

        async UniTask PlayTransitions(bool show, CancellationToken token)
        {
            if (_transitions == null) return;
            for (int i = 0; i < _transitions.Count; i++)
            {
                var transition = _transitions[i];
                if (transition == null) continue;
                await SafeAsync(show ? transition.Show(token) : transition.Hide(token));
            }
        }

        static async UniTask SafeAsync(UniTask task)
        {
            try { await task; }
            catch (OperationCanceledException) { }
            catch (Exception e) { Debug.LogException(e); }
        }
        #endregion

        #region DISPOSE
        readonly CompositeDisposable _disposables = new();
        bool _disposed;
        /// <summary>Cancels async work, disposes bound reactive members and tracked subscriptions, then destroys the prefab via the context.</summary>
        public void Dispose()
        {
            if (_disposed) return;
            CancellationTokenSource?.Cancel();
            CancellationTokenSource?.Dispose();

            var meta = ViewModelReflectionCache.GetViewModelMetadata(GetType());
            foreach (var field in meta.FieldBindings)
                (field.Member.GetValue(this) as IDisposable)?.Dispose();
            DisposeTracked();

            OnDispose();
            Disposed?.Invoke(this);
            _disposed = true;

            Disposed = null;
            Opened = null;
            Closed = null;
            Reiniting = null;
        }
        protected virtual void OnDispose() { }

        /// <summary>Registers a subscription to be disposed automatically when this view model is disposed.</summary>
        protected void Track(IDisposable disposable) => _disposables.Add(disposable);
        /// <summary>Registers several subscriptions to be disposed automatically with this view model.</summary>
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

        /// <summary>Raised after the view model has been disposed.</summary>
        public event Action<ViewModel> Disposed;
        /// <summary>Raised when the view model opens.</summary>
        public event Action<ViewModel> Opened;
        /// <summary>Raised when the view model closes.</summary>
        public event Action<ViewModel> Closed;
        /// <summary>Raised when <see cref="Reinit"/> is requested; re-applies initial values to the view.</summary>
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

        /// <summary>Creates sibling view models from templates in the same context.</summary>
        public ViewModel[] Add(IEnumerable<IViewModelTemplate> viewModels, Transform root = null) => Context?.Add(viewModels, root);
        /// <summary>Creates a sibling view model from a template in the same context.</summary>
        public ViewModel Add(IViewModelTemplate viewModelTemplate, Transform root = null) => Context?.Add(viewModelTemplate, root);
        /// <summary>Returns true if the context holds at least one view model of type <typeparamref name="T"/> matching the predicate.</summary>
        public bool Has<T>(Func<T, bool> cond = null) where T : ViewModel => Context != null && Context.Has(cond);
        /// <summary>Returns the first view model of type <typeparamref name="T"/> matching the predicate, or null.</summary>
        public T Get<T>(Func<T, bool> cond = null) where T : ViewModel => Context?.Get(cond);
        /// <summary>Returns every view model of type <typeparamref name="T"/> matching the predicate.</summary>
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

        /// <summary>Subscribes to a parameterless event on the shared bus. Track the result to auto-dispose.</summary>
        public IDisposable Subscribe<E>(Action callback) where E : IUIEvent => EventListener.Subscribe<E>(callback);
        /// <summary>Subscribes to an event carrying a <typeparamref name="T"/> payload. Track the result to auto-dispose.</summary>
        public IDisposable Subscribe<E, T>(Action<T> callback) where E : IUIEvent => EventListener.Subscribe<E, T>(callback);
        /// <summary>Removes a parameterless event subscription.</summary>
        public void Unsubscribe<E>(Action callback) where E : IUIEvent => EventListener.Unsubscribe<E>(callback);
        /// <summary>Removes a payload event subscription.</summary>
        public void Unsubscribe<E, T>(Action<T> callback) where E : IUIEvent => EventListener.Unsubscribe<E, T>(callback);
        /// <summary>Publishes a parameterless event to the shared bus. Returns true if anyone handled it.</summary>
        public bool Publish<E>() where E : IUIEvent
            => EventPublisher.Publish<E>();
        /// <summary>Publishes an event with a <typeparamref name="T"/> payload. Set <paramref name="includeNoDataChannel"/> to also notify parameterless listeners.</summary>
        public bool Publish<E, T>(T data, bool includeNoDataChannel = false) where E : IUIEvent
            => EventPublisher.Publish<E, T>(data, includeNoDataChannel);

        /// <summary>Returns the child anchor transform registered under <paramref name="key"/>, or the prefab root if none.</summary>
        public Transform GetAnchorOrDefault(string key) => Anchors.GetValueOrDefault(key) ?? Root;
        /// <summary>Requests the context to re-apply <c>[InitBind]</c> values to the view.</summary>
        protected void Reinit() => Reiniting?.Invoke(this);
    }

    /// <summary>A view model with a strongly typed, injected <see cref="Template"/> data object.</summary>
    public abstract class ViewModel<TTemplate> : ViewModel
        where TTemplate : IViewModelTemplate
    {
        /// <summary>Serialized template data and prefab reference this view model was created from.</summary>
        [UITemplate] protected TTemplate Template;
    }
}
