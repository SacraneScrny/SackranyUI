using UnityEngine;

namespace SackranyUI.Core.Components
{
    /// <summary>Names a child transform so a view model can resolve it via <c>GetAnchorOrDefault</c>.</summary>
    [AddComponentMenu("Sackrany/UI/Anchor")]
    public class Anchor : MonoBehaviour
    {
        /// <summary>Lookup key for this anchor.</summary>
        public string Key;
    }
}