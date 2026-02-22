using UnityEngine;

namespace KawaiiCandyBox.Economy
{
    /// <summary>
    /// ScriptableObject holding all economy balance values.
    /// Edit in the Unity Inspector without touching code.
    /// Create via: Assets → Create → KawaiiCandyBox → Economy Config
    /// </summary>
    [CreateAssetMenu(
        fileName = "EconomyConfig",
        menuName = "KawaiiCandyBox/Economy Config")]
    public class EconomyConfig : ScriptableObject
    {
        [Header("Candy Generation")]
        [Tooltip("Base candy generated per second before any upgrades")]
        public float baseCandyPerSecond = 1f;

        [Header("Offline Progression")]
        [Tooltip("Maximum hours of offline earnings (default 8)")]
        public float offlineCapHours = 8f;

        [Tooltip("Fraction of normal CPS earned offline (1.0 = 100%)")]
        [Range(0f, 1f)]
        public float offlineCandyRate = 1f;

        [Header("Developer Requests")]
        [Tooltip("Candy costs for each developer request in order")]
        public long[] developerRequestCosts = { 30, 5, 5, 5, 10 };

        [Header("Throw Candy")]
        [Tooltip("Candies thrown to earn a chocolate bar")]
        public long chocolateBarThreshold = 1630;

        [Header("Debug / Testing")]
        [Tooltip("Multiply CPS by this in editor only. Set to 100 for fast testing.")]
        public float debugCpsMultiplier = 1f;
    }
}
