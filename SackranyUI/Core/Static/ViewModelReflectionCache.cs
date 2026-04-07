using System;
using System.Collections.Generic;
using System.Reflection;

using SackranyUI.Core.Entities;

using UnityEngine;

namespace SackranyUI.Core.Static
{
    public static class ViewModelReflectionCache
    {
        public readonly struct ViewModelMetadata
        {
            public readonly TemplateField Template;
            public readonly BindField[] FieldBindings;
            public readonly BindField[] InitFieldBindings;
            public readonly BindMethod[] MethodBindings;

            public ViewModelMetadata(TemplateField template, BindField[] fieldBindings, BindField[] initFieldBindings, BindMethod[] methodBindings)
            {
                this.Template = template;
                this.FieldBindings = fieldBindings;
                this.InitFieldBindings = initFieldBindings;
                this.MethodBindings = methodBindings;
            }
        }
        public readonly struct TemplateField
        {
            public readonly FieldInfo Field;
            public readonly Type Type;

            public TemplateField(FieldInfo field, Type type)
            {
                this.Field = field;
                this.Type = type;
            }
        }
        public readonly struct BindField
        {
            public readonly FieldInfo Field;
            public readonly Type Type;
            public readonly object Id;

            public BindField(FieldInfo field, Type type, object id)
            {
                this.Field = field;
                this.Type = type;
                this.Id = id;
            }
        }        
        public readonly struct BindMethod
        {
            public readonly ParameterInfo Parameter;
            public readonly MethodInfo Method;
            public readonly object Id;

            public BindMethod(ParameterInfo parameter, MethodInfo method, object id)
            {
                this.Parameter = parameter;
                this.Method = method;
                this.Id = id;
            }
        }

        static readonly Dictionary<Type, ViewModelMetadata> _vmCache = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void Init() => _vmCache.Clear();
        
        public static ViewModelMetadata GetViewModelMetadata(Type type)
        {
            if (_vmCache.TryGetValue(type, out ViewModelMetadata metadata))
                return metadata;
            metadata = BuildViewModelMetadata(type);
            _vmCache[type] = metadata;
            return metadata;
        }
        static ViewModelMetadata BuildViewModelMetadata(Type type)
        {
            var templateField = new TemplateField();
            var bindings = new List<BindField>();
            var initBinds = new List<BindField>();
            var methodBinds = new List<BindMethod>();

            var current = type;
            while (current != null && current != typeof(object))
            {
                var fields = current.GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly
                );
                var methods = current.GetMethods(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly
                );

                foreach (var field in fields)
                {
                    if (field.GetCustomAttribute<UITemplateAttribute>() is { } t)
                        templateField = new TemplateField(field, field.FieldType);
                    else if (field.GetCustomAttribute<BindAttribute>() is { } d)
                        bindings.Add(new BindField(field, field.FieldType, d.id));
                    else if (field.GetCustomAttribute<InitBindAttribute>() is { } i)
                        initBinds.Add(new BindField(field, field.FieldType, i.id));
                }

                foreach (var method in methods)
                {
                    if (method.GetCustomAttribute<BindAttribute>() is { } m)
                    {
                        var all_params = method.GetParameters();
                        var param = all_params.Length == 1 ? all_params[0] : null;
                        methodBinds.Add(new BindMethod(param, method, m.id));
                    }
                }
                
                current = current.BaseType;
            }
            
            return new ViewModelMetadata(templateField, bindings.ToArray(), initBinds.ToArray(), methodBinds.ToArray());
        }
    }
}