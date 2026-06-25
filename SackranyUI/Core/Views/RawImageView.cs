using SackranyUI.Core.Entities;

using UnityEngine;
using UnityEngine.UI;

namespace SackranyUI.Core.Views
{
    /// <summary>Binds a <c>RawImage</c> texture via the <c>texture</c> channel.</summary>
    [AddComponentMenu("Sackrany/UI/General/RawImage")]
    public class RawImageView : ElementView
    {
        public string TextureKey = "texture";

        [OutputBind("texture")] public RawImage RawImage;

        protected override void OnElementBinding()
        {
            Remap("texture", TextureKey);
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
