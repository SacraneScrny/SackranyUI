using System;
using System.Collections.Generic;
using System.Reflection;

using SackranyUI.Core.Entities;

using UnityEngine;

namespace SackranyUI.Core.Static
{
    internal static class ViewReflectionCache
    {
        public readonly struct ViewMetadata
        {
            public readonly OutputField[] OutputFieldBindings;
            public readonly OutputMethod[] OutputMethodBindings;
            public readonly InputField[] InputFieldBindings;
            public readonly CollectionField[] CollectionFieldBindings;

            public ViewMetadata(OutputField[] outputFieldBindings, OutputMethod[] outputMethodBindings,
                InputField[] inputFieldBindings, CollectionField[] collectionFieldBindings)
            {
                OutputFieldBindings = outputFieldBindings;
                OutputMethodBindings = outputMethodBindings;
                InputFieldBindings = inputFieldBindings;
                CollectionFieldBindings = collectionFieldBindings;
            }
        }
        public readonly struct InputField
        {
            public readonly MemberRef Member;
            public readonly object Id;

            public InputField(MemberRef member, object id)
            {
                this.Member = member;
                this.Id = id;
            }
        }
        public readonly struct OutputField
        {
            public readonly MemberRef Member;
            public readonly object Id;

            public OutputField(MemberRef member, object id)
            {
                this.Member = member;
                this.Id = id;
            }
        }
        public readonly struct OutputMethod
        {
            public readonly ParameterInfo Parameter;
            public readonly MethodInfo Method;
            public readonly object Id;

            public OutputMethod(ParameterInfo parameter, MethodInfo method, object id)
            {
                this.Parameter = parameter;
                this.Method = method;
                this.Id = id;
            }
        }
        public readonly struct CollectionField
        {
            public readonly MemberRef Member;
            public readonly object Id;

            public CollectionField(MemberRef member, object id)
            {
                Member = member;
                Id = id;
            }
        }

        static readonly Dictionary<Type, ViewMetadata> _vCache = new();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void Init() => _vCache.Clear();

        public static ViewMetadata GetViewMetadata(Type type)
        {
            if (_vCache.TryGetValue(type, out ViewMetadata metadata))
                return metadata;
            metadata = BuildViewMetadata(type);
            _vCache[type] = metadata;
            return metadata;
        }
        static ViewMetadata BuildViewMetadata(Type type)
        {
            var fieldBinds = new List<OutputField>();
            var methodBinds = new List<OutputMethod>();
            var inputFieldBinds = new List<InputField>();
            var collectionBinds = new List<CollectionField>();

            const BindingFlags memberFlags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly;

            var current = type;
            while (current != null && current != typeof(object))
            {
                foreach (var field in current.GetFields(memberFlags))
                    CollectMember(new MemberRef(field), field, fieldBinds, inputFieldBinds, collectionBinds);

                foreach (var property in current.GetProperties(memberFlags))
                {
                    if (!property.CanRead || property.GetIndexParameters().Length > 0)
                        continue;
                    CollectMember(new MemberRef(property), property, fieldBinds, inputFieldBinds, collectionBinds);
                }

                foreach (var method in current.GetMethods(memberFlags))
                {
                    var param = method.GetParameters();
                    if (param.Length != 1) continue;
                    foreach (var m in method.GetCustomAttributes<OutputBindAttribute>())
                        methodBinds.Add(new OutputMethod(param[0], method, m.id));
                }

                current = current.BaseType;
            }

            return new ViewMetadata(fieldBinds.ToArray(), methodBinds.ToArray(),
                inputFieldBinds.ToArray(), collectionBinds.ToArray());
        }

        static void CollectMember(
            MemberRef member, MemberInfo source,
            List<OutputField> outputs, List<InputField> inputs, List<CollectionField> collections)
        {
            foreach (var d in source.GetCustomAttributes<OutputBindAttribute>())
                outputs.Add(new OutputField(member, d.id));
            foreach (var c in source.GetCustomAttributes<InputBindAttribute>())
                inputs.Add(new InputField(member, c.id));
            foreach (var col in source.GetCustomAttributes<CollectionBindAttribute>())
                collections.Add(new CollectionField(member, col.id));
        }
    }
}
