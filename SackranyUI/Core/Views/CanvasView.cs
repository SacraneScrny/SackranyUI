using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using UnityEngine;

namespace SackranyUI.Core.Views
{
    /// <summary>
    /// Binds a <see cref="Canvas"/>'s sorting order and override flag, plus a <see cref="CanvasGroup"/>
    /// alpha, via the remappable <c>sort_order</c>, <c>override_sorting</c> and <c>alpha</c> channels.
    /// </summary>
    [AddComponentMenu("Sackrany/UI/General/Canvas")]
    [RequireComponent(typeof(Canvas))]
    public class CanvasView : View
    {
        /// <summary>Per-instance key for the canvas sorting order.</summary>
        public string SortOrderKey = "sort_order";
        /// <summary>Per-instance key for the canvas override-sorting flag.</summary>
        public string OverrideSortingKey = "override_sorting";
        /// <summary>Per-instance key for the canvas group alpha.</summary>
        public string AlphaKey = "alpha";

        [SerializeField] Canvas _canvas;
        [SerializeField] CanvasGroup _canvasGroup;

        protected override void OnBeforeBinding()
        {
            if (_canvas == null) _canvas = GetComponent<Canvas>();
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            Remap("sort_order", SortOrderKey);
            Remap("override_sorting", OverrideSortingKey);
            Remap("alpha", AlphaKey);
        }

        [OutputBind("sort_order")]
        void SetSortOrder(int value)
        {
            if (_canvas != null) _canvas.sortingOrder = value;
        }

        [OutputBind("override_sorting")]
        void SetOverrideSorting(bool value)
        {
            if (_canvas != null) _canvas.overrideSorting = value;
        }

        [OutputBind("alpha")]
        void SetAlpha(float value)
        {
            if (_canvasGroup != null) _canvasGroup.alpha = value;
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            if (_canvas == null) _canvas = GetComponent<Canvas>();
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        }
        #endif
    }
}
