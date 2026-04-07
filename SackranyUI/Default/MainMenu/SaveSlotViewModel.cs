using System;

using R3;

using Sackrany.SerializableData.Storage;

using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;
using SackranyUI.Core.Static;

using UnityEngine.Localization;

namespace SackranyUI.Default.MainMenu
{
    public class SaveSlotViewModel : ViewModel
    {
        SaveSlotMeta _meta;
        int _slotNumber;
        
        [Bind("saveSlotNumber")] ReactiveProperty<string> _saveSlotNumber = new ();
        [Bind("saveSlotDate")] ReactiveProperty<string> _saveDate = new ();
        [Bind("saveSlotTime")] ReactiveProperty<string> _saveTime = new ();
        [Bind("saveSlotImage")] ReactiveProperty<byte[]> _saveSlotImage = new ();
        [Bind("saveSlotActive")] ReactiveProperty<bool> _saveSlotActive = new ();
        
        CompositeDisposable _localizations;
        const string TimeFormat = @"hh\:mm\:ss";
        
        public void SetSlotNumber(int number) => _slotNumber = number;

        CompositeDisposable _disposables;
        
        public void SetSlot(
            SaveSlotMeta meta,
            LocalizedString dateText,
            LocalizedString timeText,
            LocalizedString numText)
        {
            _meta = meta;

            DisposeTracked();
            Track(
                numText.Subscribe(t => _saveSlotNumber.Value = string.Format(t, _slotNumber), 
                    $"Slot: {_slotNumber}"),
                dateText.Subscribe(t => _saveDate.Value = string.Format(t, meta.SavedAt.ToString("G")), 
                    $"Last save: {meta.SavedAt:G}"),
                timeText.Subscribe(t => _saveTime.Value = string.Format(t, meta.PlayTime.ToString(TimeFormat)), 
                    $"Time played: {meta.PlayTime.ToString(TimeFormat)}")
            );
            
            if (meta.Thumbnail != null)
                _saveSlotImage.Value = meta.Thumbnail;
        }
        public void SetSlotActive(bool active)
        {
            _saveSlotActive.Value = active;
        }

        [Bind("continueGame")]
        void ContinueGame()
        {
            OnContinueGame?.Invoke(_slotNumber);
        }
        
        protected override void OnInitialized()
        {
            
        }
        
        public event Action<int> OnContinueGame; 
    }
}