using System;

using R3;

using Sackrany.Scenes;
using Sackrany.SerializableData;

using SackranyUI.Core.Base;
using SackranyUI.Core.Entities;
using SackranyUI.Core.Events;
using SackranyUI.Core.Static;

using UnityEngine.Localization;

namespace SackranyUI.Default.MainMenu
{
    public class MainMenuViewModel : ViewModel<MainMenu>
    {
        [Bind("continue_button_enabled")] ReactiveProperty<bool> _continueButtonEnabled = new (true);
        [Bind("newgame_button_enabled")] ReactiveProperty<bool> _newGameButtonEnabled = new (true);
        [Bind("settings_button_enabled")] ReactiveProperty<bool> _settingsButtonEnabled = new (true);
        [Bind("info_button_enabled")] ReactiveProperty<bool> _infoButtonEnabled = new (true);
        [Bind("exit_button_enabled")] ReactiveProperty<bool> _exitButtonEnabled = new (true);
        
        [Bind("continue_text")] ReactiveProperty<string> _continueText = new ("Continue");
        [Bind("newgame_text")] ReactiveProperty<string> _newGameText = new ("New Game");
        [Bind("settings_text")] ReactiveProperty<string> _settingsText = new ("Settings");
        [Bind("info_text")] ReactiveProperty<string> _infoText = new ("Information");
        [Bind("exit_text")] ReactiveProperty<string> _exitText = new ("Exit"); 
        
        [Bind("saveGameList")] ReactiveList<SaveSlotViewModel> _saveGameList = new();
        
        CompositeDisposable _localizations;
        
        #region Buttons
        [Bind("continue_button")]
        void ContinueButton()
        {
            Publish<UIEvents.CloseAllWindows>();
            Publish<UIEvents.ContinueWindowCall, bool>(true);
        }
        
        [Bind("newgame_button")]
        void NewGameButton()
        {
            Publish<UIEvents.NewGameWindowCall, bool>(true);
            Publish<UIEvents.CloseAllWindows>();
            if (!Template.OnlyOneSlot)
                DataManager.Slots.Create();
            LoadGame();
        }

        [Bind("settings_button")]
        void SettingsButton()
        {
            Publish<UIEvents.CloseAllWindows>();
            Publish<UIEvents.SettingsWindowCall, bool>(true);
        }

        [Bind("info_button")]
        void InfoButton()
        {
            Publish<UIEvents.CloseAllWindows>();
            Publish<UIEvents.InfoWindowCall, bool>(true);
        }

        [Bind("exit_button")]
        void ExitButton()
        {
            Publish<UIEvents.ExitWindowCall, bool>(true);
        }
        #endregion
        
        protected override void OnInitialized()
        {
            DataManager.PauseSession();
            
            _continueButtonEnabled.Value = Template.ShowContinueButton && !Template.OnlyOneSlot;
            _newGameButtonEnabled.Value = Template.ShowNewGameButton;
            _settingsButtonEnabled.Value = Template.ShowSettingsButton;
            _infoButtonEnabled.Value = Template.ShowInfoButton;
            _exitButtonEnabled.Value = Template.ShowExitButton;

            _localizations = CompositeDisposableHelper.Create(
                Template.ContinueText?.Subscribe(t => _continueText.Value = t, "Continue"),
                Template.NewGameText?.Subscribe(t => _newGameText.Value = t, "New Game"),
                Template.SettingsText?.Subscribe(t => _settingsText.Value = t, "Settings"),
                Template.InfoText?.Subscribe(t => _infoText.Value = t, "Info"),
                Template.ExitText?.Subscribe(t => _exitText.Value = t, "Exit")
            );

            foreach (var slots in DataManager.Slots.GetAllMeta())
            {
                var svm = new SaveSlotViewModel();
                svm.OnContinueGame += SvmOnOnContinueGame;
                svm.SetSlotNumber(slots.Key);
                _saveGameList.Add(svm);
                svm.SetSlot(slots.Value, Template.SaveSlotCreationDateText, Template.SaveSlotTimeSpentText, Template.SaveSlotNumberText);
                svm.SetSlotActive(DataManager.Slots.Current == slots.Key);
            }
            
            Open();
        }
        void SvmOnOnContinueGame(int number)
        {
            DataManager.Slots.Switch(number);
            LoadGame();
        }

        void LoadGame()
        {
            DataManager.StartSession();
            SceneLoader.Load(Template.GameScene);
        }
        protected override void OnDispose()
        {
            _localizations?.Dispose();
            
            _continueButtonEnabled.Dispose();
            _newGameButtonEnabled.Dispose();
            _settingsButtonEnabled.Dispose();
            _infoButtonEnabled.Dispose();
            _exitButtonEnabled.Dispose();
            
            _continueText.Dispose();
            _newGameText.Dispose();
            _settingsText.Dispose();
            _infoText.Dispose();
            _exitText.Dispose();
            
            _saveGameList.Dispose();
        }
    }

    [Serializable]
    public class MainMenu : ViewModelTemplate<MainMenuViewModel>
    {
        public string GameScene = "Game";
        public bool OnlyOneSlot = false;
        
        public bool ShowContinueButton = true;
        public bool ShowNewGameButton = true;
        public bool ShowSettingsButton = true;
        public bool ShowInfoButton = true;
        public bool ShowExitButton = true;
        
        public LocalizedString ContinueText;
        public LocalizedString NewGameText;
        public LocalizedString SettingsText;
        public LocalizedString InfoText;
        public LocalizedString ExitText;
        
        public LocalizedString SaveSlotNumberText;
        public LocalizedString SaveSlotCreationDateText;
        public LocalizedString SaveSlotTimeSpentText;
    }
}