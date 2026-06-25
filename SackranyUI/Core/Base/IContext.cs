using System;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

namespace SackranyUI.Core.Base
{
    /// <summary>The query and creation surface a view model uses to reach its sibling view models.</summary>
    public interface IContextUser
    {
        /// <summary>Creates view models from templates under the given root.</summary>
        public ViewModel[] Add(IEnumerable<IViewModelTemplate> viewModels, Transform root);
        /// <summary>Creates a single view model from a template under the given root.</summary>
        public ViewModel Add(IViewModelTemplate viewModelTemplate, Transform root);

        /// <summary>Returns true if any view model of type <typeparamref name="T"/> matches the predicate.</summary>
        public bool Has<T>(Func<T, bool> cond = null) where T : ViewModel;
        /// <summary>Returns the first matching view model of type <typeparamref name="T"/>, or null.</summary>
        public T Get<T>(Func<T, bool> cond = null) where T : ViewModel;
        /// <summary>Returns all matching view models of type <typeparamref name="T"/>.</summary>
        public T[] GetAll<T>(Func<T, bool> cond = null) where T : ViewModel;
        /// <summary>Tries to get the first matching view model of type <typeparamref name="T"/>.</summary>
        public bool TryGet<T>(out T result, Func<T, bool> cond = null) where T : ViewModel;
        /// <summary>Tries to get all matching view models of type <typeparamref name="T"/>.</summary>
        public bool TryGetAll<T>(out T[] result, Func<T, bool> cond = null) where T : ViewModel;

        /// <summary>Disposes the first matching view model of type <typeparamref name="T"/>. Returns true if one was found.</summary>
        public bool Dispose<T>(Func<T, bool> cond = null) where T : ViewModel
        {
            if (!TryGet<T>(out var model, cond))
                return false;
            model.Dispose();
            return true;
        }
        /// <summary>Disposes every matching view model of type <typeparamref name="T"/>. Returns true if any were found.</summary>
        public bool DisposeAll<T>(Func<T, bool> cond = null) where T : ViewModel
        {
            if (!TryGetAll<T>(out var models, cond))
                return false;
            for (int i = 0; i < models.Length; i++)
                models[i].Dispose();
            return models.Length > 0;
        }
    }

    /// <summary>Owns the live view models, their prefabs and the shared event bus for a UI root.</summary>
    public interface IContext : IContextUser, IDisposable
    {
        /// <summary>Initializes the context with its content root and a cancellation token.</summary>
        public void Init(Transform root, CancellationToken ct);
    }
}