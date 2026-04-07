using System.Linq;
using System.Threading;

using SackranyUI.Core.Base;
using SackranyUI.Core.Components;
using SackranyUI.Core.Events;
using SackranyUI.Core.Static;

using UnityEngine;

namespace SackranyUI.Core.Entities
{
    public static class ViewModelFactory
    {
        public static TViewModel Create<TViewModel>()
            where TViewModel : ViewModel, new()
        {
            var viewModel = new TViewModel();
            return viewModel;
        }
        
        public static ViewModelData Instantiate(
            ViewModel viewModel,
            IViewModelTemplate template, 
            Transform contentRoot = null)
        {
            UIDependencyInjector.Inject(viewModel, template);
            
            var prefab = Object.Instantiate(template.Prefab(), contentRoot);
            var views = prefab.GetComponentsInChildren<View>();
            var anchors = prefab.GetComponentsInChildren<Anchor>().ToDictionary(x => x.Key, x => x.transform);
            foreach (var view in views)
                view.PreInitialize();
            var binders = UIBinder.Bind(viewModel, views);
            prefab.SetActive(false);
            
            return new ViewModelData()
            {
                Prefab = prefab,
                ViewModel = viewModel,
                Views = views,
                Binders = binders,
                Anchors = anchors
            };
        }

        public static void Initialize(
            ViewModelData data,
            IContext context,
            IUIBusListener listener = null,
            IUIBusPublisher publisher = null,
            CancellationToken token = default
            )
        {
            data.ViewModel.Initialize(context, listener, publisher, data.Prefab.transform, data.Anchors, token);
            foreach (var view in data.Views)
                view.Initialize(data.ViewModel.CancellationTokenSource.Token);
            UIBinder.BindInits(data.ViewModel, data.Views);
            foreach (var binder in data.Binders)
                binder.Bind();
        }
    }
}