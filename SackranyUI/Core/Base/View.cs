using System;
using System.Collections.Generic;
using System.Threading;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace SackranyUI.Core.Base
{
    /// <summary>
    /// Base class for views: a <see cref="MonoBehaviour"/> on a prefab that exposes Unity
    /// components to a view model by string key, with its own initialization and task lifecycle.
    /// </summary>
    public abstract class View : MonoBehaviour
    {
        bool _isDestroyed;
        bool _isInitialized;
        CancellationTokenSource _cancellationSource;

        /// <summary>Called by the framework before bindings are built; set up key remaps here.</summary>
        public void PreInitialize()
        {
            OnBeforeBinding();
        }
        /// <summary>Override to cache components and register key remaps before binding.</summary>
        protected virtual void OnBeforeBinding() { }
        /// <summary>Called once after bindings are built, linking the view to the view model's cancellation token.</summary>
        public void Initialize(CancellationToken cancellationToken)
        {
            if (_isInitialized) return;
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.GetCancellationTokenOnDestroy());
            _isInitialized = true;
            OnInitialize();
        }
        protected virtual void OnInitialize() { }

        #region REMAP
        readonly Dictionary<object, object> _keyRemap = new();

        /// <summary>Maps a built-in binding key to a per-instance key configured in the inspector.</summary>
        protected void Remap(object from, object to) => _keyRemap[from] = to;
        /// <summary>Resolves a declared binding key to its remapped value, or returns it unchanged.</summary>
        public object RemapKey(object key) => _keyRemap.GetValueOrDefault(key, key);
        #endregion

        #region LIFECYCLE
        void Awake()
        {
            OnAwake();
        }
        protected virtual void OnAwake() { }

        void Start()
        {
            OnStart();
        }
        protected virtual void OnStart() { }

        void Update()
        {
            if (!_isInitialized) { return; }
            OnUpdate();
        }
        protected virtual void OnUpdate() { }

        void FixedUpdate()
        {
            if (!_isInitialized) { return; }
            OnFixedUpdate();
        }
        protected virtual void OnFixedUpdate() { }

        void LateUpdate()
        {
            if (!_isInitialized) { return; }
            OnLateUpdate();
        }
        protected virtual void OnLateUpdate() { }

        /// <summary>Destroys the view's GameObject once.</summary>
        public void DestroyView()
        {
            if (_isDestroyed || gameObject == null) { return; }
            Destroy(gameObject);
            _isDestroyed = true;
        }
        void OnDestroy()
        {
            _isInitialized = false;
            DisposeRunningTasks();
            _cancellationSource?.Cancel();
            _cancellationSource?.Dispose();
            OnDestroyView();
        }
        protected virtual void OnDestroyView() { }
        #endregion

        #region TASKS
        readonly HashSet<CancellationTokenSource> _runningTasks = new();

        /// <summary>Runs an async task tied to the view's lifetime; it is cancelled when the view is destroyed. Returns a source to cancel it early.</summary>
        public CancellationTokenSource StartTask(Func<CancellationToken, UniTask> taskFactory)
        {
            if (!_isInitialized) return null;
            var cts = new CancellationTokenSource();
            _runningTasks.Add(cts);
            var trackedTask = TrackTask(taskFactory, cts);
            trackedTask.Forget();
            return cts;
        }

        async UniTaskVoid TrackTask(Func<CancellationToken, UniTask> taskFactory, CancellationTokenSource cts)
        {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, _cancellationSource.Token);
            try
            {
                await taskFactory(linkedCts.Token);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                linkedCts?.Dispose();
                _runningTasks.Remove(cts);
                cts?.Dispose();
            }
        }

        void DisposeRunningTasks()
        {
            if (_runningTasks.Count == 0) return;
            var snapshot = new CancellationTokenSource[_runningTasks.Count];
            _runningTasks.CopyTo(snapshot);
            _runningTasks.Clear();
            foreach (var cts in snapshot)
            {
                try
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                catch (ObjectDisposedException) { }
            }
        }
        #endregion
    }
}
