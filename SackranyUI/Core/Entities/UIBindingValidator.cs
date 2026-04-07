using System.Collections.Generic;
using System.Linq;

using SackranyUI.Core.Base;
using SackranyUI.Core.Static;

using UnityEngine;

namespace SackranyUI.Core.Entities
{
    internal static class UIBindingValidator
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Validate(ViewModel viewModel, View[] views)
        {
            var vmType = viewModel.GetType();
            var vmMeta = ViewModelReflectionCache.GetViewModelMetadata(vmType);

            var vmIds = new HashSet<object>(
                vmMeta.FieldBindings.Select(f => f.Id)
                    .Concat(vmMeta.MethodBindings.Select(m => m.Id))
                    .Where(id => id != null)
            );

            var matchedVmIds = new HashSet<object>();

            foreach (var view in views)
            {
                var viewMeta = ViewReflectionCache.GetViewMetadata(view.GetType());

                Check(viewMeta.OutputFieldBindings.Select(f => (f.Id, f.Field.Name, "OutputBind field")), view, vmType, vmIds, matchedVmIds);
                Check(viewMeta.OutputMethodBindings.Select(m => (m.Id, m.Method.Name, "OutputBind method")), view, vmType, vmIds, matchedVmIds);
                Check(viewMeta.InputFieldBindings.Select(f => (f.Id, f.Field.Name, "InputBind field")), view, vmType, vmIds, matchedVmIds);
                Check(viewMeta.CollectionFieldBindings.Select(f => (f.Id, f.Field.Name, "CollectionBind field")), view, vmType, vmIds, matchedVmIds);
            }

            foreach (var id in vmIds.Where(id => !matchedVmIds.Contains(id)))
                Debug.LogWarning($"[UIValidator] {vmType.Name} — [Bind(\"{id}\")] не имеет совпадения ни в одном View");
        }

        static void Check(
            IEnumerable<(object id, string memberName, string kind)> entries,
            View view, System.Type vmType,
            HashSet<object> vmIds, HashSet<object> matchedVmIds)
        {
            foreach (var (id, name, kind) in entries)
            {
                if (vmIds.Contains(id))
                    matchedVmIds.Add(id);
                else
                    Debug.LogWarning($"[UIValidator] {view.GetType().Name}.{name} [{kind}(\"{id}\")] — нет [Bind(\"{id}\")] в {vmType.Name}");
            }
        }
    }
}