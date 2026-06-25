using System;

using JetBrains.Annotations;

namespace SackranyUI.Core.Entities
{
    /// <summary>Base attribute that tags a member with a binding key.</summary>
    [MeansImplicitUse]
    public abstract class MemberBindAttribute : Attribute
    {
        public object id;
        protected MemberBindAttribute(object id)
        {
            this.id = id;
        }
    }

    /// <summary>View model side: exposes a reactive member as output, or a method as an input handler, under the given key.</summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class BindAttribute : MemberBindAttribute
    {
        public BindAttribute(object id) : base(id) { }
    }
    /// <summary>View model side: pushes a one-shot initial value into the view under the given key.</summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class InitBindAttribute : MemberBindAttribute
    {
        public InitBindAttribute(object id) : base(id) { }
    }
    /// <summary>View side: receives values from the view (e.g. user input) for the given key.</summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class InputBindAttribute : MemberBindAttribute
    {
        public InputBindAttribute(object id) : base(id) { }
    }
    /// <summary>View side: receives values from the view model (view model → view) for the given key.</summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class OutputBindAttribute : MemberBindAttribute
    {
        public OutputBindAttribute(object id) : base(id) { }
    }
    /// <summary>View side: marks a <c>CollectionAnchor</c> that spawns one child view model per list item for the given key.</summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class CollectionBindAttribute : MemberBindAttribute
    {
        public CollectionBindAttribute(object id) : base(id) { }
    }

    /// <summary>Marks the field that receives the injected template data on a view model.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class UITemplateAttribute : Attribute
    {
        public UITemplateAttribute()
        {
            
        }
    }
}