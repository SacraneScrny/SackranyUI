using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;

using TMPro;

using UnityEngine.UI;

namespace SackranyUI.Default.Settings
{
    public class AudioSettingsView : View
    {
        [OutputBind("muteName")] TMP_Text _muteName;
        [OutputBind("masterName")] TMP_Text _masterName;
        [OutputBind("gameName")] TMP_Text _gameName;
        [OutputBind("musicName")] TMP_Text _musicName;
        [OutputBind("uiName")] TMP_Text _uiName;
        
        [OutputBind("cancelName")] TMP_Text _cancelName;
        [OutputBind("applyName")] TMP_Text _applyName;
        
        [InputBind("mute")] public Toggle Mute;
        [InputBind("masterVolume")] public Slider MasterVolume;
        [InputBind("gameVolume")] public Slider GameVolume;
        [InputBind("musicVolume")] public Slider MusicVolume;
        [InputBind("uiVolume")] public Slider UIVolume;

        [InputBind("cancel")] public Button Cancel;
        [InputBind("apply")] public Button Apply;
        
        protected override void OnBeforeBinding()
        {
            _muteName = Mute.GetComponentInChildren<TMP_Text>();
            _masterName = MasterVolume.transform.parent.GetComponentInChildren<TMP_Text>();
            _gameName = GameVolume.transform.parent.GetComponentInChildren<TMP_Text>();
            _musicName = MusicVolume.transform.parent.GetComponentInChildren<TMP_Text>();
            _uiName = UIVolume.transform.parent.GetComponentInChildren<TMP_Text>();
            
            _cancelName = Cancel.GetComponentInChildren<TMP_Text>();
            _applyName = Apply.GetComponentInChildren<TMP_Text>();
        }
    }
}