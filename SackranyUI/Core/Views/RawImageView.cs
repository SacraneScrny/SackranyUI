using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using UnityEngine;
using UnityEngine.UI;

namespace SackranyUI.Core.Views
{
    [AddComponentMenu("Sackrany/UI/General/RawImage")]
    public class RawImageView : View
    {
        public string TextureKey = "texture";
        public string ColorKey = "raw_color";
        public string RawImageActiveKey = "rawimage_active";

        [OutputBind("texture")] [OutputBind("raw_color")] public RawImage RawImage;
        [OutputBind("rawimage_active")] GameObject _rawImageGo;

        protected override void OnBeforeBinding()
        {
            if (RawImage != null) _rawImageGo = RawImage.gameObject;
            Remap("texture", TextureKey);
            Remap("raw_color", ColorKey);
            Remap("rawimage_active", RawImageActiveKey);
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            if (RawImage == null)
                RawImage = GetComponentInChildren<RawImage>();
        }
        #endif
    }
}
