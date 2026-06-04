using SackranyUI.Core.Entities;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SackranyUI.Core.Views
{
    [AddComponentMenu("Sackrany/UI/General/Dropdown")]
    public class DropdownView : SelectableView
    {
        public string LabelKey = "label";
        public string DropdownKey = "dropdown";

        [OutputBind("label")] public TMP_Text Label;
        [InputBind("dropdown")] [OutputBind("dropdown")] public TMP_Dropdown Dropdown;

        protected override Selectable Target => Dropdown;

        protected override void OnSelectableBinding()
        {
            Remap("label", LabelKey);
            Remap("dropdown", DropdownKey);
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            if (Dropdown == null)
                Dropdown = GetComponentInChildren<TMP_Dropdown>();
            if (Label == null && Dropdown != null)
                Label = Dropdown.captionText;
        }
        #endif
    }
}
