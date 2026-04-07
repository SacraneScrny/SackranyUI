using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SackranyUI.Core.Views
{
    [AddComponentMenu("Sackrany/UI/General/Button")]
    public class ButtonView : View
    {
        public string TitleKey = "title_text";
        public string ButtonKey = "button";
        public string ButtonActiveKey = "button_active";
        public string ButtonInteractableKey = "button_interactable";

        [OutputBind("title_text")] public TMP_Text Title;
        [InputBind("button")] [OutputBind("button_interactable")] public Button Button;
        [OutputBind("button_active")] GameObject _buttonGo;

        protected override void OnBeforeBinding()
        {
            _buttonGo = Button.gameObject;
            Remap("title_text", TitleKey);
            Remap("button", ButtonKey);
            Remap("button_active", ButtonActiveKey);
            Remap("button_interactable", ButtonInteractableKey);
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            if (Button == null)
                Button = GetComponentInChildren<Button>();
            if (Title == null && Button != null)
                Title = Button.GetComponentInChildren<TMP_Text>();
        }
        #endif
    }
}