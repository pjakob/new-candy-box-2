using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace KawaiiCandyBox.UI
{
    /// <summary>
    /// Manages the persistent menu bar that appears after developer request 1.
    /// Survives scene loads via PersistentUICanvas on the parent Canvas.
    ///
    /// Tab unlock order:
    ///   Request 1 → bar appears (Home only, visual)
    ///   Request 2 → CFG tab
    ///   Request 3 → SAVE tab
    ///   Request 4 → (health bar, handled by HealthBarController)
    ///   Request 5 → MAP tab
    /// </summary>
    public class MenuBarController : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private GameObject _tabContainer;


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

        // Singleton access — not using SingletonManager since this
        // lives inside PersistentUI canvas and can't call
        // DontDestroyOnLoad itself (parent handles that)
        public static MenuBarController Instance { get; private set; }

        private GameObject _activePanel = null;
        private bool _isOnMainMenu = true;

        // ── Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Subscribe here so we receive events even when
            // child objects are inactive
            Core.GameManager.OnDeveloperRequestGranted += OnRequestGranted;
            Core.GameManager.OnGameReady += OnGameReady;
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Hide visual content until request 1 is granted
            HideVisualContent();

            Debug.Log("[MenuBarController] Initialised.");
        }

        private void OnDestroy()
        {
            Instance = null;
            Core.GameManager.OnDeveloperRequestGranted -= OnRequestGranted;
            Core.GameManager.OnGameReady -= OnGameReady;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void HideVisualContent()
        {
            if (_tabContainer != null)
                _tabContainer.SetActive(false);
            //if (_panelContainer != null)
            //    _panelContainer.SetActive(false);
        }

        // ── Event handlers ───────────────────────────────────────────

        private void OnGameReady()
        {
            SetTabLabels();
            RefreshTabVisibility();

            if (Core.GameManager.Instance.HasMenuBar)
            {
                if (_tabContainer != null) _tabContainer.SetActive(true);
                //if (_panelContainer != null) _panelContainer.SetActive(true);
            }
        }

        private void OnRequestGranted(int newRequestCount)
        {
            Debug.Log($"[MenuBarController] Request granted: {newRequestCount}");

            if (newRequestCount == 1)
            {
                if (_tabContainer != null) _tabContainer.SetActive(true);
                //if (_panelContainer != null) _panelContainer.SetActive(true);
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

            SetTabVisible(_homeTabButton, true);
            SetTabVisible(_configTabButton, requestCount >= 2);
            SetTabVisible(_saveTabButton,   requestCount >= 3);
            SetTabVisible(_mapTabButton,    requestCount >= 5);
        }

        private void SetTabVisible(Button tab, bool visible)
        {
            if (tab != null)
                tab.gameObject.SetActive(visible);
        }

        private void SetTabLabels()
        {
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

        public void OnHomeTabPressed()
        {
            CloseActivePanel();
            if (!_isOnMainMenu)
                SceneManager.LoadScene(_mainMenuSceneName);
        }

        public void OnConfigTabPressed()
        {
            TogglePanel(_configPanel);
        }

        public void OnSaveTabPressed()
        {
            TogglePanel(_savePanel);
        }

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