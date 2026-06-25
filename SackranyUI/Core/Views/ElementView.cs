using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SackranyUI.Core.Views
{
    /// <summary>Base view for visual elements, adding the shared <c>alpha</c>, <c>color</c> and <c>active</c> channels to every child graphic.</summary>
    public abstract class ElementView : View
    {
        const string AlphaId = "__element_alpha";
        const string ColorId = "__element_color";
        const string ActiveId = "__element_active";

        public string AlphaKey = "alpha";

        public string ColorKey = "color";

        [FormerlySerializedAs("ButtonActiveKey")]
        [FormerlySerializedAs("TextActiveKey")]
        [FormerlySerializedAs("SliderActiveKey")]
        [FormerlySerializedAs("ToggleActiveKey")]
        [FormerlySerializedAs("InputActiveKey")]
        [FormerlySerializedAs("ImageActiveKey")]
        [FormerlySerializedAs("DropdownActiveKey")]
        [FormerlySerializedAs("RawImageActiveKey")]
        public string ActiveKey = "active";

        Graphic[] _graphics;
        [OutputBind(ActiveId)] GameObject _elementGo;

        protected sealed override void OnBeforeBinding()
        {
            _graphics = GetComponentsInChildren<Graphic>(true);
            _elementGo = gameObject;
            Remap(AlphaId, AlphaKey);
            Remap(ColorId, ColorKey);
            Remap(ActiveId, ActiveKey);
            OnElementBinding();
        }

        protected virtual void OnElementBinding() { }

        [OutputBind(AlphaId)]
        void SetAlpha(float value)
        {
            if (_graphics == null) return;
            for (int i = 0; i < _graphics.Length; i++)
            {
                var graphic = _graphics[i];
                if (graphic == null) continue;
                var color = graphic.color;
                color.a = value;
                graphic.color = color;
            }
        }

        [OutputBind(ColorId)]
        void SetColor(Color value)
        {
            if (_graphics == null) return;
            for (int i = 0; i < _graphics.Length; i++)
                if (_graphics[i] != null) _graphics[i].color = value;
        }
    }
}
