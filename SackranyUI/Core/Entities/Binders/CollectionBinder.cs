using System.Collections.Generic;
using System.Linq;

using R3;

using SackranyUI.Core.Base;
using SackranyUI.Core.Components;
using SackranyUI.Core.Static;

using UnityEngine;

namespace SackranyUI.Core.Entities.Binders
{
    internal sealed class CollectionBinder<TItemVM> : IBinder
        where TItemVM : ViewModel
    {
        readonly ReactiveList<TItemVM> _list;
        readonly Transform _container;
        readonly GameObject _prefab;
        readonly ViewModel _owner;
        readonly List<(TItemVM vm, GameObject go, IBinder[] binders)> _instances = new();
        readonly CompositeDisposable _subscriptions = new();

        public CollectionBinder(ReactiveList<TItemVM> list, Transform container, GameObject prefab, ViewModel owner)
        {
            prefab.SetActive(false);
            _list = list;
            _container = container;
            _prefab = prefab;
            _owner = owner;
        }

        public void Bind()
        {
            Unbind();

            foreach (var item in _list.Items)
                Spawn(item);

            _subscriptions.Add(_list.OnAdd.Subscribe(e => Spawn(e.item)));
            _subscriptions.Add(_list.OnRemove.Subscribe(e => Despawn(e.item)));
            _subscriptions.Add(_list.OnReset.Subscribe(_ => DespawnAll()));
        }

        public void Unbind()
        {
            _subscriptions.Dispose();
            _subscriptions.Clear();
            DespawnAll();
        }

        void Spawn(TItemVM vm)
        {
            var go = Object.Instantiate(_prefab, _container);
            go.SetActive(true);
            var views = go.GetComponentsInChildren<View>();
            var anchors = go.GetComponentsInChildren<Anchor>().ToDictionary(x => x.Key, x => x.transform);
            var binders = UIBinder.Bind(vm, views);

            vm.Initialize(vm, vm, vm, go.transform, anchors, _owner.CancellationTokenSource.Token);
            foreach (var view in views)
                view.Initialize(vm.CancellationTokenSource.Token);
            
            UIBinder.BindInits(vm, views);
            foreach (var b in binders)
                b.Bind();

            _instances.Add((vm, go, binders));
        }

        void Despawn(TItemVM vm)
        {
            var idx = _instances.FindIndex(x => x.vm.Equals(vm));
            if (idx < 0) return;

            var (_, go, binders) = _instances[idx];
            foreach (var b in binders) b.Unbind();
            vm.Dispose();
            Object.Destroy(go);
            _instances.RemoveAt(idx);
        }

        void DespawnAll()
        {
            foreach (var (_, go, binders) in _instances)
            {
                foreach (var b in binders) b.Unbind();
                if (go != null) Object.Destroy(go);
            }
            _instances.Clear();
        }
    }
}