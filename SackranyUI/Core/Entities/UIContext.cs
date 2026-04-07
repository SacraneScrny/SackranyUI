using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using SackranyUI.Core.Base;
using SackranyUI.Core.Events;
using SackranyUI.Core.Static;

using UnityEngine;

using Object = UnityEngine.Object;

namespace SackranyUI.Core.Entities
{
    [Serializable]
    public class UIContext : IContext
    {
        UIEventBus _eventBus;
        CancellationToken _cancellationToken;
        Transform _content;
        
        readonly Dictionary<int, ViewModelData> _viewModels = new ();
        readonly Dictionary<Type, List<ViewModel>> _typesToViewModels = new ();

        public void Init(Transform contentRoot, CancellationToken cancellationToken)
        {
            _eventBus = new UIEventBus();
            _cancellationToken = cancellationToken;
            _content = contentRoot;
        }

        public ViewModel[] Add(IEnumerable<IViewModelTemplate> viewModels, Transform root = null)
        {
            List<ViewModel> viewModelsList = new List<ViewModel>();
            foreach (var viewModel in viewModels)
                viewModelsList.Add(Add(viewModel, root));
            return viewModelsList.ToArray();
        }
        public ViewModel Add(IViewModelTemplate viewModelTemplate, Transform root = null)
        {
            var viewModel = viewModelTemplate.CreateViewModel();
            var id = viewModel.Id;
            var viewModelData = ViewModelFactory.Instantiate(viewModel, viewModelTemplate, root ?? _content);
            AddToCache(viewModelData, id);
            
            viewModel.Opened += (_) => SetActivePrefab(id, true);
            viewModel.Closed += (_) => SetActivePrefab(id, false);
            viewModel.Disposed += (_) => OnViewModelDisposed(id);
            viewModel.Reiniting += (_) => ReinitingViewModel(id);
            
            ViewModelFactory.Initialize(
                viewModelData, 
                this, 
                _eventBus, 
                _eventBus, 
                _cancellationToken);
            
            return viewModelData.ViewModel;
        }
        public bool Has<T>(Func<T, bool> cond = null) where T : ViewModel
        {
            if (cond == null)
                return _typesToViewModels.ContainsKey(typeof(T));
            return _typesToViewModels.TryGetValue(typeof(T), out var list) 
                   && list.OfType<T>().Where(cond).Any();
        }
        public T Get<T>(Func<T, bool> cond = null) where T : ViewModel
        {
            if (!_typesToViewModels.TryGetValue(typeof(T), out var list))
                return null;
            return cond == null 
                ? list.OfType<T>().FirstOrDefault() 
                : list.OfType<T>().FirstOrDefault(cond);
        }
        public T[] GetAll<T>(Func<T, bool> cond = null) where T : ViewModel
        {
            if (!_typesToViewModels.TryGetValue(typeof(T), out var list))
                return null;
            return cond == null 
                ? list.OfType<T>().ToArray() 
                : list.OfType<T>().Where(cond).ToArray();
        }
        public bool TryGet<T>(out T result, Func<T, bool> cond = null) where T : ViewModel
        {
            if (!_typesToViewModels.TryGetValue(typeof(T), out var list))
            {
                result = null;
                return false;
            }
            result = cond == null 
                ? list.OfType<T>().FirstOrDefault() 
                : list.OfType<T>().FirstOrDefault(cond);
            return result != null;
        }
        public bool TryGetAll<T>(out T[] result, Func<T, bool> cond = null) where T : ViewModel
        {
            if (!_typesToViewModels.TryGetValue(typeof(T), out var list))
            {
                result = Array.Empty<T>();
                return false;
            }
            result = cond == null 
                ? list.OfType<T>().ToArray() 
                : list.OfType<T>().Where(cond).ToArray();
            return result.Length > 0;
        }
        
        void OnViewModelDisposed(int id)
        {
            if (!_viewModels.TryGetValue(id, out var viewModelData)) return;
            Object.Destroy(viewModelData.Prefab);
            foreach (var binder in viewModelData.Binders)
                binder.Unbind();
            RemoveFromCache(id);
        }

        void AddToCache(ViewModelData viewModelData, int id)
        {
            _viewModels[id] = viewModelData;
            var type = viewModelData.ViewModel.GetType();
            if (!_typesToViewModels.TryGetValue(type, out var list))
            {
                list = new List<ViewModel>();
                _typesToViewModels.Add(type, list);
            }
            list.Add(viewModelData.ViewModel);
        }
        void RemoveFromCache(int id)
        {
            if (!_viewModels.TryGetValue(id, out var viewModelData)) return;
            var type = viewModelData.ViewModel.GetType();
            if (_typesToViewModels.TryGetValue(type, out var list))
            {
                list.Remove(viewModelData.ViewModel);
                if (list.Count == 0)
                    _typesToViewModels.Remove(type);
            }
            _viewModels.Remove(id);
        }

        void SetActivePrefab(int id, bool value)
        {
            if (!_viewModels.TryGetValue(id, out var viewModelData)) return;
            viewModelData.Prefab.SetActive(value);
        }
        void ReinitingViewModel(int id)
        {
            if (!_viewModels.TryGetValue(id, out var viewModelData)) return;
            UIBinder.BindInits(viewModelData.ViewModel, viewModelData.Views);
        }

        bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            
            _eventBus.Reset();
            while (_viewModels.Count > 0)
                _viewModels.First().Value.ViewModel.Dispose();
            
            _disposed = true;
        }
    }
}