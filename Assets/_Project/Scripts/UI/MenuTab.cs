namespace KawaiiCandyBox.UI
{
    /// <summary>
    /// Identifies which tab is being referenced.
    /// Add new values here as new tabs are unlocked in later phases.
    /// </summary>
    public enum MenuTabType
    {
        Home,
        Config,
        Save,
        Map,
        Inventory,
        LollipopFarm,
        Cauldron
    }

    /// <summary>
    /// Defines when a tab becomes available and what it does.
    /// </summary>
    [System.Serializable]
    public class MenuTabDefinition
    {
        public MenuTabType tabType;
        public string labelKey;           // Localisation key for tab label
        public int requiredRequestCount;  // 0 = always visible once bar appears
        public bool opensScene;           // true = loads a scene, false = opens panel
        public string sceneOrPanelName;   // scene name or panel GameObject name
    }
}
