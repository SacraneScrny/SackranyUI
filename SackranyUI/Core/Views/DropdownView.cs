using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using TMPro;

using UnityEngine;

namespace SackranyUI.Core.Views
{
    [AddComponentMenu("Sackrany/UI/General/Dropdown")]
    public class DropdownView : View
    {
        public string LabelKey = "label";
        public string DropdownKey = "dropdown";
        public string DropdownActiveKey = "dropdown_active";
        public string DropdownInteractableKey = "dropdown_interactable";

        [OutputBind("label")] public TMP_Text Label;
        [InputBind("dropdown")] [OutputBind("dropdown")] [OutputBind("dropdown_interactable")] public TMP_Dropdown Dropdown;
        [OutputBind("dropdown_active")] GameObject _dropdownGo;

        protected override void OnBeforeBinding()
        {
            if (Dropdown != null) _dropdownGo = Dropdown.gameObject;
            Remap("label", LabelKey);
            Remap("dropdown", DropdownKey);
            Remap("dropdown_active", DropdownActiveKey);
            Remap("dropdown_interactable", DropdownInteractableKey);
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
