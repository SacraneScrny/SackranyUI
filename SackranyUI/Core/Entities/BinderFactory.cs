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
    public static class BinderFactory
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void Init()
        {
            _makeMethodCache.Clear();
            _reactivePropertyTypeCache.Clear();
            _actionTypeCache.Clear();
        }

        static readonly Dictionary<(Type, Type), Func<object, object, IBinder>> _factories = new()
        {
            { (typeof(ReactiveProperty<string>), typeof(TMP_Text)),
                (vm, v) => MakeGenericBinder((ReactiveProperty<string>)vm, x => ((TMP_Text)v).text = x) },

            { (typeof(ReactiveProperty<DateTime>), typeof(TMP_Text)),
                (vm, v) => MakeGenericBinder((ReactiveProperty<DateTime>)vm, x => ((TMP_Text)v).text = x.ToString("dd:MM:yyyy HH:mm:ss")) },

            { (typeof(ReactiveProperty<TimeSpan>), typeof(TMP_Text)),
                (vm, v) => MakeGenericBinder((ReactiveProperty<TimeSpan>)vm, x => ((TMP_Text)v).text = x.ToString("g")) },

            { (typeof(ReactiveProperty<float>), typeof(Slider)),
                (vm, v) => MakeGenericBinder((ReactiveProperty<float>)vm, x => ((Slider)v).value = x) },

            { (typeof(ReactiveProperty<float>), typeof(Image)),
                (vm, v) => MakeGenericBinder((ReactiveProperty<float>)vm, x => ((Image)v).fillAmount = x) },

            { (typeof(ReactiveProperty<Color>), typeof(Image)),
                (vm, v) => MakeGenericBinder((ReactiveProperty<Color>)vm, x => ((Image)v).color = x) },

            { (typeof(ReactiveProperty<Sprite>), typeof(Image)),
                (vm, v) => MakeGenericBinder((ReactiveProperty<Sprite>)vm, x => ((Image)v).sprite = x) },

            { (typeof(ReactiveProperty<bool>), typeof(GameObject)),
                (vm, v) => MakeGenericBinder((ReactiveProperty<bool>)vm, x => ((GameObject)v).SetActive(x)) },

            { (typeof(ReactiveProperty<bool>), typeof(Button)),
                (vm, v) => MakeGenericBinder((ReactiveProperty<bool>)vm, x => ((Button)v).interactable = x) },

            { (typeof(ReactiveCommand), typeof(Button)),
                (vm, v) => new ActionBinder((Button)v, () => ((ReactiveCommand)vm).Execute(Unit.Default)) },
        };

        static readonly Dictionary<Type, InputFactoryData> _inputFactories = new()
        {
            { typeof(Slider), new InputFactoryData(typeof(float), (vm, method, view) =>
            {
                var action = (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), vm, method);
                var component = (Slider)view;
                return new InputBinder<float>(action,
                    h => component.onValueChanged.AddListener(h),
                    h => component.onValueChanged.RemoveListener(h));
            })},

            { typeof(Toggle), new InputFactoryData(typeof(bool), (vm, method, view) =>
            {
                var action = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>), vm, method);
                var component = (Toggle)view;
                return new InputBinder<bool>(action,
                    h => component.onValueChanged.AddListener(h),
                    h => component.onValueChanged.RemoveListener(h));
            })},

            { typeof(TMP_InputField), new InputFactoryData(typeof(string), (vm, method, view) =>
            {
                var action = (Action<string>)Delegate.CreateDelegate(typeof(Action<string>), vm, method);
                var component = (TMP_InputField)view;
                return new InputBinder<string>(action,
                    h => component.onValueChanged.AddListener(h),
                    h => component.onValueChanged.RemoveListener(h));
            })},
        };

        static readonly Dictionary<(Type, Type), Action<object, object>> _initializers = new()
        {
            { (typeof(float), typeof(Slider)), (v, c) => ((Slider)c).value = (float)v },
            { (typeof(bool), typeof(Toggle)), (v, c) => ((Toggle)c).isOn = (bool)v },
            { (typeof(string), typeof(TMP_InputField)), (v, c) => ((TMP_InputField)c).text = (string)v },
        };

        static readonly MethodInfo _makeMethodBase = typeof(BinderFactory)
            .GetMethod(nameof(MakeGenericBinder), BindingFlags.Static | BindingFlags.NonPublic);

        static readonly MethodInfo _collectionBinderCtorBase = typeof(BinderFactory)
            .GetMethod(nameof(CreateCollectionBinderGeneric), BindingFlags.Static | BindingFlags.NonPublic);

        static readonly Dictionary<Type, MethodInfo> _makeMethodCache = new();
        static readonly Dictionary<Type, Type> _reactivePropertyTypeCache = new();
        static readonly Dictionary<Type, Type> _actionTypeCache = new();

        // ─── Private ─────────────────────────────────────────────────────────────────

        static IBinder MakeGenericBinder<T>(ReadOnlyReactiveProperty<T> prop, Action<T> setter)
            => new GenericBinder<T>(prop, setter);

        static IBinder CreateCollectionBinderGeneric<TItemVM>(
            ReactiveList<TItemVM> list, Transform container, GameObject prefab, ViewModel owner)
            where TItemVM : ViewModel
            => new CollectionBinder<TItemVM>(list, container, prefab, owner);

        static TValue GetOrAdd<TKey, TValue>(Dictionary<TKey, TValue> cache, TKey key, Func<TKey, TValue> create)
        {
            if (!cache.TryGetValue(key, out var value))
                cache[key] = value = create(key);
            return value;
        }

        readonly struct InputFactoryData
        {
            public readonly Type ExpectedType;
            public readonly Func<object, MethodInfo, object, IBinder> Factory;

            public InputFactoryData(Type expectedType, Func<object, MethodInfo, object, IBinder> factory)
            {
                ExpectedType = expectedType;
                Factory = factory;
            }
        }

        // ─── Public API ───────────────────────────────────────────────────────────────

        public static void ApplyInitialValue(object vmValue, object viewField)
        {
            if (_initializers.TryGetValue((vmValue.GetType(), viewField.GetType()), out var setter))
                setter(vmValue, viewField);
        }

        public static IBinder CreateFieldToField(object vmField, object viewField)
        {
            var vmType = vmField.GetType();
            var viewType = viewField.GetType();

            if (_factories.TryGetValue((vmType, viewType), out var factory))
                return factory(vmField, viewField);

            var current = viewType.BaseType;
            while (current != null && current != typeof(object))
            {
                if (_factories.TryGetValue((vmType, current), out factory))
                    return factory(vmField, viewField);
                current = current.BaseType;
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

            var viewType = viewInputField.GetType();
            if (!_inputFactories.TryGetValue(viewType, out var factoryData)) return null;
            if (parameterInfo.ParameterType != factoryData.ExpectedType) return null;

            return factoryData.Factory(vmInstance, vmBindMethod, viewInputField);
        }

        public static IBinder CreateForOutputMethod(object vmField, object viewInstance, MethodInfo method, ParameterInfo parameter)
        {
            var t = parameter.ParameterType;

            var expectedType = GetOrAdd(_reactivePropertyTypeCache, t, x => typeof(ReadOnlyReactiveProperty<>).MakeGenericType(x));
            if (!expectedType.IsInstanceOfType(vmField)) return null;

            var makeMethod = GetOrAdd(_makeMethodCache, t, x => _makeMethodBase.MakeGenericMethod(x));
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