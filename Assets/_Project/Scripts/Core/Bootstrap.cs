using UnityEngine;
using UnityEngine.SceneManagement;

namespace KawaiiCandyBox.Core
{
    /// <summary>
    /// Entry point for the entire game. Lives in the Bootstrap scene
    /// (scene index 0 in Build Settings) and runs exactly once per
    /// game session.
    ///
    /// Responsibilities:
    ///   1. Create all singleton service GameObjects in the correct order
    ///   2. Tell GameManager when everything is ready
    ///   3. Load the MainMenu scene
    ///
    /// Scene setup:
    ///   Create an empty GameObject in the Bootstrap scene called
    ///   "Bootstrap" and attach this script to it.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [Header("Scene To Load After Bootstrap")]
        [SerializeField] private string _nextSceneName = "MainMenu";

        private void Awake()
        {
            Debug.Log("[Bootstrap] Starting initialisation...");
            CreateServices();
        }

        private void Start()
        {
            // Start() runs after all Awake() calls, so all services
            // are fully initialised before we tell GameManager
            GameManager.Instance.OnAllServicesReady();

            Debug.Log("[Bootstrap] All services ready. Loading " +
                      $"{_nextSceneName}...");

            SceneManager.LoadScene(_nextSceneName);
        }

        /// <summary>
        /// Creates all service GameObjects in dependency order:
        ///   1. SaveManager   — needed by everything
        ///   2. ContentRegistry — needed by game systems
        ///   3. LocalizationManager — needed by all UI
        ///   4. GameManager   — coordinates everything above
        ///
        /// Each service uses DontDestroyOnLoad so it persists
        /// after the Bootstrap scene is replaced by MainMenu.
        /// </summary>
        private void CreateServices()
        {
            CreateService<SaveManager>("SaveManager");
            CreateService<Content.ContentRegistry>("ContentRegistry");
            CreateService<Localisation.LocalizationManager>("LocalizationManager");
            CreateService<Economy.ResourceManager>("ResourceManager");
            // CreateService<UI.MenuBarController>("MenuBarController");  // ← add this
            CreateService<GameManager>("GameManager");
        }

        /// <summary>
        /// Creates a new GameObject with the given name and attaches
        /// the specified manager component to it.
        /// </summary>
        private T CreateService<T>(string serviceName) where T : MonoBehaviour
        {
            GameObject go = new GameObject(serviceName);
            T service = go.AddComponent<T>();
            Debug.Log($"[Bootstrap] Created service: {serviceName}");
            return service;
        }
    }
}