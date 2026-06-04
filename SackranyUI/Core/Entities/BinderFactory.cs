using System;
using System.Collections.Generic;
using System.Reflection;

using R3;

using SackranyUI.Core.Base;
using SackranyUI.Core.Components;
using SackranyUI.Core.Entities.Binders;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SackranyUI.Core.Entities
{
    internal static class BinderFactory
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void Init()
        {
            _outputMakeCache.Clear();
            _formattedMakeCache.Clear();
            _methodMakeCache.Clear();
            _reactivePropertyTypeCache.Clear();
            _actionTypeCache.Clear();
        }

        static readonly MethodInfo _outputMakeBase = typeof(BinderFactory)
            .GetMethod(nameof(MakeOutputBinder), BindingFlags.Static | BindingFlags.NonPublic);
        static readonly MethodInfo _formattedMakeBase = typeof(BinderFactory)
            .GetMethod(nameof(MakeFormattedTextBinder), BindingFlags.Static | BindingFlags.NonPublic);
        static readonly MethodInfo _methodMakeBase = typeof(BinderFactory)
            .GetMethod(nameof(MakeGenericBinder), BindingFlags.Static | BindingFlags.NonPublic);
        static readonly MethodInfo _collectionBinderCtorBase = typeof(BinderFactory)
            .GetMethod(nameof(CreateCollectionBinderGeneric), BindingFlags.Static | BindingFlags.NonPublic);

        static readonly Dictionary<Type, MethodInfo> _outputMakeCache = new();
        static readonly Dictionary<Type, MethodInfo> _formattedMakeCache = new();
        static readonly Dictionary<Type, MethodInfo> _methodMakeCache = new();
        static readonly Dictionary<Type, Type> _reactivePropertyTypeCache = new();
        static readonly Dictionary<Type, Type> _actionTypeCache = new();

        // ─── Generic builders (invoked via reflection) ───────────────────────────────

        static IBinder MakeGenericBinder<T>(ReadOnlyReactiveProperty<T> prop, Action<T> setter)
            => new GenericBinder<T>(prop, setter);

        static IBinder MakeOutputBinder<T>(ReadOnlyReactiveProperty<T> prop, object component, Action<object, object> setter)
            => new GenericBinder<T>(prop, v => setter(component, v));

        static IBinder MakeFormattedTextBinder<T>(ReadOnlyReactiveProperty<T> prop, TMP_Text text, Func<object, string> formatter)
            => new GenericBinder<T>(prop, v => text.text = formatter(v));

        static IBinder CreateCollectionBinderGeneric<TItemVM>(
            ReactiveList<TItemVM> list, Transform container, GameObject prefab, ViewModel owner)
            where TItemVM : ViewModel
            => new CollectionBinder<TItemVM>(list, container, prefab, owner);

        // ─── Helpers ─────────────────────────────────────────────────────────────────

        static TValue GetOrAdd<TKey, TValue>(Dictionary<TKey, TValue> cache, TKey key, Func<TKey, TValue> create)
        {
            if (!cache.TryGetValue(key, out var value))
                cache[key] = value = create(key);
            return value;
        }

        static bool TryGetReactiveValueType(Type type, out Type valueType)
        {
            var t = type;
            while (t != null && t != typeof(object))
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ReadOnlyReactiveProperty<>))
                {
                    valueType = t.GetGenericArguments()[0];
                    return true;
                }
                t = t.BaseType;
            }
            valueType = null;
            return false;
        }

        // ─── Public API ───────────────────────────────────────────────────────────────

        public static void ApplyInitialValue(object vmValue, object viewField)
        {
            if (vmValue == null || viewField == null) return;
            if (BinderRegistry.TryGetInit(vmValue.GetType(), viewField.GetType(), out var setter))
                setter(viewField, vmValue);
        }

        public static IBinder CreateFieldToField(object vmField, object viewField)
        {
            if (vmField == null || viewField == null) return null;

            if (vmField is ReactiveCommand command && viewField is Button button)
                return new ActionBinder(button, () => command.Execute(Unit.Default));

            if (!TryGetReactiveValueType(vmField.GetType(), out var valueType))
                return null;

            var componentType = viewField.GetType();

            if (BinderRegistry.TryGetOutput(valueType, componentType, out var setter))
            {
                var make = GetOrAdd(_outputMakeCache, valueType, t => _outputMakeBase.MakeGenericMethod(t));
                return (IBinder)make.Invoke(null, new[] { vmField, viewField, setter });
            }

            if (viewField is TMP_Text text &&
                BinderRegistry.TextFormatters.TryGetValue(valueType, out var formatter))
            {
                var make = GetOrAdd(_formattedMakeCache, valueType, t => _formattedMakeBase.MakeGenericMethod(t));
                return (IBinder)make.Invoke(null, new object[] { vmField, text, formatter });
            }

            return null;
        }

        public static IBinder CreateForInputMethod(object vmInstance, MethodInfo vmBindMethod, ParameterInfo parameterInfo, object viewInputField)
        {
            if (viewInputField is Button btn)
            {
                if (parameterInfo != null) return null;
                var action = (Action)Delegate.CreateDelegate(typeof(Action), vmInstance, vmBindMethod);
                return new ActionBinder(btn, action);
            }

            if (parameterInfo == null) return null;

            if (!BinderRegistry.TryGetInput(viewInputField.GetType(), out var registration)) return null;
            if (parameterInfo.ParameterType != registration.ValueType) return null;

            return registration.Factory(viewInputField, vmBindMethod, vmInstance);
        }

        public static IBinder CreateForOutputMethod(object vmField, object viewInstance, MethodInfo method, ParameterInfo parameter)
        {
            if (vmField == null) return null;

            var t = parameter.ParameterType;

            var expectedType = GetOrAdd(_reactivePropertyTypeCache, t, x => typeof(ReadOnlyReactiveProperty<>).MakeGenericType(x));
            if (!expectedType.IsInstanceOfType(vmField)) return null;

            var makeMethod = GetOrAdd(_methodMakeCache, t, x => _methodMakeBase.MakeGenericMethod(x));
            var actionType = GetOrAdd(_actionTypeCache, t, x => typeof(Action<>).MakeGenericType(x));

            var action = Delegate.CreateDelegate(actionType, viewInstance, method);
            return (IBinder)makeMethod.Invoke(null, new[] { vmField, action });
        }

        public static IBinder CreateCollectionBinder(ViewModel owner, object vmField, object viewField)
        {
            if (viewField is not CollectionAnchor anchor) return null;

            var vmType = vmField.GetType();
            if (!vmType.IsGenericType || vmType.GetGenericTypeDefinition() != typeof(ReactiveList<>))
                return null;

            var itemType = vmType.GetGenericArguments()[0];
            if (!typeof(ViewModel).IsAssignableFrom(itemType)) return null;

            var method = _collectionBinderCtorBase.MakeGenericMethod(itemType);
            return (IBinder)method.Invoke(null, new[] { vmField, anchor.Container, anchor.Prefab, owner });
        }
    }
}
