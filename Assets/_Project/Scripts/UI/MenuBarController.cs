using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace KawaiiCandyBox.UI
{
    /// <summary>
    /// Manages the persistent menu bar.
    ///
    /// Tab unlock order:
    ///   Request 1 → menu bar appears (Home tab only)
    ///   Request 2 → Options tab
    ///   Request 3 → 1 lollipop granted, lollipop counter appears
    ///   Request 4 → health bar (handled by HealthBarController)
    ///   Request 5 → Map tab
    ///
    /// Menu bar is forced open on the home screen.
    /// On other scenes it can be toggled via the ToggleButton.
    /// Toggling uses push behaviour — ContentArea shifts down/up.
    /// </summary>
    public class MenuBarController : MonoBehaviour
    {
        [Header("Menu Bar")]
        [SerializeField] private RectTransform _menuBar;
        [SerializeField] private RectTransform _contentArea;
        [SerializeField] private GameObject _toggleButton;

        [Header("Tab Buttons")]
        [SerializeField] private Button _homeTabButton;
        [SerializeField] private TextMeshProUGUI _homeTabLabel;

        [SerializeField] private Button _mapTabButton;
        [SerializeField] private TextMeshProUGUI _mapTabLabel;

        [SerializeField] private Button _inventoryTabButton;
        [SerializeField] private TextMeshProUGUI _inventoryTabLabel;

        [SerializeField] private Button _lollipopFarmTabButton;
        [SerializeField] private TextMeshProUGUI _lollipopFarmTabLabel;

        [SerializeField] private Button _cauldronTabButton;
        [SerializeField] private TextMeshProUGUI _cauldronTabLabel;

        [SerializeField] private Button _optionsTabButton;
        [SerializeField] private TextMeshProUGUI _optionsTabLabel;

        [SerializeField] private Button _saveTabButton;
        [SerializeField] private TextMeshProUGUI _saveTabLabel;

        [Header("Panels")]
        [SerializeField] private GameObject _configPanel;
        [SerializeField] private GameObject _savePanel;

        [Header("Layout")]
        [SerializeField] private float _menuBarHeight = 70f;
        [SerializeField] private float _healthBarHeight = 40f;

        [Header("Scene Names")]
        [SerializeField] private string _mapSceneName = "WorldMap";
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        public static MenuBarController Instance { get; private set; }

        private GameObject _activePanel = null;
        private bool _isOnMainMenu = true;
        private bool _menuBarVisible = false;
        private bool _menuBarUnlocked = false;

        // ── Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            Core.GameManager.OnDeveloperRequestGranted += OnRequestGranted;
            Core.GameManager.OnGameReady += OnGameReady;
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Start hidden
            SetMenuBarVisible(false, animate: false);

            Debug.Log("[MenuBarController] Initialised.");
        }

        private void OnDestroy()
        {
            Instance = null;
            Core.GameManager.OnDeveloperRequestGranted -= OnRequestGranted;
            Core.GameManager.OnGameReady -= OnGameReady;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // ── Event handlers ───────────────────────────────────────────

        private void OnGameReady()
        {
            SetTabLabels();
            RefreshTabVisibility();

            if (Core.GameManager.Instance.HasMenuBar)
            {
                _menuBarUnlocked = true;
                SetMenuBarVisible(true, animate: false);
            }
        }

        private void OnRequestGranted(int newRequestCount)
        {
            Debug.Log($"[MenuBarController] Request granted: {newRequestCount}");

            if (newRequestCount == 1)
            {
                _menuBarUnlocked = true;
                SetMenuBarVisible(true, animate: false);
                Debug.Log("[MenuBarController] Menu bar revealed.");
            }

            RefreshTabVisibility();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _isOnMainMenu = scene.name == _mainMenuSceneName;
            CloseActivePanel();

            if (!_menuBarUnlocked) return;

            if (_isOnMainMenu)
            {
                // Force open on home screen, hide toggle button
                SetMenuBarVisible(true, animate: false);
                if (_toggleButton != null)
                    _toggleButton.SetActive(false);
            }
            else
            {
                // On other scenes show toggle button, menu bar
                // stays in whatever state it was
                if (_toggleButton != null)
                    _toggleButton.SetActive(!_menuBarVisible);
            }
        }

        // ── Tab visibility ───────────────────────────────────────────

        private void RefreshTabVisibility()
        {
            int requestCount = Core.GameManager.Instance.DeveloperRequestCount;

            SetTabVisible(_homeTabButton,         true);
            SetTabVisible(_optionsTabButton,      requestCount >= 2);
            SetTabVisible(_mapTabButton,          requestCount >= 5);

            // These tabs will be unlocked through gameplay later
            SetTabVisible(_inventoryTabButton,    false);
            SetTabVisible(_lollipopFarmTabButton, false);
            SetTabVisible(_cauldronTabButton,     false);
            SetTabVisible(_saveTabButton,         true); // always visible once bar unlocked
        }

        private void SetTabVisible(Button tab, bool visible)
        {
            if (tab != null)
                tab.gameObject.SetActive(visible);
        }

        private void SetTabLabels()
        {
            SetLabel(_homeTabLabel,         "ui.menu.tab.home");
            SetLabel(_mapTabLabel,          "ui.menu.tab.map");
            SetLabel(_inventoryTabLabel,    "ui.menu.tab.inventory");
            SetLabel(_lollipopFarmTabLabel, "ui.menu.tab.lollipop_farm");
            SetLabel(_cauldronTabLabel,     "ui.menu.tab.cauldron");
            SetLabel(_optionsTabLabel,      "ui.menu.tab.config");
            SetLabel(_saveTabLabel,         "ui.menu.tab.save");
        }

        private void SetLabel(TextMeshProUGUI label, string key)
        {
            if (label != null)
                label.text = Localisation.LocalizationManager.Instance.Get(key);
        }

        // ── Menu bar toggle ──────────────────────────────────────────

        public void OnToggleButtonPressed()
        {
            // Don't allow hiding on home screen
            if (_isOnMainMenu) return;

            SetMenuBarVisible(!_menuBarVisible, animate: true);
        }

        private void SetMenuBarVisible(bool visible, bool animate)
        {
            _menuBarVisible = visible;

            if (_menuBar != null)
                _menuBar.gameObject.SetActive(visible);

            // Push ContentArea down when menu bar visible,
            // up when hidden
            if (_contentArea != null)
            {
                float topOffset = _healthBarHeight +
                                  (visible ? _menuBarHeight : 0f);
                _contentArea.offsetMax = new Vector2(
                    _contentArea.offsetMax.x, -topOffset);
            }

            // Toggle button is visible only when menu bar is hidden
            // and we're not on the home screen
            if (_toggleButton != null)
                _toggleButton.SetActive(!visible && !_isOnMainMenu);

            // TODO: replace SetActive with animation when
            // MenuBarAnimator is added in a later phase
        }

        // ── Tab button handlers ──────────────────────────────────────

        public void OnHomeTabPressed()
        {
            CloseActivePanel();
            if (!_isOnMainMenu)
                SceneManager.LoadScene(_mainMenuSceneName);
        }

        public void OnMapTabPressed()
        {
            CloseActivePanel();
            SceneManager.LoadScene(_mapSceneName);
        }

        public void OnInventoryTabPressed()
        {
            CloseActivePanel();
            // TODO: load Inventory scene
        }

        public void OnLollipopFarmTabPressed()
        {
            CloseActivePanel();
            // TODO: load LollipopFarm scene
        }

        public void OnCauldronTabPressed()
        {
            CloseActivePanel();
            // TODO: load Cauldron scene
        }

        public void OnOptionsTabPressed()
        {
            TogglePanel(_configPanel);
        }

        public void OnSaveTabPressed()
        {
            TogglePanel(_savePanel);
        }

        // ── Panel management ─────────────────────────────────────────

        private void TogglePanel(GameObject panel)
        {
            if (panel == null) return;

            if (_activePanel == panel)
            {
                CloseActivePanel();
                return;
            }

            CloseActivePanel();
            _activePanel = panel;
            _activePanel.SetActive(true);
        }

        public void CloseActivePanel()
        {
            if (_activePanel != null)
            {
                _activePanel.SetActive(false);
                _activePanel = null;
            }
        }
    }
}