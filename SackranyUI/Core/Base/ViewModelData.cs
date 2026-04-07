using System.Collections.Generic;

using SackranyUI.Core.Entities;

using UnityEngine;

namespace SackranyUI.Core.Base
{
    internal class ViewModelData
    {
        public ViewModel ViewModel;
        public GameObject Prefab;
        public View[] Views;
        public IBinder[] Binders;
        public Dictionary<string, Transform> Anchors;
    }
}