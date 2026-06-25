using SackranyUI.Core.Entities;

using UnityEngine;
using UnityEngine.UI;

namespace SackranyUI.Core.Views
{
    /// <summary>Binds an <c>Image</c> sprite and fill amount via the <c>sprite</c> and <c>fill</c> channels.</summary>
    [AddComponentMenu("Sackrany/UI/General/Image")]
    public class ImageView : ElementView
    {
        public string SpriteKey = "sprite";
        public string FillKey = "fill";

        [OutputBind("sprite")] [OutputBind("fill")] public Image Image;

        protected override void OnElementBinding()
        {
            Remap("sprite", SpriteKey);
            Remap("fill", FillKey);
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            if (Image == null)
                Image = GetComponentInChildren<Image>();
        }
        #endif
    }
}
