using System;

using Cysharp.Threading.Tasks;
using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using UnityEngine;

namespace SackranyUI.Core.Components
{
    [AddComponentMenu("Sackrany/UI/UserInterfaceInstaller")]
    public class UserInterfaceInstaller : MonoBehaviour
    {
        [SerializeField] Transform Content;
        [SerializeReference] [SubclassSelector] [SerializeField] IViewModelTemplate[] Default;
        [SerializeReference] [SubclassSelector] [SerializeField] IContext Context;

        #if UNITY_EDITOR
        void OnValidate()
        {
            if (Content == null)
            {
                Content = new GameObject("Content").transform;
                Content.SetParent(transform);
                Content.localScale = Vector3.one;
                Content.localRotation = Quaternion.identity;
                Content.localPosition = Vector3.zero;
            }
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            
            gameObject.name = "UserInterface";
            
            Context ??= new UIContext();
        }
        #endif
        
        void Awake()
        {
            Context.Init(Content, gameObject.GetCancellationTokenOnDestroy());
        }
        void Start()
        {
            Context.Add(Default, Content);
        }
    }
}