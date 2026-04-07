using System;

using R3;

namespace SackranyUI.Core.Entities.Binders
{
    internal sealed class GenericBinder<T> : Observer<T>, IBinder
    {
        readonly ReadOnlyReactiveProperty<T> _property;
        readonly Action<T> _setter;
        IDisposable _handle;

        public GenericBinder(ReadOnlyReactiveProperty<T> property, Action<T> setter)
        {
            _property = property;
            _setter = setter;
        }

        public void Bind()
        {
            Unbind();
            OnNextCore(_property.CurrentValue);
            _handle = _property.Subscribe(this);
        }
        public void Unbind()
        {
            _handle?.Dispose();
            _handle = null;
        }

        protected override void OnNextCore(T value) => _setter(value);
        protected override void OnErrorResumeCore(Exception error) { }
        protected override void OnCompletedCore(Result result) { }
    }
}