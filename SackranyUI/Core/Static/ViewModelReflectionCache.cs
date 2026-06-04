using System;
using System.Collections.Generic;
using System.Reflection;

using SackranyUI.Core.Entities;

using UnityEngine;

namespace SackranyUI.Core.Static
{
    internal static class ViewModelReflectionCache
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
            public readonly MemberRef Member;
            public readonly object Id;

            public BindField(MemberRef member, object id)
            {
                this.Member = member;
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

            const BindingFlags memberFlags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly;

            var current = type;
            while (current != null && current != typeof(object))
            {
                foreach (var field in current.GetFields(memberFlags))
                {
                    if (field.GetCustomAttribute<UITemplateAttribute>() is { })
                    {
                        templateField = new TemplateField(field, field.FieldType);
                        continue;
                    }
                    CollectBindMember(new MemberRef(field), field, bindings, initBinds);
                }

                foreach (var property in current.GetProperties(memberFlags))
                {
                    if (!property.CanRead || property.GetIndexParameters().Length > 0)
                        continue;
                    CollectBindMember(new MemberRef(property), property, bindings, initBinds);
                }

                foreach (var method in current.GetMethods(memberFlags))
                {
                    var allParams = method.GetParameters();
                    var param = allParams.Length == 1 ? allParams[0] : null;
                    foreach (var m in method.GetCustomAttributes<BindAttribute>())
                        methodBinds.Add(new BindMethod(param, method, m.id));
                }

                current = current.BaseType;
            }

            return new ViewModelMetadata(templateField, bindings.ToArray(), initBinds.ToArray(), methodBinds.ToArray());
        }

        static void CollectBindMember(MemberRef member, MemberInfo source, List<BindField> bindings, List<BindField> initBinds)
        {
            foreach (var d in source.GetCustomAttributes<BindAttribute>())
                bindings.Add(new BindField(member, d.id));
            foreach (var i in source.GetCustomAttributes<InitBindAttribute>())
                initBinds.Add(new BindField(member, i.id));
        }
    }
}
