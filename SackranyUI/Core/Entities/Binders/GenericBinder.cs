using System;

using R3;

using UnityEngine;

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

        protected override void OnNextCore(T value)
        {
            try
            {
                _setter(value);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        protected override void OnErrorResumeCore(Exception error) => Debug.LogException(error);
        protected override void OnCompletedCore(Result result)
        {
            if (result.IsFailure)
                Debug.LogException(result.Exception);
        }
    }
}
