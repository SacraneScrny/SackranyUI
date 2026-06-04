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
    /// Публичный реестр привязок. Позволяет добавлять собственные биндеры,
    /// форматтеры текста и инициализаторы без правки ядра (#6, #7).
    /// </summary>
    public static class BinderRegistry
    {
        // (тип значения, тип компонента) -> setter(component, value)
        internal static readonly Dictionary<(Type value, Type component), Action<object, object>> Output = new();
        // тип значения -> форматтер в строку
        internal static readonly Dictionary<Type, Func<object, string>> TextFormatters = new();
        // тип компонента -> регистрация входной привязки
        internal static readonly Dictionary<Type, InputRegistration> Input = new();
        // (тип значения, тип компонента) -> применение начального значения
        internal static readonly Dictionary<(Type value, Type component), Action<object, object>> Init = new();

        internal readonly struct InputRegistration
        {
            public readonly Type ValueType;
            public readonly Func<object /*component*/, MethodInfo /*vm method*/, object /*vm instance*/, IBinder> Factory;

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

        // ─── Public API ──────────────────────────────────────────────────────────────

        /// <summary>Привязка реактивного свойства <typeparamref name="TValue"/> к компоненту <typeparamref name="TComponent"/> (VM → View).</summary>
        public static void RegisterOutput<TValue, TComponent>(Action<TComponent, TValue> setter)
            => Output[(typeof(TValue), typeof(TComponent))] = (c, v) => setter((TComponent)c, (TValue)v);

        /// <summary>Форматтер значения в строку для привязок к <see cref="TMP_Text"/>.</summary>
        public static void RegisterTextFormatter<TValue>(Func<TValue, string> formatter)
            => TextFormatters[typeof(TValue)] = v => formatter((TValue)v);

        /// <summary>Применение начального значения из VM в компонент при инициализации.</summary>
        public static void RegisterInit<TValue, TComponent>(Action<TComponent, TValue> setter)
            => Init[(typeof(TValue), typeof(TComponent))] = (c, v) => setter((TComponent)c, (TValue)v);

        /// <summary>Входная привязка: события компонента <typeparamref name="TComponent"/> → метод VM (View → VM).</summary>
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

        // ─── Lookups (component base-type aware) ─────────────────────────────────────

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

        // ─── Defaults ────────────────────────────────────────────────────────────────

        static void RegisterDefaults()
        {
            // Output: значение -> компонент
            RegisterOutput<string, TMP_Text>((c, v) => c.text = v);
            RegisterOutput<Color, TMP_Text>((c, v) => c.color = v);

            RegisterOutput<float, Slider>((c, v) => c.value = v);
            RegisterOutput<float, Image>((c, v) => c.fillAmount = v);
            RegisterOutput<Color, Image>((c, v) => c.color = v);
            RegisterOutput<Sprite, Image>((c, v) => c.sprite = v);

            RegisterOutput<int, TMP_Dropdown>((c, v) => c.value = v);

            RegisterOutput<Texture, RawImage>((c, v) => c.texture = v);
            RegisterOutput<Texture2D, RawImage>((c, v) => c.texture = v);
            RegisterOutput<Color, RawImage>((c, v) => c.color = v);

            RegisterOutput<float, CanvasGroup>((c, v) => c.alpha = v);

            RegisterOutput<bool, GameObject>((c, v) => c.SetActive(v));
            // Selectable покрывает Button / Slider / Toggle / TMP_InputField / TMP_Dropdown
            RegisterOutput<bool, Selectable>((c, v) => c.interactable = v);

            // Форматтеры текста (#7)
            RegisterTextFormatter<string>(v => v);
            RegisterTextFormatter<int>(v => v.ToString());
            RegisterTextFormatter<long>(v => v.ToString());
            RegisterTextFormatter<float>(v => v.ToString());
            RegisterTextFormatter<double>(v => v.ToString());
            RegisterTextFormatter<bool>(v => v.ToString());
            RegisterTextFormatter<DateTime>(v => v.ToString("dd:MM:yyyy HH:mm:ss"));
            RegisterTextFormatter<TimeSpan>(v => v.ToString("g"));

            // Input: события компонента -> метод VM
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

            // Init: начальные значения
            RegisterInit<float, Slider>((c, v) => c.value = v);
            RegisterInit<bool, Toggle>((c, v) => c.isOn = v);
            RegisterInit<string, TMP_InputField>((c, v) => c.text = v);
            RegisterInit<int, TMP_Dropdown>((c, v) => c.value = v);
        }
    }
}
