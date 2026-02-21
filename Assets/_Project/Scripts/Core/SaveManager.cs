using System;
using System.IO;
using UnityEngine;

namespace KawaiiCandyBox.Core
{
    /// <summary>
    /// Stores all persistent game data. Add new fields here
    /// as new systems are built. All fields are serialized to JSON.
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        // ── Core resources ──────────────────────────────────────────
        public long candyCount = 0;
        public long candyEatenTotal = 0;        // drives max HP formula
        public long lollipopCount = 0;

        // ── Offline progression ──────────────────────────────────────
        public long lastSaveTimestampUtc = 0;   // Unix timestamp (seconds)

        // ── Developer request progression ───────────────────────────
        // 0 = nothing unlocked, 5 = fully unlocked
        public int developerRequestCount = 0;

        // ── Health ──────────────────────────────────────────────────
        // Only meaningful once developerRequestCount >= 4
        public float currentHp = 100f;
        public float maxHp = 100f;

        // ── Settings ─────────────────────────────────────────────────
        public string languageCode = "";        // empty = auto-detect from device
        // ── Inventory (early game) ───────────────────────────────────
public int chocolateBarCount = 0;
public long totalCandiesThrown = 0;  // tracks throw animation progress
    }

    /// <summary>
    /// Handles saving and loading game data to/from JSON on disk.
    /// Also records the UTC timestamp needed for offline progression.
    ///
    /// NOTE: The save *system* is always active from session 1.
    /// The save *button* in the UI is separately gated behind
    /// developer request 3 (HasSaveButton). These are different things.
    /// </summary>
    public class SaveManager : SingletonManager<SaveManager>
    {
        private const string SaveFileName = "save.json";
        private const float AutoSaveIntervalSeconds = 300f;    // 5 minutes

        private string _savePath;
        private float _autoSaveTimer = 0f;
        private bool _dataLoaded = false;

        public SaveData Data { get; private set; } = new SaveData();

        protected override void OnInitialise()
        {
            _savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
            Debug.Log($"[SaveManager] Save path: {_savePath}");
        }

        private void Update()
        {
            if (!_dataLoaded) return;

            _autoSaveTimer += Time.deltaTime;
            if (_autoSaveTimer >= AutoSaveIntervalSeconds)
            {
                _autoSaveTimer = 0f;
                SaveGame();
            }
        }

        /// <summary>
        /// Called by iOS when the app is backgrounded or foregrounded.
        /// This is the critical hook for offline progression timing.
        /// </summary>
        private void OnApplicationPause(bool isPaused)
        {
            if (!_dataLoaded) return;

            if (isPaused)
            {
                // App going to background — record timestamp and save
                Debug.Log("[SaveManager] App paused — saving with timestamp.");
                SaveGame();
            }
            else
            {
                // App returning to foreground — apply offline progression
                Debug.Log("[SaveManager] App resumed — checking offline earnings.");
                ApplyOfflineProgression();
            }
        }

        // ── Public API ───────────────────────────────────────────────

        /// <summary>
        /// Save current game state to disk.
        /// Always records the current UTC timestamp.
        /// </summary>
        public void SaveGame()
        {
            try
            {
                Data.lastSaveTimestampUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                string json = JsonUtility.ToJson(Data, prettyPrint: true);
                File.WriteAllText(_savePath, json);

                Debug.Log("[SaveManager] Game saved.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save failed: {e.Message}");
            }
        }

        /// <summary>
        /// Load game state from disk. If no save exists, starts fresh.
        /// Called once by GameManager during initialisation.
        /// </summary>
        public void LoadGame()
        {
            try
            {
                if (File.Exists(_savePath))
                {
                    string json = File.ReadAllText(_savePath);
                    Data = JsonUtility.FromJson<SaveData>(json);
                    _dataLoaded = true;
                    Debug.Log("[SaveManager] Save loaded successfully.");

                    // Apply any offline earnings from time since last save
                    ApplyOfflineProgression();
                }
                else
                {
                    // First ever session — start with fresh data
                    Data = new SaveData();
                    _dataLoaded = true;
                    Debug.Log("[SaveManager] No save found — starting fresh.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Load failed: {e.Message}. Starting fresh.");
                Data = new SaveData();
                _dataLoaded = true;
            }
        }

        /// <summary>
        /// Delete save data and reset to a fresh game state.
        /// Used by the debug menu and for testing.
        /// </summary>
        public void DeleteSave()
        {
            try
            {
                if (File.Exists(_savePath))
                    File.Delete(_savePath);

                Data = new SaveData();
                Debug.Log("[SaveManager] Save deleted.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Delete failed: {e.Message}");
            }
        }

        // ── Offline progression ──────────────────────────────────────

        /// <summary>
        /// Calculates how much time has passed since last save and
        /// awards the appropriate offline earnings.
        /// </summary>
        private void ApplyOfflineProgression()
        {
            if (Data.lastSaveTimestampUtc == 0)
            {
                // First ever session, no previous timestamp
                return;
            }

            long nowUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long elapsedSeconds = nowUtc - Data.lastSaveTimestampUtc;

            // Clamp to offline cap (from EconomyConfig — hardcoded
            // temporarily until EconomyConfig ScriptableObject exists)
            const long offlineCapSeconds = 8 * 60 * 60;    // 8 hours
            elapsedSeconds = Math.Min(elapsedSeconds, offlineCapSeconds);

            if (elapsedSeconds < 60)
            {
                // Less than a minute — not worth showing the screen
                return;
            }

            // ResourceManager will handle the actual resource award
            // once it exists. For now we store the elapsed time so
            // ResourceManager can pick it up during its own initialisation.
            _pendingOfflineSeconds = elapsedSeconds;

            Debug.Log($"[SaveManager] Offline time: {elapsedSeconds}s " +
                      $"({elapsedSeconds / 3600f:F1} hours)");
        }

        // Pending offline seconds — ResourceManager reads this
        // during its initialisation to apply the actual earnings
        private long _pendingOfflineSeconds = 0;
        public long ConsumePendingOfflineSeconds()
        {
            long pending = _pendingOfflineSeconds;
            _pendingOfflineSeconds = 0;
            return pending;
        }
    }
}
