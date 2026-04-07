using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SackranyUI.Default.MainMenu
{
    public class SaveSlotView : View
    {
        public RawImage SaveSlotImage;
        [OutputBind("saveSlotNumber")] public TMP_Text SaveSlotNumber;
        [OutputBind("saveSlotDate")] public TMP_Text SaveSlotDate;
        [OutputBind("saveSlotTime")] public TMP_Text SaveSlotTime;
        [OutputBind("saveSlotActive")] public GameObject SaveSlotActive;
        
        [InputBind("continueGame")] public Button ContinueGame;
        
        Texture2D _saveSlotImage;
        [OutputBind("saveSlotImage")] 
        void UpdateSaveSlotImage(byte[] image)
        {
            if (_saveSlotImage != null)
                Object.Destroy(_saveSlotImage);
            _saveSlotImage = new Texture2D(1, 1);
            _saveSlotImage.LoadImage(image);
            
            SaveSlotImage.texture = _saveSlotImage;
        }
        
        protected override void OnDestroyView()
        {
            if (_saveSlotImage != null)
                Object.Destroy(_saveSlotImage);
        }
    }
}