using SackranyUI.Core.Base;
using SackranyUI.Core.Components;
using SackranyUI.Core.Entities;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SackranyUI.Default.MainMenu
{
    public class MainMenuView : View
    {
        [OutputBind("continue_button_enabled")] GameObject _continueButtonGameObject;
        [OutputBind("newgame_button_enabled")] GameObject _newGameButtonGameObject;
        [OutputBind("settings_button_enabled")] GameObject _settingsButtonGameObject;
        [OutputBind("info_button_enabled")] GameObject _infoButtonGameObject;
        [OutputBind("exit_button_enabled")] GameObject _exitButtonGameObject;
        
        [InputBind("continue_button")] public Button ContinueButton;
        [InputBind("newgame_button")] public Button NewGameButton;
        [InputBind("settings_button")] public Button SettingsButton;
        [InputBind("info_button")] public Button InfoButton;
        [InputBind("exit_button")] public Button ExitButton;
        
        [OutputBind("continue_text")] TMP_Text _continueText;
        [OutputBind("newgame_text")] TMP_Text _newGameText;
        [OutputBind("settings_text")] TMP_Text _settingsText;
        [OutputBind("info_text")] TMP_Text _infoText;
        [OutputBind("exit_text")] TMP_Text _exitText;
        
        [CollectionBind("saveGameList")] public CollectionAnchor SaveGameList; 
        
        protected override void OnBeforeBinding()
        {
            _continueButtonGameObject = ContinueButton.gameObject;
            _newGameButtonGameObject = NewGameButton.gameObject;
            _settingsButtonGameObject = SettingsButton.gameObject;
            _infoButtonGameObject = InfoButton.gameObject;
            _exitButtonGameObject = ExitButton.gameObject;
            
            _continueText = ContinueButton.GetComponentInChildren<TMP_Text>();
            _newGameText = NewGameButton.GetComponentInChildren<TMP_Text>();
            _settingsText = SettingsButton.GetComponentInChildren<TMP_Text>();
            _infoText = InfoButton.GetComponentInChildren<TMP_Text>();
            _exitText = ExitButton.GetComponentInChildren<TMP_Text>();
        }
    }
}