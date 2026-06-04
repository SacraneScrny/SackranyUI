using SackranyUI.Core.Entities;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SackranyUI.Core.Views
{
    [AddComponentMenu("Sackrany/UI/General/InputField")]
    public class InputFieldView : SelectableView
    {
        public string LabelKey = "label";
        public string InputKey = "input";

        [OutputBind("label")] public TMP_Text Label;
        [InputBind("input")] [OutputBind("input")] public TMP_InputField InputField;

        protected override Selectable Target => InputField;

        protected override void OnSelectableBinding()
        {
            Remap("label", LabelKey);
            Remap("input", InputKey);
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
