using System.Collections.Generic;
using UnityEngine;

namespace KawaiiCandyBox.Content
{
    /// <summary>
    /// Central asset discovery service. All game systems load
    /// ScriptableObject assets through this class exclusively.
    /// Never call Resources.LoadAll directly anywhere else.
    ///
    /// This abstraction means if we ever migrate to Addressables,
    /// only this file needs to change.
    /// </summary>
    public class ContentRegistry : Core.SingletonManager<ContentRegistry>
    {
        // Internal caches — assets are loaded once and reused
        private readonly Dictionary<System.Type, object[]> _cache
            = new Dictionary<System.Type, object[]>();

        protected override void OnInitialise()
        {
            Debug.Log("[ContentRegistry] Initialised.");
        }

        // ── Public API ───────────────────────────────────────────────

        /// <summary>
        /// Returns all assets of type T found in Resources/{folderName}/.
        /// Results are cached after the first load.
        ///
        /// Example: ContentRegistry.Instance.GetAll<LocationDefinition>()
        /// </summary>
        public T[] GetAll<T>() where T : ScriptableObject
        {
            System.Type type = typeof(T);
            string folderName = GetFolderName<T>();

            if (_cache.TryGetValue(type, out object[] cached))
                return cached as T[];

            T[] loaded = Resources.LoadAll<T>(folderName);

            if (loaded == null || loaded.Length == 0)
            {
                Debug.LogWarning($"[ContentRegistry] No assets of type {type.Name} " +
                                 $"found in Resources/{folderName}/");
                return System.Array.Empty<T>();
            }

            _cache[type] = loaded;
            Debug.Log($"[ContentRegistry] Loaded {loaded.Length} " +
                      $"{type.Name} assets from Resources/{folderName}/");
            return loaded;
        }

        /// <summary>
        /// Returns a single asset of type T matching the given ID.
        /// Searches the cached results from GetAll<T>().
        /// Returns null and logs a warning if not found.
        ///
        /// Example: ContentRegistry.Instance.GetById<QuestDefinition>("cellar")
        /// </summary>
        //public T GetById<T>(string id) where T : ScriptableObject, IIdentifiable
        public T GetById<T>(string id) where T : ScriptableObject
{
    // TODO: Restore IIdentifiable constraint once Definition classes exist
    Debug.LogWarning($"[ContentRegistry] GetById not yet implemented.");
    return null;
}

        /// <summary>
        /// Clears the asset cache. Only needed during development
        /// (e.g. hot-reload in editor). Never call in production.
        /// </summary>
        public void ClearCache()
        {
            _cache.Clear();
            Debug.Log("[ContentRegistry] Cache cleared.");
        }

        // ── Private helpers ──────────────────────────────────────────

        /// <summary>
        /// Maps a ScriptableObject type to its Resources subfolder name.
        /// Add a new entry here whenever a new definition type is created.
        /// </summary>
        private string GetFolderName<T>() where T : ScriptableObject
{
    // TODO: Add entries here as Definition types are created
    // e.g. { typeof(Definitions.LocationDefinition), "Locations" },
    
    // For now, fall back to the class name as the folder name
    System.Type type = typeof(T);
    Debug.LogWarning($"[ContentRegistry] No folder mapping for {type.Name}. " +
                     $"Falling back to '{type.Name}'.");
    return type.Name;
}
    }
}