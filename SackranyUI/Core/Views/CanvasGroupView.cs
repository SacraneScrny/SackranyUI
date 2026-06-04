using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using UnityEngine;

namespace SackranyUI.Core.Views
{
    [AddComponentMenu("Sackrany/UI/General/CanvasGroup")]
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupView : View
    {
        public string AlphaKey = "alpha";
        public string InteractableKey = "canvas_interactable";
        public string BlocksRaycastsKey = "canvas_blocks";
        public string CanvasActiveKey = "canvas_active";

        [OutputBind("alpha")] public CanvasGroup CanvasGroup;
        [OutputBind("canvas_active")] GameObject _canvasGo;

        protected override void OnBeforeBinding()
        {
            if (CanvasGroup == null) CanvasGroup = GetComponent<CanvasGroup>();
            if (CanvasGroup != null) _canvasGo = CanvasGroup.gameObject;
            Remap("alpha", AlphaKey);
            Remap("canvas_active", CanvasActiveKey);
            Remap("canvas_interactable", InteractableKey);
            Remap("canvas_blocks", BlocksRaycastsKey);
        }

        [OutputBind("canvas_interactable")]
        void SetInteractable(bool value)
        {
            if (CanvasGroup != null) CanvasGroup.interactable = value;
        }

        [OutputBind("canvas_blocks")]
        void SetBlocksRaycasts(bool value)
        {
            if (CanvasGroup != null) CanvasGroup.blocksRaycasts = value;
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            if (CanvasGroup == null) CanvasGroup = GetComponent<CanvasGroup>();
        }
        #endif
    }
}
