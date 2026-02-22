using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KawaiiCandyBox.UI
{
    /// <summary>
    /// The save panel — opened via the SAVE tab.
    /// Allows manual save and game reset.
    /// Note: autosave always runs regardless of this panel.
    /// </summary>
    public class SavePanel : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _saveButton;
        [SerializeField] private TextMeshProUGUI _saveButtonLabel;

        [SerializeField] private Button _resetButton;
        [SerializeField] private TextMeshProUGUI _resetButtonLabel;

        [Header("Confirmation")]
        [SerializeField] private GameObject _resetConfirmationGroup;
        [SerializeField] private TextMeshProUGUI _resetConfirmationLabel;
        [SerializeField] private Button _confirmResetButton;
        [SerializeField] private Button _cancelResetButton;

        [Header("Close")]
        [SerializeField] private Button _closeButton;

        private void OnEnable()
        {
            RefreshLabels();

            // Always hide confirmation on open
            if (_resetConfirmationGroup != null)
                _resetConfirmationGroup.SetActive(false);
        }

        private void RefreshLabels()
        {
            var loc = Localisation.LocalizationManager.Instance;
            if (_saveButtonLabel != null)
                _saveButtonLabel.text = loc.Get("ui.save.save_button");
            if (_resetButtonLabel != null)
                _resetButtonLabel.text = loc.Get("ui.save.reset_button");
            if (_resetConfirmationLabel != null)
                _resetConfirmationLabel.text = loc.Get("ui.save.reset_confirm");
        }

        public void OnSavePressed()
        {
            Core.SaveManager.Instance.SaveGame();
            Debug.Log("[SavePanel] Manual save triggered.");
        }

        public void OnResetPressed()
        {
            // Show confirmation before doing anything destructive
            if (_resetConfirmationGroup != null)
                _resetConfirmationGroup.SetActive(true);
        }

        public void OnConfirmResetPressed()
        {
            Core.SaveManager.Instance.DeleteSave();

            // Reload the game from scratch
            UnityEngine.SceneManagement.SceneManager.LoadScene("Bootstrap");
        }

        public void OnCancelResetPressed()
        {
            if (_resetConfirmationGroup != null)
                _resetConfirmationGroup.SetActive(false);
        }

        public void OnClosePressed()
        {
            MenuBarController.Instance.CloseActivePanel();
        }
    }
}
