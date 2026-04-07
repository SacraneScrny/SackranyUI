using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using UnityEngine;
using UnityEngine.UI;

namespace SackranyUI.Core.Views
{
    [AddComponentMenu("Sackrany/UI/General/Image")]
    public class ImageView : View
    {
        public string SpriteKey = "sprite";
        public string ColorKey = "color";
        public string FillKey = "fill";
        public string ImageActiveKey = "image_active";

        [OutputBind("sprite")] [OutputBind("color")] [OutputBind("fill")] public Image Image;
        [OutputBind("image_active")] GameObject _imageGo;

        protected override void OnBeforeBinding()
        {
            _imageGo = Image.gameObject;
            Remap("sprite", SpriteKey);
            Remap("color", ColorKey);
            Remap("fill", FillKey);
            Remap("image_active", ImageActiveKey);
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