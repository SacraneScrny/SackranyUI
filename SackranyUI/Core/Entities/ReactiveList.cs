using System;
using System.Collections;
using System.Collections.Generic;

using R3;

namespace SackranyUI.Core.Entities
{
    public sealed class ReactiveList<T> : IReadOnlyList<T>, IDisposable
    {
        readonly List<T> _items = new();
        readonly Subject<(int index, T item)> _onAdd = new();
        readonly Subject<(int index, T item)> _onRemove = new();
        readonly Subject<(int index, T item)> _onReplace = new();
        readonly Subject<(int from, int to)> _onMove = new();
        readonly Subject<Unit> _onReset = new();

        public IReadOnlyList<T> Items => _items;
        public Observable<(int index, T item)> OnAdd => _onAdd;
        public Observable<(int index, T item)> OnRemove => _onRemove;
        public Observable<(int index, T item)> OnReplace => _onReplace;
        public Observable<(int from, int to)> OnMove => _onMove;
        public Observable<Unit> OnReset => _onReset;

        public int Count => _items.Count;

        public T this[int index]
        {
            get => _items[index];
            set
            {
                if (_disposed) return;
                _items[index] = value;
                _onReplace.OnNext((index, value));
            }
        }

        public void Add(T item)
        {
            if (_disposed) return;
            _items.Add(item);
            _onAdd.OnNext((_items.Count - 1, item));
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (_disposed || items == null) return;
            foreach (var item in items)
                Add(item);
        }

        public void Insert(int index, T item)
        {
            if (_disposed) return;
            if (index < 0 || index > _items.Count) index = _items.Count;
            _items.Insert(index, item);
            _onAdd.OnNext((index, item));
        }

        public void RemoveAt(int index)
        {
            if (_disposed) return;
            if (index < 0 || index >= _items.Count) return;
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

        public void Move(int from, int to)
        {
            if (_disposed) return;
            if (from < 0 || from >= _items.Count) return;
            if (to < 0 || to >= _items.Count) return;
            if (from == to) return;
            var item = _items[from];
            _items.RemoveAt(from);
            _items.Insert(to, item);
            _onMove.OnNext((from, to));
        }

        public bool Contains(T item) => _items.Contains(item);
        public int IndexOf(T item) => _items.IndexOf(item);

        public void Clear()
        {
            if (_disposed) return;
            _items.Clear();
            _onReset.OnNext(Unit.Default);
        }

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _onAdd.Dispose();
            _onRemove.Dispose();
            _onReplace.Dispose();
            _onMove.Dispose();
            _onReset.Dispose();
            _items.Clear();
            _disposed = true;
        }
    }
}
