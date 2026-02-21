using UnityEngine;

namespace KawaiiCandyBox.Core
{
    /// <summary>
    /// Generic singleton base class. Inherit from this to create a
    /// persistent, self-registering singleton manager.
    /// Usage: public class MyManager : SingletonManager<MyManager> { }
    /// </summary>
    public abstract class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _isQuitting = false;

        public static T Instance
        {
            get
            {
                if (_isQuitting)
                {
                    Debug.LogWarning($"[SingletonManager] Instance of {typeof(T)} requested " +
                                     "after application quit. Returning null.");
                    return null;
                }

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        Debug.LogError($"[SingletonManager] No instance of {typeof(T)} found " +
                                       "in the scene. Make sure it exists in the Bootstrap scene.");
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning($"[SingletonManager] Duplicate instance of {typeof(T)} " +
                                 "detected and destroyed.");
                Destroy(gameObject);
                return;
            }

            _instance = this as T;
            DontDestroyOnLoad(gameObject);
            OnInitialise();
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        /// <summary>
        /// Called once when the singleton is first created.
        /// Override this instead of Awake() in subclasses.
        /// </summary>
        protected virtual void OnInitialise() { }
    }

    /// <summary>
    /// Central game manager. Owns top-level game state and
    /// coordinates initialisation order of all other services.
    /// </summary>
    public class GameManager : SingletonManager<GameManager>
    {
        [Header("Developer Request Progression")]
        [SerializeField] private int _developerRequestCount = 0;

        // Events other systems listen to when progression state changes
        public static event System.Action<int> OnDeveloperRequestGranted;
        public static event System.Action OnGameReady;

        public int DeveloperRequestCount => _developerRequestCount;

        // Convenience properties so other systems can query
        // unlock state without knowing the magic numbers
        public bool HasMenuBar      => _developerRequestCount >= 1;
        public bool HasConfigMenu   => _developerRequestCount >= 2;
        public bool HasSaveButton   => _developerRequestCount >= 3;
        public bool HasHealthBar    => _developerRequestCount >= 4;
        public bool HasMap          => _developerRequestCount >= 5;

        protected override void OnInitialise()
        {
            Debug.Log("[GameManager] Initialised.");
        }

        /// <summary>
        /// Called by Bootstrap once all other services are ready.
        /// </summary>
        public void OnAllServicesReady()
{
    // 1. Load save data first — everything else depends on it
    SaveManager.Instance.LoadGame();

    // 2. Restore our local state from save data
    _developerRequestCount = SaveManager.Instance.Data.developerRequestCount;

    // 3. Load the correct language
    Localisation.LocalizationManager.Instance.LoadLanguage(
        SaveManager.Instance.Data.languageCode
    );

    // 4. Apply offline earnings now that save data is loaded
    Economy.ResourceManager.Instance.OnSaveLoaded();

    Debug.Log($"[GameManager] All services ready. " +
              $"Developer requests: {_developerRequestCount}");

    OnGameReady?.Invoke();
}

        /// <summary>
        /// Attempt to spend candies on a developer request.
        /// Returns true if the request was granted.
        /// </summary>
        public bool TryGrantDeveloperRequest(long cost)
{
    if (_developerRequestCount >= 5)
    {
        Debug.Log("[GameManager] All developer requests already granted.");
        return false;
    }

    // Restored now that ResourceManager exists
    if (!Economy.ResourceManager.Instance.TrySpendCandy(cost))
    {
        Debug.Log("[GameManager] Not enough candy for developer request.");
        return false;
    }

    _developerRequestCount++;
    SaveManager.Instance.Data.developerRequestCount = _developerRequestCount;
    SaveManager.Instance.SaveGame();

    Debug.Log($"[GameManager] Developer request {_developerRequestCount} granted.");
    OnDeveloperRequestGranted?.Invoke(_developerRequestCount);

    return true;
}
/// <summary>
/// Called when the player eats candy. Recalculates max HP
/// based on the total eaten so far using the original game's formula.
/// HP is capped at 1000 from candy eating alone.
/// </summary>
public void OnCandyEaten(long totalEaten)
{
    // Original Candy Box 2 HP formula
    // Base 100 HP + up to 900 more from eating, capped at 1000 total
    float newMaxHp = Mathf.Min(100f + (totalEaten / 2673.845f), 1000f);

    SaveManager.Instance.Data.maxHp = newMaxHp;

    // Restore HP to full when eating candy (feels good on mobile)
    SaveManager.Instance.Data.currentHp = newMaxHp;

    Debug.Log($"[GameManager] Max HP updated to {newMaxHp:F1} " +
              $"from {totalEaten} total candy eaten.");
}

        /// <summary>
        /// Restore developer request count from loaded save data.
        /// Called by SaveManager after a successful load.
        /// </summary>
        public void RestoreFromSave(int requestCount)
        {
            _developerRequestCount = requestCount;
        }
    }
}