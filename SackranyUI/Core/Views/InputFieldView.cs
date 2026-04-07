using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using TMPro;

using UnityEngine;

namespace SackranyUI.Core.Views
{
    [AddComponentMenu("Sackrany/UI/General/InputField")]
    public class InputFieldView : View
    {
        public string LabelKey = "label";
        public string InputKey = "input";
        public string InputActiveKey = "input_active";
        public string InputInteractableKey = "input_interactable";

        [OutputBind("label")] public TMP_Text Label;
        [InputBind("input")] [OutputBind("input_interactable")] public TMP_InputField InputField;
        [OutputBind("input_active")] GameObject _inputGo;

        protected override void OnBeforeBinding()
        {
            _inputGo = InputField.gameObject;
            Remap("label", LabelKey);
            Remap("input", InputKey);
            Remap("input_active", InputActiveKey);
            Remap("input_interactable", InputInteractableKey);
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            if (InputField == null)
                InputField = GetComponentInChildren<TMP_InputField>();
            if (Label == null)
                Label = GetComponentInChildren<TMP_Text>();
        }
        #endif
    }
}