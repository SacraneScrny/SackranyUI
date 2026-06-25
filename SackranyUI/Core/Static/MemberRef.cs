using System;
using System.Reflection;

namespace SackranyUI.Core.Static
{
    /// <summary>Uniform read access to a field or property, used by the binding reflection caches.</summary>
    public readonly struct MemberRef
    {
        readonly FieldInfo _field;
        readonly PropertyInfo _property;

        /// <summary>Declared member name.</summary>
        public readonly string Name;
        /// <summary>Declared member value type.</summary>
        public readonly Type ValueType;

        public MemberRef(FieldInfo field)
        {
            _field = field;
            _property = null;
            Name = field.Name;
            ValueType = field.FieldType;
        }
        public MemberRef(PropertyInfo property)
        {
            _field = null;
            _property = property;
            Name = property.Name;
            ValueType = property.PropertyType;
        }

        /// <summary>True if this reference wraps a real field or property.</summary>
        public bool IsValid => _field != null || _property != null;

        /// <summary>Reads the member value from <paramref name="target"/>.</summary>
        public object GetValue(object target)
            => _field != null ? _field.GetValue(target) : _property?.GetValue(target);
    }
}
