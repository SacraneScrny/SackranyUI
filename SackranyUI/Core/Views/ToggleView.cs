using SackranyUI.Core.Entities;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SackranyUI.Core.Views
{
    /// <summary>Two-way binds a <c>Toggle</c> (via the <c>toggle</c> channel) with an optional <c>label</c>.</summary>
    [AddComponentMenu("Sackrany/UI/General/Toggle")]
    public class ToggleView : SelectableView
    {
        public string LabelKey = "label";
        public string ToggleKey = "toggle";

        [OutputBind("label")] public TMP_Text Label;
        [InputBind("toggle")] [OutputBind("toggle")] public Toggle Toggle;

        protected override Selectable Target => Toggle;

        protected override void OnSelectableBinding()
        {
            Remap("label", LabelKey);
            Remap("toggle", ToggleKey);
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            if (Toggle == null)
                Toggle = GetComponentInChildren<Toggle>();
            if (Label == null && Toggle != null)
                Label = Toggle.GetComponentInChildren<TMP_Text>();
        }
        #endif
    }
}
