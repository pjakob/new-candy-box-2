using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KawaiiCandyBox.UI
{
    /// <summary>
    /// The Config panel opened via the CFG tab in the menu bar.
    /// Provides language selection, music volume, and about text.
    ///
    /// Scene hierarchy expected:
    ///   ConfigPanel (this script, full screen overlay)
    ///     ├── Overlay (Image — dim background, Button for click-outside-to-close)
    ///     └── PanelContent (centred popup)
    ///           ├── CloseButton (Button + TMP Text "X")
    ///           ├── TitleLabel (TMP Text)
    ///           ├── LanguageSection
    ///           │     ├── LanguageLabel (TMP Text)
    ///           │     └── LanguageButtons
    ///           │           ├── EnglishButton (Button + TMP Text)
    ///           │           ├── FrenchButton  (Button + TMP Text)
    ///           │           └── GermanButton  (Button + TMP Text)
    ///           ├── MusicSection
    ///           │     ├── MusicLabel (TMP Text)
    ///           │     └── MusicSlider (Slider)
    ///           └── AboutSection
    ///                 ├── AboutTitleLabel (TMP Text)
    ///                 └── AboutText (TMP Text)
    /// </summary>
    public class ConfigPanel : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI _titleLabel;
        [SerializeField] private TextMeshProUGUI _languageLabel;
        [SerializeField] private TextMeshProUGUI _musicLabel;
        [SerializeField] private TextMeshProUGUI _aboutTitleLabel;
        [SerializeField] private TextMeshProUGUI _aboutText;

        [Header("Language Buttons")]
        [SerializeField] private Button _englishButton;
        [SerializeField] private Button _frenchButton;
        [SerializeField] private Button _germanButton;


        [Header("Music")]
        [SerializeField] private Slider _musicSlider;

        // Colours for selected/unselected language buttons
        private static readonly Color SelectedColour =
            new Color(0.2f, 0.6f, 1f, 1f);   // bright blue
        private static readonly Color UnselectedColour =
            new Color(1f, 1f, 1f, 1f);         // white

        private void OnEnable()
        {
            SetLabels();
            RefreshLanguageButtons();
            RefreshMusicSlider();
        }

        private void SetLabels()
        {
            var loc = Localisation.LocalizationManager.Instance;
            if (loc == null) return;

            SetLabel(_titleLabel,      "ui.config.title");
            SetLabel(_languageLabel,   "ui.config.language");
            SetLabel(_musicLabel,      "ui.config.music");
            SetLabel(_aboutTitleLabel, "ui.config.about.title");
            SetLabel(_aboutText,       "ui.config.about.text");
        }

        private void SetLabel(TextMeshProUGUI label, string key)
        {
            if (label != null)
                label.text = Localisation.LocalizationManager.Instance.Get(key);
        }

        // ── Language ─────────────────────────────────────────────────

        private void RefreshLanguageButtons()
        {
            string current = Core.SaveManager.Instance.Data.languageCode;
            if (string.IsNullOrEmpty(current)) current = "en";

            SetButtonSelected(_englishButton, current == "en");
            SetButtonSelected(_frenchButton,  current == "fr");
            SetButtonSelected(_germanButton,  current == "de");
        }

        private void SetButtonSelected(Button button, bool selected)
        {
            if (button == null) return;
            ColorBlock colors = button.colors;
            colors.normalColor = selected ? SelectedColour : UnselectedColour;
            button.colors = colors;
        }

        public void OnEnglishPressed()
        {
            SetLanguage("en");
        }

        public void OnFrenchPressed()
        {
            SetLanguage("fr");
        }

        public void OnGermanPressed()
        {
            SetLanguage("de");
        }

        private void SetLanguage(string code)
        {
            Localisation.LocalizationManager.Instance.SetLanguage(code);
            Core.SaveManager.Instance.Data.languageCode = code;
            Core.SaveManager.Instance.SaveGame();
            RefreshLanguageButtons();

            // Refresh all labels in this panel to the new language
            SetLabels();

            Debug.Log($"[ConfigPanel] Language set to {code}.");
        }

        // ── Music ─────────────────────────────────────────────────────

        private void RefreshMusicSlider()
        {
            if (_musicSlider == null) return;

            _musicSlider.minValue = 0f;
            _musicSlider.maxValue = 1f;
            _musicSlider.value = Core.SaveManager.Instance.Data.musicVolume;

            // Subscribe to slider changes
            _musicSlider.onValueChanged.RemoveAllListeners();
            _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        private void OnMusicVolumeChanged(float value)
        {
            Core.SaveManager.Instance.Data.musicVolume = value;
            Core.AudioManager.Instance.SetMusicVolume(value);
            Debug.Log($"[ConfigPanel] Music volume set to {value:F2}.");
        }

        private void OnDisable()
        {
            // Save music volume when panel closes
            if (_musicSlider != null)
                Core.SaveManager.Instance.SaveGame();
        }
    }
}