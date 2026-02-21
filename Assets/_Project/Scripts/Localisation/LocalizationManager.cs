using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KawaiiCandyBox.Localisation
{
    /// <summary>
    /// Manages all player-facing text. Every string the player sees
    /// must go through LocalizationManager.Instance.Get(key).
    ///
    /// String tables are JSON files stored in:
    /// StreamingAssets/Localisation/{languageCode}.json
    ///
    /// Key naming convention:
    ///   ui.hud.health_label
    ///   item.wooden_sword.name
    ///   quest.cellar.name
    ///   npc.squirrel.greeting
    ///   developer.request_1
    ///   offline.welcome_back
    /// </summary>
    public class LocalizationManager : Core.SingletonManager<LocalizationManager>
    {
        private const string FallbackLanguage = "en";
        private const string LocalisationFolder = "Localisation";

        private Dictionary<string, string> _strings
            = new Dictionary<string, string>();

        private string _currentLanguageCode = FallbackLanguage;

        // Fired when language changes so UI components can refresh
        public static event Action OnLanguageChanged;

        public string CurrentLanguageCode => _currentLanguageCode;

        protected override void OnInitialise()
        {
            Debug.Log("[LocalizationManager] Initialised.");
            // Language is loaded properly once SaveManager has loaded,
            // so GameManager calls LoadLanguage() during OnAllServicesReady.
        }

        // ── Public API ───────────────────────────────────────────────

        /// <summary>
        /// Returns the localised string for the given key in the
        /// current language. Falls back to English if missing.
        /// Returns the key itself if not found in any language,
        /// so missing strings are immediately obvious during development.
        /// </summary>
        public string Get(string key)
        {
            if (_strings.TryGetValue(key, out string value))
                return value;

            // Key missing in current language — try English fallback
            if (_currentLanguageCode != FallbackLanguage)
            {
                Debug.LogWarning($"[LocalizationManager] Key '{key}' missing in " +
                                 $"'{_currentLanguageCode}'. Trying English fallback.");

                // Load English table temporarily to find the key
                var fallbackStrings = LoadStringTable(FallbackLanguage);
                if (fallbackStrings != null &&
                    fallbackStrings.TryGetValue(key, out string fallback))
                    return fallback;
            }

            // Not found anywhere — return the key so it's visible in UI
            Debug.LogWarning($"[LocalizationManager] Key '{key}' not found " +
                             $"in any language table.");
            return $"[{key}]";
        }

        /// <summary>
        /// Load the string table for the given language code.
        /// Called by GameManager once save data is available.
        /// Pass empty string to auto-detect from device locale.
        /// </summary>
        public void LoadLanguage(string languageCode)
        {
            // Auto-detect from device if no preference saved
            if (string.IsNullOrEmpty(languageCode))
                languageCode = DetectDeviceLanguage();

            var table = LoadStringTable(languageCode);

            if (table == null || table.Count == 0)
            {
                Debug.LogWarning($"[LocalizationManager] Could not load '{languageCode}'. " +
                                 $"Falling back to English.");
                languageCode = FallbackLanguage;
                table = LoadStringTable(FallbackLanguage);
            }

            if (table == null)
            {
                Debug.LogError("[LocalizationManager] Could not load English fallback. " +
                               "Check that StreamingAssets/Localisation/en.json exists.");
                return;
            }

            _strings = table;
            _currentLanguageCode = languageCode;

            Debug.Log($"[LocalizationManager] Loaded '{languageCode}' " +
                      $"({_strings.Count} strings).");

            OnLanguageChanged?.Invoke();
        }

        /// <summary>
        /// Change language at runtime (called from settings screen).
        /// Saves the preference to SaveData.
        /// </summary>
        public void SetLanguage(string languageCode)
        {
            LoadLanguage(languageCode);
            Core.SaveManager.Instance.Data.languageCode = _currentLanguageCode;
            Core.SaveManager.Instance.SaveGame();
        }

        // ── Private helpers ──────────────────────────────────────────

        /// <summary>
        /// Reads and parses a JSON string table from StreamingAssets.
        /// Returns null if the file doesn't exist or can't be parsed.
        /// </summary>
        private Dictionary<string, string> LoadStringTable(string languageCode)
        {
            string path = Path.Combine(
                Application.streamingAssetsPath,
                LocalisationFolder,
                $"{languageCode}.json"
            );

            try
            {
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[LocalizationManager] File not found: {path}");
                    return null;
                }

                string json = File.ReadAllText(path);

                // JsonUtility can't deserialize Dictionary directly,
                // so we use a simple wrapper class
                var wrapper = JsonUtility.FromJson<StringTableWrapper>(json);
                if (wrapper?.entries == null)
                {
                    Debug.LogWarning($"[LocalizationManager] Failed to parse: {path}");
                    return null;
                }

                var table = new Dictionary<string, string>();
                foreach (var entry in wrapper.entries)
                    table[entry.key] = entry.value;

                return table;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalizationManager] Error reading {path}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Maps Unity's SystemLanguage enum to our language code strings.
        /// Add more languages here as translations become available.
        /// </summary>
        private string DetectDeviceLanguage()
        {
            return Application.systemLanguage switch
            {
                SystemLanguage.French  => "fr",
                SystemLanguage.German  => "de",
                SystemLanguage.Spanish => "es",
                _                      => FallbackLanguage
            };
        }

        // ── JSON serialisation helpers ───────────────────────────────

        [Serializable]
        private class StringTableWrapper
        {
            public StringEntry[] entries;
        }

        [Serializable]
        private class StringEntry
        {
            public string key;
            public string value;
        }
    }
}
