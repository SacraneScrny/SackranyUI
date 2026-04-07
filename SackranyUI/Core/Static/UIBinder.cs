using System.Collections.Generic;
using System.Linq;

using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

namespace SackranyUI.Core.Static
{
    internal static class UIBinder
    {
        internal static IBinder[] Bind(ViewModel viewModel, IEnumerable<View> views)
        {
            var binders = new List<IBinder>();
            var vmMeta = ViewModelReflectionCache.GetViewModelMetadata(viewModel.GetType());
            var viewArray = views as View[] ?? views.ToArray();

            foreach (var view in viewArray)
            {
                var viewMeta = ViewReflectionCache.GetViewMetadata(view.GetType());

                foreach (var vmField in vmMeta.FieldBindings)
                {
                    var vmValue = vmField.Field.GetValue(viewModel);

                    foreach (var viewField in viewMeta.OutputFieldBindings)
                    {
                        if (vmField.Id?.Equals(view.RemapKey(viewField.Id)) != true) continue;
                        var bind = BinderFactory.CreateFieldToField(vmValue, viewField.Field.GetValue(view));
                        if (bind != null) binders.Add(bind);
                    }
                    foreach (var viewField in viewMeta.InputFieldBindings)
                    {
                        if (vmField.Id?.Equals(view.RemapKey(viewField.Id)) != true) continue;
                        var bind = BinderFactory.CreateFieldToField(vmValue, viewField.Field.GetValue(view));
                        if (bind != null) binders.Add(bind);
                    }
                    foreach (var viewMethod in viewMeta.OutputMethodBindings)
                    {
                        if (vmField.Id?.Equals(view.RemapKey(viewMethod.Id)) != true) continue;
                        var bind = BinderFactory.CreateForOutputMethod(vmValue, view, viewMethod.Method, viewMethod.Parameter);
                        if (bind != null) binders.Add(bind);
                    }
                    foreach (var colField in viewMeta.CollectionFieldBindings)
                    {
                        if (vmField.Id?.Equals(view.RemapKey(colField.Id)) != true) continue;
                        var bind = BinderFactory.CreateCollectionBinder(viewModel, vmValue, colField.Field.GetValue(view));
                        if (bind != null) binders.Add(bind);
                    }
                }

                foreach (var vmMethod in vmMeta.MethodBindings)
                {
                    foreach (var viewField in viewMeta.InputFieldBindings)
                    {
                        if (vmMethod.Id?.Equals(view.RemapKey(viewField.Id)) != true) continue;
                        var bind = BinderFactory.CreateForInputMethod(viewModel, vmMethod.Method, vmMethod.Parameter, viewField.Field.GetValue(view));
                        if (bind != null) binders.Add(bind);
                    }
                }
            }

            UIBindingValidator.Validate(viewModel, viewArray);

            return binders.ToArray();
        }
        internal static void BindInits(ViewModel viewModel, IEnumerable<View> views)
        {
            var vmMeta = ViewModelReflectionCache.GetViewModelMetadata(viewModel.GetType());
            var viewArray = views as View[] ?? views.ToArray();
            foreach (var initField in vmMeta.InitFieldBindings)
            {
                var initValue = initField.Field.GetValue(viewModel);
                foreach (var view in viewArray)
                {
                    var viewMeta = ViewReflectionCache.GetViewMetadata(view.GetType());
                    foreach (var viewField in viewMeta.InputFieldBindings)
                    {
                        if (initField.Id?.Equals(view.RemapKey(viewField.Id)) != true) continue;
                        BinderFactory.ApplyInitialValue(initValue, viewField.Field.GetValue(view));
                    }
                }
            }
        }
    }
}