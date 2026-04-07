using SackranyUI.Core.Entities;

using UnityEngine;

namespace SackranyUI.Core.Base
{
    public interface IViewModelTemplate
    {
        GameObject Prefab();
        ViewModel CreateViewModel();
    }

    public abstract class ViewModelTemplate<TViewModel> : IViewModelTemplate
        where TViewModel : ViewModel, new()
    {
        public GameObject UIPrefab;
        public GameObject Prefab() => UIPrefab;
        ViewModel IViewModelTemplate.CreateViewModel() => ViewModelFactory.Create<TViewModel>();
    }
}