using UnityEngine;

namespace SackranyUI.Core.Components
{
    /// <summary>Marks where a bound <c>ReactiveList</c> spawns one child per item, using <see cref="Prefab"/> under <see cref="Container"/>.</summary>
    [AddComponentMenu("Sackrany/UI/CollectionAnchor")]
    public class CollectionAnchor : MonoBehaviour
    {
        /// <summary>Parent transform for spawned items.</summary>
        public Transform Container;
        /// <summary>Prefab instantiated per list item.</summary>
        public GameObject Prefab;
    }
}