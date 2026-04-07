using System;
using System.Collections.Generic;

using UnityEngine;

namespace SackranyUI.Core.Events
{
    public static class UIEventRegistry
    {
        static readonly Dictionary<Type, int> _typeToId = new();
        static readonly List<Type> _idToType = new();
        static int _nextId;
        static readonly object _registryLock = new();

        static readonly Dictionary<Type, IUIEvent> _instances = new();
        static readonly object _instanceLock = new();

        public static int Count { get { lock (_registryLock) return _nextId; } }

        public static int GetId<T>() where T : IUIEvent => Id<T>.Value;
        public static int GetId(Type type)
        {
            lock (_registryLock)
                return _typeToId.GetValueOrDefault(type, -1);
        }
        public static int GetOrRegister(Type type)
        {
            lock (_registryLock)
            {
                if (_typeToId.TryGetValue(type, out var id))
                    return id;

                var newId = _nextId++;
                _typeToId[type] = newId;
                _idToType.Add(type);
                return newId;
            }
        }
        public static Type GetTypeById(int id)
        {
            lock (_registryLock)
                return id >= 0 && id < _idToType.Count ? _idToType[id] : null;
        }

        public static IUIEvent GetInstance<T>() where T : IUIEvent => Id<T>.Instance;

        static class Id<T> where T : IUIEvent
        {
            public static readonly int Value = GetOrRegister(typeof(T));
            public static readonly IUIEvent Instance = CreateInstance(typeof(T));
        }

        static IUIEvent CreateInstance(Type type)
        {
            lock (_instanceLock)
            {
                if (_instances.TryGetValue(type, out var inst))
                    return inst;

                if (type.IsAbstract)
                    return null;

                inst = Activator.CreateInstance(type) as IUIEvent
                       ?? throw new ArgumentException($"No public parameterless ctor for {type.FullName}");

                _instances[type] = inst;
                return inst;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init() => _nextId = int.MinValue;
    }
}