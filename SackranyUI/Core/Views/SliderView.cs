using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SackranyUI.Core.Views
{
    [AddComponentMenu("Sackrany/UI/General/Slider")]
    public class SliderView : View
    {
        public string LabelKey = "label";
        public string SliderKey = "slider";
        public string SliderActiveKey = "slider_active";
        public string SliderInteractableKey = "slider_interactable";

        [OutputBind("label")] public TMP_Text Label;
        [InputBind("slider")] [OutputBind("slider_interactable")] public Slider Slider;
        [OutputBind("slider_active")] GameObject _sliderGo;

        protected override void OnBeforeBinding()
        {
            _sliderGo = Slider.gameObject;
            Remap("label", LabelKey);
            Remap("slider", SliderKey);
            Remap("slider_active", SliderActiveKey);
            Remap("slider_interactable", SliderInteractableKey);
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            if (Slider == null)
                Slider = GetComponentInChildren<Slider>();
            if (Label == null)
                Label = GetComponentInChildren<TMP_Text>();
        }
        #endif
    }
}