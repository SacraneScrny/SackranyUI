using System;
using System.Reflection;

namespace SackranyUI.Core.Static
{
    public readonly struct MemberRef
    {
        readonly FieldInfo _field;
        readonly PropertyInfo _property;

        public readonly string Name;
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

        public bool IsValid => _field != null || _property != null;

        public object GetValue(object target)
            => _field != null ? _field.GetValue(target) : _property?.GetValue(target);
    }
}
