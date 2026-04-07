using System;
using System.Collections.Generic;
using System.Reflection;

using SackranyUI.Core.Entities;

using UnityEngine;

namespace SackranyUI.Core.Static
{
    public static class ViewReflectionCache
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
            public readonly FieldInfo Field;
            public readonly Type Type;
            public readonly object Id;

            public InputField(FieldInfo field, Type type, object id)
            {
                this.Field = field;
                this.Type = type;
                this.Id = id;
            }
        }       
        public readonly struct OutputField
        {
            public readonly FieldInfo Field;
            public readonly Type Type;
            public readonly object Id;

            public OutputField(FieldInfo field, Type type, object id)
            {
                this.Field = field;
                this.Type = type;
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
            public readonly FieldInfo Field;
            public readonly Type Type;
            public readonly object Id;

            public CollectionField(FieldInfo field, Type type, object id)
            {
                Field = field;
                Type = type;
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
                    if (field.GetCustomAttribute<OutputBindAttribute>() is { } d)
                        fieldBinds.Add(new OutputField(field, field.FieldType, d.id));
                    if (field.GetCustomAttribute<InputBindAttribute>() is { } c)
                        inputFieldBinds.Add(new InputField(field, field.FieldType, c.id));
                    if (field.GetCustomAttribute<CollectionBindAttribute>() is { } col)
                        collectionBinds.Add(new CollectionField(field, field.FieldType, col.id));
                }

                foreach (var method in methods)
                {
                    if (method.GetCustomAttribute<OutputBindAttribute>() is { } m)
                    {
                        var param = method.GetParameters();
                        if (param.Length != 1) continue;
                        methodBinds.Add(new OutputMethod(param[0], method, m.id));
                    }
                }
                
                current = current.BaseType;
            }
            
            return new ViewMetadata(fieldBinds.ToArray(), methodBinds.ToArray(),
                inputFieldBinds.ToArray(), collectionBinds.ToArray());
        }
    }
}