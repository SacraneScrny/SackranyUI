using Cysharp.Threading.Tasks;

using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using UnityEngine;

namespace SackranyUI.Core.Components
{
    [AddComponentMenu("Sackrany/UserInterfaceInstaller")]
    [RequireComponent(typeof(UIValidator))]
    public class UserInterfaceInstaller : MonoBehaviour
    {
        public Transform Content;
        [SerializeReference] [SubclassSelector] public IViewModelTemplate[] Default;
        
        public UIContext Context { get; private set; }
        
        void Awake()
        {
            Context = new UIContext(Content, gameObject.GetCancellationTokenOnDestroy());
        }
        void Start()
        {
            Context.Add(Default);
        }
    }
}