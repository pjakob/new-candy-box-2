using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace KawaiiCandyBox.UI
{
    /// <summary>
    /// Manages the persistent menu bar that appears after developer request 1.
    /// Survives scene loads via DontDestroyOnLoad.
    ///
    /// Two display modes:
    ///   FullBar  — used on MainMenu, bar is always expanded
    ///   TabBar   — used on all other scenes, tapping a tab
    ///              slides open a panel drawer from the top
    ///
    /// Tab unlock order:
    ///   Request 1 → bar appears (Home only, visual)
    ///   Request 2 → CFG tab
    ///   Request 3 → SAVE tab
    ///   Request 4 → (health bar, handled by HealthBarController)
    ///   Request 5 → MAP tab
    ///
    /// Scene hierarchy expected:
    ///   MenuBar (this script)
    ///     ├── TabContainer         (horizontal layout group)
    ///     │     ├── HomeTab        (Button + TextMeshProUGUI)
    ///     │     ├── ConfigTab      (Button + TextMeshProUGUI)
    ///     │     ├── SaveTab        (Button + TextMeshProUGUI)
    ///     │     └── MapTab         (Button + TextMeshProUGUI)
    ///     └── PanelContainer       (holds Config and Save panels)
    ///           ├── ConfigPanel    (GameObject, inactive by default)
    ///           └── SavePanel      (GameObject, inactive by default)
    /// </summary>
    public class MenuBarController : Core.SingletonManager<MenuBarController>
    {
        [Header("Tab Buttons")]
        [SerializeField] private Button _homeTabButton;
        [SerializeField] private TextMeshProUGUI _homeTabLabel;

        [SerializeField] private Button _configTabButton;
        [SerializeField] private TextMeshProUGUI _configTabLabel;

        [SerializeField] private Button _saveTabButton;
        [SerializeField] private TextMeshProUGUI _saveTabLabel;

        [SerializeField] private Button _mapTabButton;
        [SerializeField] private TextMeshProUGUI _mapTabLabel;

        [Header("Panels")]
        [SerializeField] private GameObject _configPanel;
        [SerializeField] private GameObject _savePanel;

        [Header("Scene Names")]
        [SerializeField] private string _mapSceneName = "WorldMap";
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        [Header("Containers")]
[SerializeField] private GameObject _tabContainer;
[SerializeField] private GameObject _panelContainer;

        // Which panel is currently open (null = none)
        private GameObject _activePanel = null;

        // Whether we're on the main menu or another scene
        private bool _isOnMainMenu = true;

        protected override void OnInitialise()
{
    Core.GameManager.OnDeveloperRequestGranted += OnRequestGranted;
    Core.GameManager.OnGameReady += OnGameReady;
    SceneManager.sceneLoaded += OnSceneLoaded;
    Debug.Log("[MenuBarController] Initialised.");
    // Do NOT call gameObject.SetActive(false) here
    // Instead hide just the visual content, not the root GameObject
    HideVisualContent();
}

private void HideVisualContent()
{
    // Hide the tab container and panel container
    // but keep the root MenuBar active so Awake runs
    if (_tabContainer != null)
        _tabContainer.SetActive(false);
    if (_panelContainer != null)
        _panelContainer.SetActive(false);
}

        private void OnEnable()
        {
            Core.GameManager.OnDeveloperRequestGranted += OnRequestGranted;
            Core.GameManager.OnGameReady += OnGameReady;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            Core.GameManager.OnDeveloperRequestGranted -= OnRequestGranted;
            Core.GameManager.OnGameReady -= OnGameReady;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // ── Initialisation ───────────────────────────────────────────

        private void OnGameReady()
{
    SetTabLabels();
    RefreshTabVisibility();

    if (Core.GameManager.Instance.HasMenuBar)
    {
        if (_tabContainer != null) _tabContainer.SetActive(true);
        if (_panelContainer != null) _panelContainer.SetActive(true);
    }
}

        private void OnRequestGranted(int newRequestCount)
{
    Debug.Log($"[MenuBarController] OnRequestGranted called with {newRequestCount}");
    if (newRequestCount == 1)
    {
        if (_tabContainer != null) _tabContainer.SetActive(true);
        if (_panelContainer != null) _panelContainer.SetActive(true);
        Debug.Log("[MenuBarController] Menu bar revealed.");
    }
    RefreshTabVisibility();
}

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _isOnMainMenu = scene.name == _mainMenuSceneName;
            CloseActivePanel();
        }

        // ── Tab visibility ───────────────────────────────────────────

        private void RefreshTabVisibility()
        {
            int requestCount = Core.GameManager.Instance.DeveloperRequestCount;

            // Home is always visible once bar exists
            SetTabVisible(_homeTabButton, true);

            // Config — request 2
            SetTabVisible(_configTabButton,
                requestCount >= 2);

            // Save — request 3
            SetTabVisible(_saveTabButton,
                requestCount >= 3);

            // Map — request 5
            SetTabVisible(_mapTabButton,
                requestCount >= 5);
        }

        private void SetTabVisible(Button tab, bool visible)
        {
            if (tab != null)
                tab.gameObject.SetActive(visible);
        }

        private void SetTabLabels()
        {
            var loc = Localisation.LocalizationManager.Instance;

            SetLabel(_homeTabLabel,   "ui.menu.tab.home");
            SetLabel(_configTabLabel, "ui.menu.tab.config");
            SetLabel(_saveTabLabel,   "ui.menu.tab.save");
            SetLabel(_mapTabLabel,    "ui.menu.tab.map");
        }

        private void SetLabel(TextMeshProUGUI label, string key)
        {
            if (label != null)
                label.text = Localisation.LocalizationManager.Instance.Get(key);
        }

        // ── Tab button handlers ──────────────────────────────────────

        /// <summary>
        /// Called by Home tab button OnClick.
        /// </summary>
        public void OnHomeTabPressed()
        {
            CloseActivePanel();

            if (!_isOnMainMenu)
                SceneManager.LoadScene(_mainMenuSceneName);
        }

        /// <summary>
        /// Called by Config tab button OnClick.
        /// </summary>
        public void OnConfigTabPressed()
        {
            TogglePanel(_configPanel);
        }

        /// <summary>
        /// Called by Save tab button OnClick.
        /// </summary>
        public void OnSaveTabPressed()
        {
            TogglePanel(_savePanel);
        }

        /// <summary>
        /// Called by Map tab button OnClick.
        /// </summary>
        public void OnMapTabPressed()
        {
            CloseActivePanel();
            SceneManager.LoadScene(_mapSceneName);
        }

        // ── Panel management ─────────────────────────────────────────

        private void TogglePanel(GameObject panel)
        {
            if (panel == null) return;

            if (_activePanel == panel)
            {
                // Tapping the same tab again closes the panel
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
