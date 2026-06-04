using SackranyUI.Core.Entities;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SackranyUI.Core.Views
{
    [AddComponentMenu("Sackrany/UI/General/Button")]
    public class ButtonView : SelectableView
    {
        public string TitleKey = "title_text";
        public string TitleColorKey = "title_color";
        public string ButtonKey = "button";

        [OutputBind("title_text")] public TMP_Text Title;
        [OutputBind("title_color")] TMP_Text _titleColor;
        [InputBind("button")] public Button Button;

        protected override Selectable Target => Button;

        protected override void OnSelectableBinding()
        {
            _titleColor = Title;
            Remap("title_text", TitleKey);
            Remap("title_color", TitleColorKey);
            Remap("button", ButtonKey);
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
