using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KawaiiCandyBox.UI
{
    /// <summary>
    /// The configuration panel — opened via the CFG tab.
    /// Contains language selection and sound toggle.
    /// More options can be added here in later phases.
    /// </summary>
    public class ConfigPanel : MonoBehaviour
    {
        [Header("Sound")]
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private TextMeshProUGUI _soundToggleLabel;

        [Header("Language")]
        [SerializeField] private TextMeshProUGUI _languageLabel;
        [SerializeField] private Button _languageEnglishButton;
        [SerializeField] private Button _languageFrenchButton;
        [SerializeField] private Button _languageGermanButton;

        [Header("Close")]
        [SerializeField] private Button _closeButton;

        private void OnEnable()
        {
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            var loc = Localisation.LocalizationManager.Instance;
            if (_soundToggleLabel != null)
                _soundToggleLabel.text = loc.Get("ui.config.sound");
            if (_languageLabel != null)
                _languageLabel.text = loc.Get("ui.config.language");
        }

        public void OnSoundToggleChanged(bool isOn)
        {
            // TODO: Wire to AudioManager when audio system is built
            Debug.Log($"[ConfigPanel] Sound: {(isOn ? "on" : "off")}");
            Core.SaveManager.Instance.Data.soundEnabled = isOn;
            Core.SaveManager.Instance.SaveGame();
        }

        public void OnLanguageEnglishPressed()
            => SetLanguage("en");

        public void OnLanguageFrenchPressed()
            => SetLanguage("fr");

        public void OnLanguageGermanPressed()
            => SetLanguage("de");

        private void SetLanguage(string code)
        {
            Localisation.LocalizationManager.Instance.SetLanguage(code);
            RefreshLabels();
        }

        public void OnClosePressed()
        {
            MenuBarController.Instance.CloseActivePanel();
        }
    }
}
