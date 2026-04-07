using System;
using System.Collections.Generic;

using R3;

namespace SackranyUI.Core.Entities
{
    public sealed class ReactiveList<T> : IDisposable
    {
        readonly List<T> _items = new();
        readonly Subject<(int index, T item)> _onAdd = new();
        readonly Subject<(int index, T item)> _onRemove = new();
        readonly Subject<Unit> _onReset = new();

        public IReadOnlyList<T> Items => _items;
        public Observable<(int index, T item)> OnAdd => _onAdd;
        public Observable<(int index, T item)> OnRemove => _onRemove;
        public Observable<Unit> OnReset => _onReset;

        public void Add(T item)
        {
            if (_disposed) return;
            _items.Add(item);
            _onAdd.OnNext((_items.Count - 1, item));
        }

        public void RemoveAt(int index)
        {
            if (_disposed) return;
            var item = _items[index];
            _items.RemoveAt(index);
            _onRemove.OnNext((index, item));
        }

        public void Remove(T item)
        {
            if (_disposed) return;
            var index = _items.IndexOf(item);
            if (index >= 0) RemoveAt(index);
        }

        public void Clear()
        {
            if (_disposed) return;
            _items.Clear();
            _onReset.OnNext(Unit.Default);
        }

        bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _onAdd.Dispose();
            _onRemove.Dispose();
            _onReset.Dispose();
            _items.Clear();
            _disposed = true;
        }
    }
}