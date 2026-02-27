using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace KawaiiCandyBox.UI
{
    public class ResourceDisplayController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _candyLabel;
        [SerializeField] private TextMeshProUGUI _lollipopLabel;
        [SerializeField] private TextMeshProUGUI _chocolateBarLabel;
        [SerializeField] private TextMeshProUGUI _painsLabel;

        // Also hide/show the dividers alongside their resource label
        // so the row doesn't have orphaned dividers when a resource
        // hasn't been unlocked yet
        [SerializeField] private GameObject _lollipopDivider;
        [SerializeField] private GameObject _chocolateDivider;
        [SerializeField] private GameObject _painsDivider;

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

        private void OnGameReady() => RefreshAll();

        private void OnCandyChanged(long count) => UpdateCandyLabel(count);

        private void OnLollipopChanged(long count)
        {
            // First time receiving a lollipop — set the flag
            if (count > 0 && !Core.SaveManager.Instance.Data.hasSeenLollipops)
            {
                Core.SaveManager.Instance.Data.hasSeenLollipops = true;
                Core.SaveManager.Instance.SaveGame();
            }
            UpdateLollipopLabel(count);
        }

        private void OnChocolateBarChanged(int count)
        {
            // Flag is already set by ResourceManager when first earned
            UpdateChocolateBarLabel(count);
        }

        private void RefreshAll()
        {
            var rm = Economy.ResourceManager.Instance;
            var data = Core.SaveManager.Instance.Data;

            UpdateCandyLabel(rm.CandyCount);
            UpdateLollipopLabel(rm.LollipopCount);
            UpdateChocolateBarLabel(data.chocolateBarCount);
            UpdatePainsLabel(data.painsAuChocolatCount);
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
            bool seen = Core.SaveManager.Instance.Data.hasSeenLollipops;
            _lollipopLabel.gameObject.SetActive(seen);
            if (_lollipopDivider != null) _lollipopDivider.SetActive(seen);
            if (!seen) return;

            _lollipopLabel.text = Localisation.LocalizationManager.Instance
                .Get("ui.resources.lollipops")
                .Replace("{0}", FormatNumber(count));
        }

        private void UpdateChocolateBarLabel(int count)
        {
            if (_chocolateBarLabel == null) return;
            bool seen = Core.SaveManager.Instance.Data.hasSeenChocolateBars;
            _chocolateBarLabel.gameObject.SetActive(seen);
            if (_chocolateDivider != null) _chocolateDivider.SetActive(seen);
            if (!seen) return;

            _chocolateBarLabel.text = Localisation.LocalizationManager.Instance
                .Get("ui.resources.chocolate_bars")
                .Replace("{0}", count.ToString());
        }

        private void UpdatePainsLabel(int count)
        {
            if (_painsLabel == null) return;
            bool seen = Core.SaveManager.Instance.Data.hasSeenPainsAuChocolat;
            _painsLabel.gameObject.SetActive(seen);
            if (_painsDivider != null) _painsDivider.SetActive(seen);
            if (!seen) return;

            _painsLabel.text = Localisation.LocalizationManager.Instance
                .Get("ui.resources.pains")
                .Replace("{0}", count.ToString());
        }

        private string FormatNumber(long count)
        {
            if (count >= 1_000_000_000) return $"{count / 1_000_000_000.0:F1}B";
            if (count >= 1_000_000)     return $"{count / 1_000_000.0:F1}M";
            if (count >= 1_000)         return $"{count / 1_000.0:F1}K";
            return count.ToString();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu")
                RefreshAll();
        }
    }
}