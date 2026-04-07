using UnityEngine;

namespace SackranyUI.Core.Components
{
    [RequireComponent(typeof(UserInterfaceInstaller))]
    public class UIValidator : MonoBehaviour
    {
        UserInterfaceInstaller _userInterfaceInstaller;
        void OnValidate()
        {
            if (!GetComponent<UserInterfaceInstaller>()) return;
            _userInterfaceInstaller = GetComponent<UserInterfaceInstaller>();
            if (_userInterfaceInstaller.Content == null)
            {
                _userInterfaceInstaller.Content = new GameObject("Content").transform;
                _userInterfaceInstaller.Content.SetParent(transform);
                _userInterfaceInstaller.Content.localScale = Vector3.one;
                _userInterfaceInstaller.Content.localRotation = Quaternion.identity;
                _userInterfaceInstaller.Content.localPosition = Vector3.zero;
            }
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            
            gameObject.name = "UserInterface";
        }
    }
}