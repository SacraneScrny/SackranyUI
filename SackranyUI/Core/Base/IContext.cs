using System;
using System.Collections.Generic;

using UnityEngine;

namespace SackranyUI.Core.Base
{
    public interface IContext
    {
        public ViewModel[] Add(IEnumerable<IViewModelTemplate> viewModels, Transform root);
        public ViewModel Add(IViewModelTemplate viewModelTemplate, Transform root);

        public bool Has<T>(Func<T, bool> cond = null) where T : ViewModel;
        public T Get<T>(Func<T, bool> cond = null) where T : ViewModel;
        public T[] GetAll<T>(Func<T, bool> cond = null) where T : ViewModel;
        public bool TryGet<T>(out T result, Func<T, bool> cond = null) where T : ViewModel;
        public bool TryGetAll<T>(out T[] result, Func<T, bool> cond = null) where T : ViewModel;

        public bool Dispose<T>(Func<T, bool> cond = null) where T : ViewModel
        {
            if (!TryGet<T>(out var model, cond))
                return false;
            model.Dispose();
            return true;
        }        
        public bool DisposeAll<T>(Func<T, bool> cond = null) where T : ViewModel
        {
            if (!TryGetAll<T>(out var models, cond))
                return false;
            for (int i = 0; i < models.Length; i++)
                models[i].Dispose();
            return models.Length > 0;
        }
    }
}