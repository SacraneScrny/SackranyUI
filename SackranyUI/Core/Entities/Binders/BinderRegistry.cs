using System;
using System.Collections.Generic;
using System.Reflection;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SackranyUI.Core.Entities.Binders
{
    /// <summary>
    /// Central registry of binders, text formatters and initializers. Register custom rules at
    /// startup to extend how values map onto components without touching the core.
    /// </summary>
    public static class BinderRegistry
    {
        internal static readonly Dictionary<(Type value, Type component), Action<object, object>> Output = new();
        internal static readonly Dictionary<Type, Func<object, string>> TextFormatters = new();
        internal static readonly Dictionary<Type, InputRegistration> Input = new();
        internal static readonly Dictionary<(Type value, Type component), Action<object, object>> Init = new();

        internal readonly struct InputRegistration
        {
            public readonly Type ValueType;
            public readonly Func<object, MethodInfo, object, IBinder> Factory;

            public InputRegistration(Type valueType, Func<object, MethodInfo, object, IBinder> factory)
            {
                ValueType = valueType;
                Factory = factory;
            }
        }

        static BinderRegistry()
        {
            RegisterDefaults();
        }

        /// <summary>Registers how a <typeparamref name="TValue"/> is written onto a <typeparamref name="TComponent"/> (view model → view).</summary>
        public static void RegisterOutput<TValue, TComponent>(Action<TComponent, TValue> setter)
            => Output[(typeof(TValue), typeof(TComponent))] = (c, v) => setter((TComponent)c, (TValue)v);

        /// <summary>Registers how a <typeparamref name="TValue"/> is formatted to a string for <c>TMP_Text</c> bindings.</summary>
        public static void RegisterTextFormatter<TValue>(Func<TValue, string> formatter)
            => TextFormatters[typeof(TValue)] = v => formatter((TValue)v);

        /// <summary>Registers how a one-shot <c>[InitBind]</c> <typeparamref name="TValue"/> seeds a <typeparamref name="TComponent"/>.</summary>
        public static void RegisterInit<TValue, TComponent>(Action<TComponent, TValue> setter)
            => Init[(typeof(TValue), typeof(TComponent))] = (c, v) => setter((TComponent)c, (TValue)v);

        /// <summary>Registers how a <typeparamref name="TComponent"/> reports <typeparamref name="TValue"/> changes back to the view model (view → view model).</summary>
        public static void RegisterInput<TValue, TComponent>(
            Action<TComponent, UnityAction<TValue>> addListener,
            Action<TComponent, UnityAction<TValue>> removeListener)
        {
            Input[typeof(TComponent)] = new InputRegistration(typeof(TValue), (component, method, vm) =>
            {
                var action = (Action<TValue>)Delegate.CreateDelegate(typeof(Action<TValue>), vm, method);
                var c = (TComponent)component;
                return new InputBinder<TValue>(action,
                    h => addListener(c, h),
                    h => removeListener(c, h));
            });
        }

        internal static bool TryGetOutput(Type valueType, Type componentType, out Action<object, object> setter)
        {
            var c = componentType;
            while (c != null && c != typeof(object))
            {
                if (Output.TryGetValue((valueType, c), out setter)) return true;
                c = c.BaseType;
            }
            setter = null;
            return false;
        }

        internal static bool TryGetInit(Type valueType, Type componentType, out Action<object, object> setter)
        {
            var c = componentType;
            while (c != null && c != typeof(object))
            {
                if (Init.TryGetValue((valueType, c), out setter)) return true;
                c = c.BaseType;
            }
            setter = null;
            return false;
        }

        internal static bool TryGetInput(Type componentType, out InputRegistration registration)
        {
            var c = componentType;
            while (c != null && c != typeof(object))
            {
                if (Input.TryGetValue(c, out registration)) return true;
                c = c.BaseType;
            }
            registration = default;
            return false;
        }

        static void RegisterDefaults()
        {
            RegisterOutput<string, TMP_Text>((c, v) => c.text = v);
            RegisterOutput<Color, TMP_Text>((c, v) => c.color = v);

            RegisterOutput<float, Slider>((c, v) => c.value = v);
            RegisterOutput<float, Image>((c, v) => c.fillAmount = v);
            RegisterOutput<Color, Image>((c, v) => c.color = v);
            RegisterOutput<Sprite, Image>((c, v) => c.sprite = v);

            RegisterOutput<bool, Toggle>((c, v) => c.isOn = v);
            RegisterOutput<string, TMP_InputField>((c, v) => c.text = v);
            RegisterOutput<int, TMP_Dropdown>((c, v) => c.value = v);

            RegisterOutput<Texture, RawImage>((c, v) => c.texture = v);
            RegisterOutput<Texture2D, RawImage>((c, v) => c.texture = v);
            RegisterOutput<Color, RawImage>((c, v) => c.color = v);

            RegisterOutput<float, CanvasGroup>((c, v) => c.alpha = v);

            RegisterOutput<bool, GameObject>((c, v) => c.SetActive(v));
            RegisterOutput<bool, Selectable>((c, v) => c.interactable = v);

            RegisterTextFormatter<string>(v => v);
            RegisterTextFormatter<int>(v => v.ToString());
            RegisterTextFormatter<long>(v => v.ToString());
            RegisterTextFormatter<float>(v => v.ToString());
            RegisterTextFormatter<double>(v => v.ToString());
            RegisterTextFormatter<bool>(v => v.ToString());
            RegisterTextFormatter<DateTime>(v => v.ToString("dd:MM:yyyy HH:mm:ss"));
            RegisterTextFormatter<TimeSpan>(v => v.ToString("g"));

            RegisterInput<float, Slider>(
                (c, h) => c.onValueChanged.AddListener(h),
                (c, h) => c.onValueChanged.RemoveListener(h));
            RegisterInput<bool, Toggle>(
                (c, h) => c.onValueChanged.AddListener(h),
                (c, h) => c.onValueChanged.RemoveListener(h));
            RegisterInput<string, TMP_InputField>(
                (c, h) => c.onValueChanged.AddListener(h),
                (c, h) => c.onValueChanged.RemoveListener(h));
            RegisterInput<int, TMP_Dropdown>(
                (c, h) => c.onValueChanged.AddListener(h),
                (c, h) => c.onValueChanged.RemoveListener(h));

            RegisterInit<float, Slider>((c, v) => c.value = v);
            RegisterInit<bool, Toggle>((c, v) => c.isOn = v);
            RegisterInit<string, TMP_InputField>((c, v) => c.text = v);
            RegisterInit<int, TMP_Dropdown>((c, v) => c.value = v);
        }
    }
}
