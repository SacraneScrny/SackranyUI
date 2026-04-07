using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using TMPro;

using UnityEngine;

namespace SackranyUI.Core.Views
{
    [AddComponentMenu("Sackrany/UI/General/Text")]
    public class TextView : View
    {
        public string TextKey = "text";
        public string TextActiveKey = "text_active";

        [OutputBind("text")] public TMP_Text Text;
        [OutputBind("text_active")] GameObject _textGo;

        protected override void OnBeforeBinding()
        {
            _textGo = Text.gameObject;
            Remap("text", TextKey);
            Remap("text_active", TextActiveKey);
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            if (Text == null)
                Text = GetComponentInChildren<TMP_Text>();
        }
        #endif
    }
}