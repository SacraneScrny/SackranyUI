using System;

using JetBrains.Annotations;

namespace SackranyUI.Core.Entities
{
    [MeansImplicitUse]
    public abstract class MemberBindAttribute : Attribute
    {
        public object id;
        protected MemberBindAttribute(object id)
        {
            this.id = id;
        }
    }
    
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class BindAttribute : MemberBindAttribute
    {
        public BindAttribute(object id) : base(id) { }
    }
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class InitBindAttribute : MemberBindAttribute
    {
        public InitBindAttribute(object id) : base(id) { }
    }
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class InputBindAttribute : MemberBindAttribute
    {
        public InputBindAttribute(object id) : base(id) { }
    }    
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class OutputBindAttribute : MemberBindAttribute
    {
        public OutputBindAttribute(object id) : base(id) { }
    }
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CollectionBindAttribute : MemberBindAttribute
    {
        public CollectionBindAttribute(object id) : base(id) { }
    }    

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class UITemplateAttribute : Attribute
    {
        public UITemplateAttribute()
        {
            
        }
    }
}