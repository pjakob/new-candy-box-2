using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace KawaiiCandyBox.UI
{
    /// <summary>
    /// The Save panel opened via the SAVE tab in the menu bar.
    /// Provides game reset with an inline confirmation step.
    ///
    /// Scene hierarchy expected:
    ///   SavePanel (this script, inside PanelContainer)
    ///     ├── ResetButton        (Button + TMP Text)
    ///     └── ConfirmGroup       (GameObject, inactive by default)
    ///           ├── ConfirmLabel (TMP Text)
    ///           ├── ConfirmButton (Button + TMP Text)
    ///           └── CancelButton  (Button + TMP Text)
    /// </summary>
    public class SavePanel : MonoBehaviour
    {
        [Header("Reset Button")]
        [SerializeField] private Button _resetButton;
        [SerializeField] private TextMeshProUGUI _resetButtonText;

        [Header("Confirmation Group")]
        [SerializeField] private GameObject _confirmGroup;
        [SerializeField] private TextMeshProUGUI _confirmLabel;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private TextMeshProUGUI _confirmButtonText;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private TextMeshProUGUI _cancelButtonText;

        private void OnEnable()
        {
            // Always start in default state when panel opens
            SetLabels();
            ShowDefaultState();
        }

        private void SetLabels()
        {
            var loc = Localisation.LocalizationManager.Instance;
            if (loc == null) return;

            SetLabel(_resetButtonText,   "ui.save.reset_button");
            SetLabel(_confirmLabel,      "ui.save.reset_confirm");
            SetLabel(_confirmButtonText, "ui.save.confirm_yes");
            SetLabel(_cancelButtonText,  "ui.save.confirm_no");
        }

        private void SetLabel(TextMeshProUGUI label, string key)
        {
            if (label != null)
                label.text = Localisation.LocalizationManager.Instance.Get(key);
        }

        // ── Button handlers ──────────────────────────────────────────

        public void OnResetPressed()
        {
            ShowConfirmState();
        }

        public void OnConfirmResetPressed()
{
    Debug.Log("[SavePanel] Game reset confirmed.");
    Core.SaveManager.Instance.DeleteSave();
    
    // Destroy all DontDestroyOnLoad objects so Bootstrap
    // starts completely fresh
    GameObject.Destroy(Core.SaveManager.Instance.gameObject);
    GameObject.Destroy(Core.GameManager.Instance.gameObject);
    GameObject.Destroy(Economy.ResourceManager.Instance.gameObject);
    GameObject.Destroy(Content.ContentRegistry.Instance.gameObject);
    GameObject.Destroy(Localisation.LocalizationManager.Instance.gameObject);
    
    // Destroy PersistentUI (contains MenuBar, HealthBar, panels etc.)
    GameObject.Destroy(transform.root.gameObject);
    
    SceneManager.LoadScene("Bootstrap");
}

        public void OnCancelPressed()
        {
            ShowDefaultState();
        }

        // ── State helpers ────────────────────────────────────────────

        private void ShowDefaultState()
        {
            if (_resetButton != null)
                _resetButton.gameObject.SetActive(true);
            if (_confirmGroup != null)
                _confirmGroup.SetActive(false);
        }

        private void ShowConfirmState()
        {
            if (_resetButton != null)
                _resetButton.gameObject.SetActive(false);
            if (_confirmGroup != null)
                _confirmGroup.SetActive(true);
        }
    }
}