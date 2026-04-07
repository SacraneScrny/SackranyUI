using System;
using System.Collections.Generic;
using System.Threading;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace SackranyUI.Core.Base
{
    public abstract class View : MonoBehaviour
    {
        bool _isDestroyed;
        bool _isInitialized;
        CancellationTokenSource _cancellationSource;

        public void PreInitialize()
        {
            OnBeforeBinding();
        }
        protected virtual void OnBeforeBinding() { }
        public void Initialize(CancellationToken cancellationToken)
        {
            _cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.GetCancellationTokenOnDestroy());
            _isInitialized = true;
            OnInitialize();
        }
        protected virtual void OnInitialize() { }

        #region REMAP
        readonly Dictionary<object, object> _keyRemap = new();

        protected void Remap(object from, object to) => _keyRemap[from] = to;
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

        public void DestroyView()
        {
            if (_isDestroyed || gameObject == null) { return; }
            Destroy(gameObject);
            _isDestroyed = true;
        }
        void OnDestroy()
        {
            _isInitialized = false;    
            _cancellationSource?.Cancel();
            _cancellationSource?.Dispose();
            OnDestroyView();
        }
        protected virtual void OnDestroyView() { }
        #endregion

        #region TASKS
        public CancellationTokenSource StartTask(Func<CancellationToken, UniTask> taskFactory)
        {
            if (!_isInitialized) return null;
            var cts = new CancellationTokenSource();
            var trackedTask = TrackTask(taskFactory, cts);
            trackedTask.Forget();
            return cts;
        }

        async UniTaskVoid TrackTask(Func<CancellationToken, UniTask> taskFactory, CancellationTokenSource cts)
        {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, this._cancellationSource.Token);
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
                cts?.Dispose();
            }
        }
        #endregion
    }
}