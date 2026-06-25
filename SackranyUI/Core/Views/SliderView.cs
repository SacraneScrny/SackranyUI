using SackranyUI.Core.Entities;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SackranyUI.Core.Views
{
    /// <summary>Two-way binds a <c>Slider</c> (via the <c>slider</c> channel) with an optional <c>label</c>.</summary>
    [AddComponentMenu("Sackrany/UI/General/Slider")]
    public class SliderView : SelectableView
    {
        public string LabelKey = "label";
        public string SliderKey = "slider";

        [OutputBind("label")] public TMP_Text Label;
        [InputBind("slider")] [OutputBind("slider")] public Slider Slider;

        protected override Selectable Target => Slider;

        protected override void OnSelectableBinding()
        {
            Remap("label", LabelKey);
            Remap("slider", SliderKey);
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
