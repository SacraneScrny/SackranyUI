using System;
using System.Collections;
using System.Collections.Generic;

using R3;

namespace SackranyUI.Core.Entities
{
    /// <summary>
    /// An observable list that drives collection bindings: mutations emit change streams so
    /// the view's spawned child hierarchy stays in sync.
    /// </summary>
    public sealed class ReactiveList<T> : IReadOnlyList<T>, IDisposable
    {
        readonly List<T> _items = new();
        readonly Subject<(int index, T item)> _onAdd = new();
        readonly Subject<(int index, T item)> _onRemove = new();
        readonly Subject<(int index, T item)> _onReplace = new();
        readonly Subject<(int from, int to)> _onMove = new();
        readonly Subject<Unit> _onReset = new();

        /// <summary>The current items, read-only.</summary>
        public IReadOnlyList<T> Items => _items;
        /// <summary>Emits the index and item whenever an element is added or inserted.</summary>
        public Observable<(int index, T item)> OnAdd => _onAdd;
        /// <summary>Emits the index and item whenever an element is removed.</summary>
        public Observable<(int index, T item)> OnRemove => _onRemove;
        /// <summary>Emits the index and new item whenever an element is replaced via the indexer.</summary>
        public Observable<(int index, T item)> OnReplace => _onReplace;
        /// <summary>Emits the source and target indices whenever an element is moved.</summary>
        public Observable<(int from, int to)> OnMove => _onMove;
        /// <summary>Emits whenever the list is cleared.</summary>
        public Observable<Unit> OnReset => _onReset;

        /// <summary>Number of items in the list.</summary>
        public int Count => _items.Count;

        /// <summary>Gets the item at <paramref name="index"/>, or replaces it (raising <see cref="OnReplace"/>).</summary>
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

        /// <summary>Appends an item and raises <see cref="OnAdd"/>.</summary>
        public void Add(T item)
        {
            if (_disposed) return;
            _items.Add(item);
            _onAdd.OnNext((_items.Count - 1, item));
        }

        /// <summary>Appends several items, raising <see cref="OnAdd"/> for each.</summary>
        public void AddRange(IEnumerable<T> items)
        {
            if (_disposed || items == null) return;
            foreach (var item in items)
                Add(item);
        }

        /// <summary>Inserts an item at <paramref name="index"/> (clamped) and raises <see cref="OnAdd"/>.</summary>
        public void Insert(int index, T item)
        {
            if (_disposed) return;
            if (index < 0 || index > _items.Count) index = _items.Count;
            _items.Insert(index, item);
            _onAdd.OnNext((index, item));
        }

        /// <summary>Removes the item at <paramref name="index"/> and raises <see cref="OnRemove"/>.</summary>
        public void RemoveAt(int index)
        {
            if (_disposed) return;
            if (index < 0 || index >= _items.Count) return;
            var item = _items[index];
            _items.RemoveAt(index);
            _onRemove.OnNext((index, item));
        }

        /// <summary>Removes the first occurrence of <paramref name="item"/> and raises <see cref="OnRemove"/>.</summary>
        public void Remove(T item)
        {
            if (_disposed) return;
            var index = _items.IndexOf(item);
            if (index >= 0) RemoveAt(index);
        }

        /// <summary>Moves an item between indices and raises <see cref="OnMove"/>.</summary>
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

        /// <summary>Returns true if the list contains <paramref name="item"/>.</summary>
        public bool Contains(T item) => _items.Contains(item);
        /// <summary>Returns the index of <paramref name="item"/>, or -1.</summary>
        public int IndexOf(T item) => _items.IndexOf(item);

        /// <summary>Removes all items and raises <see cref="OnReset"/>.</summary>
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
