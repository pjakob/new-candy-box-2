using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace KawaiiCandyBox.UI
{
    /// <summary>
    /// Displays current resource counts (candy, lollipops, chocolate bars).
    /// Lives in PersistentUI so it's visible on all scenes.
    /// Replaces the old CandyCounterLabel in CandyBoxScreen.
    /// </summary>
    public class ResourceDisplayController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _candyLabel;
        [SerializeField] private TextMeshProUGUI _lollipopLabel;
        [SerializeField] private TextMeshProUGUI _chocolateBarLabel;

        private void Awake()
        {
            Economy.ResourceManager.OnCandyChanged += OnCandyChanged;
            Economy.ResourceManager.OnLollipopChanged += OnLollipopChanged;
            Economy.ResourceManager.OnChocolateBarChanged += OnChocolateBarChanged;
            Core.GameManager.OnGameReady += OnGameReady;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            Economy.ResourceManager.OnCandyChanged -= OnCandyChanged;
            Economy.ResourceManager.OnLollipopChanged -= OnLollipopChanged;
            Economy.ResourceManager.OnChocolateBarChanged -= OnChocolateBarChanged;
            Core.GameManager.OnGameReady -= OnGameReady;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnGameReady()
        {
            RefreshAll();
        }

        private void OnCandyChanged(long count) => UpdateCandyLabel(count);
        private void OnLollipopChanged(long count) => UpdateLollipopLabel(count);
        private void OnChocolateBarChanged(int count) => UpdateChocolateBarLabel(count);

        private void RefreshAll()
        {
            var rm = Economy.ResourceManager.Instance;
            UpdateCandyLabel(rm.CandyCount);
            UpdateLollipopLabel(rm.LollipopCount);
            UpdateChocolateBarLabel(
                Core.SaveManager.Instance.Data.chocolateBarCount);
        }

        private void UpdateCandyLabel(long count)
        {
            if (_candyLabel == null) return;

            string countStr;
            if (count == 42)        countStr = "42 \\o/";
            else if (count == 1337) countStr = "leet";
            else                    countStr = FormatNumber(count);

            _candyLabel.text = Localisation.LocalizationManager.Instance
                .Get("ui.resources.candy")
                .Replace("{0}", countStr);
        }

        private void UpdateLollipopLabel(long count)
        {
            if (_lollipopLabel == null) return;

            bool hasSeenLollipops =
                Core.SaveManager.Instance.Data.hasSeenLollipops;
            _lollipopLabel.gameObject.SetActive(hasSeenLollipops);

            if (hasSeenLollipops)
                _lollipopLabel.text = Localisation.LocalizationManager.Instance
                    .Get("ui.resources.lollipops")
                    .Replace("{0}", FormatNumber(count));
        }

        private void UpdateChocolateBarLabel(int count)
        {
            if (_chocolateBarLabel == null) return;

            bool hasSeenChocolate =
                Core.SaveManager.Instance.Data.hasSeenChocolateBars;
            _chocolateBarLabel.gameObject.SetActive(hasSeenChocolate);

            if (hasSeenChocolate)
                _chocolateBarLabel.text = Localisation.LocalizationManager.Instance
                    .Get("ui.resources.chocolate_bars")
                    .Replace("{0}", count.ToString());
        }

        private string FormatNumber(long count)
        {
            if (count >= 1_000_000_000) return $"{count / 1_000_000_000.0:F1}B";
            if (count >= 1_000_000)     return $"{count / 1_000_000.0:F1}M";
            if (count >= 1_000)         return $"{count / 1_000.0:F1}K";
            return count.ToString();
        }
        private void OnSceneLoaded(
            UnityEngine.SceneManagement.Scene scene,
            UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (scene.name == "MainMenu")
                RefreshAll();
            }
        }
}
