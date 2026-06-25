using SackranyUI.Core.Entities;

using TMPro;

using UnityEngine;

namespace SackranyUI.Core.Views
{
    /// <summary>Binds a <c>TMP_Text</c> via the <c>text</c> channel.</summary>
    [AddComponentMenu("Sackrany/UI/General/Text")]
    public class TextView : ElementView
    {
        public string TextKey = "text";

        [OutputBind("text")] public TMP_Text Text;

        protected override void OnElementBinding()
        {
            Remap("text", TextKey);
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
