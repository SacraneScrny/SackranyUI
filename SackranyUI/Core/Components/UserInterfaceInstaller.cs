using System;

using Cysharp.Threading.Tasks;
using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using UnityEngine;

namespace SackranyUI.Core.Components
{
    /// <summary>Scene entry point: on Start it initializes the context and instantiates the default view models.</summary>
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
                var rect = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
                rect.SetParent(transform, false);
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
                rect.localPosition = Vector3.zero;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                Content = rect;
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
        void OnDestroy()
        {
            Context?.Dispose();
        }
    }
}