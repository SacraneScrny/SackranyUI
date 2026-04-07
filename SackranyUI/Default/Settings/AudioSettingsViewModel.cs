using System;

using R3;

using Sackrany.ConfigSystem;
using Sackrany.GameAudio;
using Sackrany.GameAudio.Configurations;

using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;
using SackranyUI.Core.Events;
using SackranyUI.Core.Static;

using UnityEngine.Localization;

namespace SackranyUI.Default.Settings
{
    public class AudioSettingsViewModel : ViewModel<AudioSettings>
    {
        [InitBind("mute")] bool _initMute;
        [InitBind("masterVolume")] float _initMasterVolume;
        [InitBind("gameVolume")] float _initGameVolume;
        [InitBind("musicVolume")] float _initMusicVolume;
        [InitBind("uiVolume")] float _initUIVolume;
        
        [Bind("mute_label")] ReactiveProperty<string> _muteName = new ();
        [Bind("masterVolume_label")] ReactiveProperty<string> _masterVolumeName = new ();
        [Bind("gameVolume_label")] ReactiveProperty<string> _gameVolumeName = new ();
        [Bind("musicVolume_label")] ReactiveProperty<string> _musicVolumeName = new ();
        [Bind("uiVolume_label")] ReactiveProperty<string> _uiVolumeName = new ();
        
        [Bind("saveButton_label")] ReactiveProperty<string> _saveName = new ();
        [Bind("saveButton_active")] ReactiveProperty<bool> _saveActive = new ();
        
        GameAudioConfig _gameAudioConfig;
        
        protected override void OnInitialized()
        {
            Track(
                Template.MuteName?.Subscribe(t => _muteName.Value = t, "Mute"),
                Template.MasterVolumeName?.Subscribe(t => _masterVolumeName.Value = t, "Master"),
                Template.GameVolumeName?.Subscribe(t => _gameVolumeName.Value = t, "Game"),
                Template.MusicVolumeName?.Subscribe(t => _musicVolumeName.Value = t, "Music"),
                Template.UIVolumeName?.Subscribe(t => _uiVolumeName.Value = t, "UI"),
                Template.ApplyName?.Subscribe(t => _saveName.Value = t, "Save"),
                Subscribe<UIEvents.SettingsWindowCall, bool>(OnSettingsWindowCall)
            );
            
            LoadConfig();
        }
        void OnSettingsWindowCall(bool value)
        {
            _saveActive.Value = false;
            ConfigSet<GameAudioConfig>.DoAndSave(_gameAudioConfig);
            AudioManager.ApplySettings(_gameAudioConfig);
        }
        void LoadConfig()
        {
            _gameAudioConfig = ConfigGet<GameAudioConfig>.Value;
            _initMute = _gameAudioConfig.Muted;
            _initMasterVolume = _gameAudioConfig.MasterVolume;
            _initGameVolume = _gameAudioConfig.GameVolume;
            _initMusicVolume = _gameAudioConfig.MusicVolume;
            _initUIVolume = _gameAudioConfig.UIVolume;
        }

        [Bind("mute")]
        void MuteBind(bool value)
        {
            _saveActive.Value = true;
            _gameAudioConfig.Muted = value;
            AudioManager.ApplySettings(_gameAudioConfig);
        }        
        [Bind("masterVolume")]
        void MasterVolumeBind(float value)
        {
            _saveActive.Value = true;
            _gameAudioConfig.MasterVolume = value;
            AudioManager.ApplySettings(_gameAudioConfig);
        }
        [Bind("gameVolume")]
        void GameVolumeBind(float value)
        {
            _saveActive.Value = true;
            _gameAudioConfig.GameVolume = value;
            AudioManager.ApplySettings(_gameAudioConfig);
        }
        [Bind("musicVolume")]
        void MusicVolumeBind(float value)
        {
            _saveActive.Value = true;
            _gameAudioConfig.MusicVolume = value;
            AudioManager.ApplySettings(_gameAudioConfig);
        }
        [Bind("uiVolume")]
        void UIVolumeBind(float value)
        {
            _saveActive.Value = true;
            _gameAudioConfig.UIVolume = value;
            AudioManager.ApplySettings(_gameAudioConfig);
        }

        [Bind("saveButton")]
        void SaveSettings()
        {
            Close();
            Publish<UIEvents.SettingsWindowCall, bool>(false);
        }
    }

    [Serializable]
    public class AudioSettings : ViewModelTemplate<AudioSettingsViewModel>
    {
        public LocalizedString MuteName;
        public LocalizedString MasterVolumeName;
        public LocalizedString GameVolumeName;
        public LocalizedString MusicVolumeName;
        public LocalizedString UIVolumeName;
        
        public LocalizedString ApplyName;
    }
}