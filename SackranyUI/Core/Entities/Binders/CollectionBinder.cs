using System;
using System.Collections.Generic;
using System.Linq;

using R3;

using SackranyUI.Core.Base;
using SackranyUI.Core.Components;
using SackranyUI.Core.Static;

using UnityEngine;

using Object = UnityEngine.Object;

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

            for (int i = 0; i < _list.Items.Count; i++)
                Insert(i, _list.Items[i]);

            _subscriptions.Add(_list.OnAdd.Subscribe(e => Insert(e.index, e.item)));
            _subscriptions.Add(_list.OnRemove.Subscribe(e => RemoveAt(e.index)));
            _subscriptions.Add(_list.OnReplace.Subscribe(e => Replace(e.index, e.item)));
            _subscriptions.Add(_list.OnMove.Subscribe(e => Move(e.from, e.to)));
            _subscriptions.Add(_list.OnReset.Subscribe(_ => DespawnAll()));
        }

        public void Unbind()
        {
            _subscriptions.Dispose();
            _subscriptions.Clear();
            DespawnAll();
        }

        void Insert(int index, TItemVM vm)
        {
            if (index < 0 || index > _instances.Count) index = _instances.Count;

            GameObject go = null;
            try
            {
                go = Object.Instantiate(_prefab, _container);
                go.SetActive(true);
                go.transform.SetSiblingIndex(index);

                var views = go.GetComponentsInChildren<View>();
                var anchors = go.GetComponentsInChildren<Anchor>().ToDictionary(x => x.Key, x => x.transform);
                var transitions = CollectTransitions(go);
                var binders = UIBinder.Bind(vm, views);

                vm.Initialize(
                    _owner.Context,
                    _owner.EventListener,
                    _owner.EventPublisher,
                    go.transform,
                    anchors,
                    _owner.CancellationTokenSource.Token,
                    transitions);

                foreach (var view in views)
                    view.Initialize(vm.CancellationTokenSource.Token);

                UIBinder.BindInits(vm, views);
                foreach (var b in binders)
                    b.Bind();

                _instances.Insert(index, (vm, go, binders));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                if (go != null) Object.Destroy(go);
            }
        }

        void RemoveAt(int index)
        {
            if (index < 0 || index >= _instances.Count) return;

            var (vm, go, binders) = _instances[index];
            _instances.RemoveAt(index);

            foreach (var b in binders) b.Unbind();
            vm.Dispose();
            if (go != null) Object.Destroy(go);
        }

        void Replace(int index, TItemVM vm)
        {
            RemoveAt(index);
            Insert(index, vm);
        }

        void Move(int from, int to)
        {
            if (from < 0 || from >= _instances.Count) return;
            if (to < 0 || to >= _instances.Count) return;
            if (from == to) return;

            var instance = _instances[from];
            _instances.RemoveAt(from);
            _instances.Insert(to, instance);
            if (instance.go != null)
                instance.go.transform.SetSiblingIndex(to);
        }

        void DespawnAll()
        {
            for (int i = _instances.Count - 1; i >= 0; i--)
            {
                var (vm, go, binders) = _instances[i];
                foreach (var b in binders) b.Unbind();
                vm.Dispose();
                if (go != null) Object.Destroy(go);
            }
            _instances.Clear();
        }

        static IReadOnlyList<IUITransition> CollectTransitions(GameObject go)
        {
            var found = go.GetComponentsInChildren<MonoBehaviour>(true).OfType<IUITransition>().ToArray();
            return found.Length > 0 ? found : null;
        }
    }
}
