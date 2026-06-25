using SackranyUI.Core.Entities;

using UnityEngine;

namespace SackranyUI.Core.Base
{
    /// <summary>Serializable data plus prefab reference used to create and configure a view model.</summary>
    public interface IViewModelTemplate
    {
        /// <summary>The prefab to instantiate for this view model.</summary>
        GameObject Prefab();
        /// <summary>Creates a new view model instance for this template.</summary>
        ViewModel CreateViewModel();
    }

    /// <summary>Base template that produces a <typeparamref name="TViewModel"/> and carries its prefab and serialized data.</summary>
    public abstract class ViewModelTemplate<TViewModel> : IViewModelTemplate
        where TViewModel : ViewModel, new()
    {
        /// <summary>Prefab instantiated for this view model.</summary>
        public GameObject UIPrefab;
        /// <summary>The prefab to instantiate for this view model.</summary>
        public GameObject Prefab() => UIPrefab;
        ViewModel IViewModelTemplate.CreateViewModel() => ViewModelFactory.Create<TViewModel>();
    }
}