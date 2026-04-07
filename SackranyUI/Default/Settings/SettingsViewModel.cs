using System;

using R3;

using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;
using SackranyUI.Core.Events;
using SackranyUI.Core.Static;

using UnityEngine;
using UnityEngine.Localization;

namespace SackranyUI.Default.Settings
{
    public class SettingsViewModel : ViewModel<Settings>
    {
        [Bind("settings_title")] ReactiveProperty<string> _title = new ();
        [Bind("cancelButton_label")] ReactiveProperty<string> _cancelName = new ();

        [Bind("audioButton_label")] ReactiveProperty<string> _audioButtonLabel = new ReactiveProperty<string>();
        [Bind("graphicsButton_label")] ReactiveProperty<string> _graphicsButtonLabel = new ReactiveProperty<string>();
        [Bind("controlsButton_label")] ReactiveProperty<string> _controlsButtonLabel = new ReactiveProperty<string>();
        [Bind("gameButton_label")] ReactiveProperty<string> _gameButtonLabel = new ReactiveProperty<string>();
        
        [Bind("audioButton_active")] ReactiveProperty<bool> _audioButtonActive = new ReactiveProperty<bool>();
        [Bind("graphicsButton_active")] ReactiveProperty<bool> _graphicsButtonActive = new ReactiveProperty<bool>();
        [Bind("controlsButton_active")] ReactiveProperty<bool> _controlsButtonActive = new ReactiveProperty<bool>();
        [Bind("gameButton_active")] ReactiveProperty<bool> _gameButtonActive = new ReactiveProperty<bool>();
        
        [Bind("audioButton_interactable")] ReactiveProperty<bool> _audioButtonInteractable = new ReactiveProperty<bool>();
        [Bind("graphicsButton_interactable")] ReactiveProperty<bool> _graphicsButtonInteractable = new ReactiveProperty<bool>();
        [Bind("controlsButton_interactable")] ReactiveProperty<bool> _controlsButtonInteractable = new ReactiveProperty<bool>();
        [Bind("gameButton_interactable")] ReactiveProperty<bool> _gameButtonInteractable = new ReactiveProperty<bool>();
        
        ViewModel _audioViewModel;
        ViewModel _graphicsViewModel;
        ViewModel _controlsViewModel;
        ViewModel _gameViewModel;

        CompositeDisposable _disposables;
        
        protected override void OnInitialized()
        {
            _audioButtonInteractable.Value = false;
            
            if (Template.AudioSettingsWindow != null)
                _audioViewModel = (Add(Template.AudioSettingsWindow, GetAnchorOrDefault("SettingsContent")));
            if (Template.GraphicsSettingsWindow != null)
                _graphicsViewModel = (Add(Template.GraphicsSettingsWindow, GetAnchorOrDefault("SettingsContent")));
            if (Template.ControlsSettingsWindow != null)
                _controlsViewModel = (Add(Template.ControlsSettingsWindow, GetAnchorOrDefault("SettingsContent")));
            if (Template.GameSettingsWindow != null)
                _gameViewModel = (Add(Template.GameSettingsWindow, GetAnchorOrDefault("SettingsContent")));

            _audioButtonActive.Value = Template.Audio;
            _graphicsButtonActive.Value = Template.Graphics;
            _controlsButtonActive.Value = Template.Controls;
            _gameButtonActive.Value = Template.Game;
            
            Track(
                Template.Title?.Subscribe(t => _title.Value = t, "Settings"),
                Template.CancelName?.Subscribe(t => _cancelName.Value = t, "Cancel"),
                
                Template.AudioName?.Subscribe(t => _audioButtonLabel.Value = t, "Audio"),
                Template.GraphicsName?.Subscribe(t => _graphicsButtonLabel.Value = t, "Graphic"),
                Template.ControlsName?.Subscribe(t => _controlsButtonLabel.Value = t, "Control"),
                Template.GameName?.Subscribe(t => _gameButtonLabel.Value = t, "Game"),
                
                _audioViewModel, _graphicsViewModel, _controlsViewModel, _gameViewModel,
                Subscribe<UIEvents.SettingsWindowCall, bool>(OnSettingsWindowCall)
            );
        }
        void OnSettingsWindowCall(bool value)
        {
            if (!HasAny()) return;
            
            if (value && IsOpened)
            {
                Close();
                return;
            }
            if (!value)
            {
                Close();
                return;
            }
            
            Open();
            
            if (!OpenAny())
            {
                CallAudio();
            }
        }

        [Bind("audioButton")]
        void CallAudio()
        {
            if (_audioViewModel == null) return;
            CloseAll();
            _audioViewModel.Open();
            _audioButtonInteractable.Value = false;
        }
        [Bind("graphicsButton")]
        void CallGraphics()
        {
            if (_graphicsViewModel == null) return;
            CloseAll();
            _graphicsViewModel.Open();
            _graphicsButtonInteractable.Value = false;
        }      
        [Bind("controlsButton")] 
        void CallControls()
        {
            if (_controlsViewModel == null) return;
            CloseAll();
            _controlsViewModel.Open();
            _controlsButtonInteractable.Value = false;
        }
        [Bind("gameButton")]
        void CallGame()
        {
            if (_gameViewModel == null) return;
            CloseAll();
            _gameViewModel.Open();
            _gameButtonInteractable.Value = false;
        }
        
        [Bind("cancelButton")]
        void CancelSettings()
        {
            Publish<UIEvents.SettingsWindowCall, bool>(false);
            Close();
        }

        void CloseAll()
        {
            _audioViewModel?.Close();
            _graphicsViewModel?.Close();
            _controlsViewModel?.Close();
            _gameViewModel?.Close();
            
            _audioButtonInteractable.Value = true;
            _graphicsButtonInteractable.Value = true;
            _controlsButtonInteractable.Value = true;
            _gameButtonInteractable.Value = true;
        }
        bool OpenAny()
        {
            if (_audioViewModel?.IsOpened ?? false) return true;
            if (_graphicsViewModel?.IsOpened ?? false) return true;
            if (_controlsViewModel?.IsOpened ?? false) return true;
            if (_gameViewModel?.IsOpened ?? false) return true;
            return false;
        }
        bool HasAny()
        {
            if (_audioViewModel != null) return true;
            if (_graphicsViewModel != null) return true;
            if (_controlsViewModel != null) return true;
            if (_gameViewModel != null) return true;
            return false;
        }
    }

    [Serializable]
    public class Settings : ViewModelTemplate<SettingsViewModel>
    {
        public LocalizedString Title;
        public LocalizedString CancelName;
        
        public bool Audio = true;
        public bool Graphics = true;
        public bool Controls = true;
        public bool Game = true;
        
        public LocalizedString AudioName;
        public LocalizedString GraphicsName;
        public LocalizedString ControlsName;
        public LocalizedString GameName;
        
        [SerializeReference] [SubclassSelector] public IViewModelTemplate AudioSettingsWindow;
        [SerializeReference] [SubclassSelector] public IViewModelTemplate GraphicsSettingsWindow;
        [SerializeReference] [SubclassSelector] public IViewModelTemplate ControlsSettingsWindow;
        [SerializeReference] [SubclassSelector] public IViewModelTemplate GameSettingsWindow;
    }
}