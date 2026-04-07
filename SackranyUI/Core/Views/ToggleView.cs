using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SackranyUI.Core.Views
{
    [AddComponentMenu("Sackrany/UI/General/Toggle")]
    public class ToggleView : View
    {
        public string LabelKey = "label";
        public string ToggleKey = "toggle";
        public string ToggleActiveKey = "toggle_active";
        public string ToggleInteractableKey = "toggle_interactable";

        [OutputBind("label")] public TMP_Text Label;
        [InputBind("toggle")] [OutputBind("toggle_interactable")] public Toggle Toggle;
        [OutputBind("toggle_active")] GameObject _toggleGo;

        protected override void OnBeforeBinding()
        {
            _toggleGo = Toggle.gameObject;
            Remap("label", LabelKey);
            Remap("toggle", ToggleKey);
            Remap("toggle_active", ToggleActiveKey);
            Remap("toggle_interactable", ToggleInteractableKey);
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