using SackranyUI.Core.Entities;

using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SackranyUI.Core.Views
{
    /// <summary>Base view for interactive elements, adding the <c>interactable</c> channel on top of <see cref="ElementView"/>.</summary>
    public abstract class SelectableView : ElementView
    {
        const string InteractableId = "__element_interactable";

        [FormerlySerializedAs("ButtonInteractableKey")]
        [FormerlySerializedAs("SliderInteractableKey")]
        [FormerlySerializedAs("ToggleInteractableKey")]
        [FormerlySerializedAs("InputInteractableKey")]
        [FormerlySerializedAs("DropdownInteractableKey")]
        public string InteractableKey = "interactable";

        protected abstract Selectable Target { get; }

        protected sealed override void OnElementBinding()
        {
            Remap(InteractableId, InteractableKey);
            OnSelectableBinding();
        }

        protected virtual void OnSelectableBinding() { }

        [OutputBind(InteractableId)]
        void SetInteractable(bool value)
        {
            if (Target != null) Target.interactable = value;
        }
    }
}
