using UnityEngine;
using TMPro;

namespace KawaiiCandyBox.UI
{
    /// <summary>
    /// Displays the developer's response messages as requests are granted.
    /// Each new response replaces the previous one.
    /// Appears after the first request is granted.
    /// </summary>
    public class DeveloperResponseLabel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _responseLabel;

        private void OnEnable()
        {
            Core.GameManager.OnDeveloperRequestGranted += OnRequestGranted;
            Core.GameManager.OnGameReady += OnGameReady;
        }

        private void OnDisable()
        {
            Core.GameManager.OnDeveloperRequestGranted -= OnRequestGranted;
            Core.GameManager.OnGameReady -= OnGameReady;
        }

        private void OnGameReady()
        {
            // Restore last response for returning players
            int count = Core.GameManager.Instance.DeveloperRequestCount;
            if (count > 0)
                ShowResponse(count);
            else
                gameObject.SetActive(false);
        }

        private void OnRequestGranted(int newRequestCount)
        {
            gameObject.SetActive(true);
            ShowResponse(newRequestCount);
        }

        private void ShowResponse(int requestCount)
        {
            if (_responseLabel == null) return;

            string key = $"developer.response.{requestCount}";
            _responseLabel.text = Localisation.LocalizationManager.Instance.Get(key);
        }
    }
}
