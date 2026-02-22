using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KawaiiCandyBox.UI
{
    /// <summary>
    /// Controls the health bar display.
    /// Appears after developer request 4.
    ///
    /// Scene hierarchy expected:
    ///   HealthBar (this script + inactive by default)
    ///     ├── HealthSlider (Slider component, non-interactable)
    ///     └── HealthText (TextMeshProUGUI — "23 / 35")
    /// </summary>
    public class HealthBarController : MonoBehaviour
    {
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private TextMeshProUGUI _healthText;

        // Replace OnEnable/OnDisable with these in HealthBarController.cs

private void Awake()
{
    Core.GameManager.OnDeveloperRequestGranted += OnRequestGranted;
    Core.GameManager.OnGameReady += OnGameReady;
    Core.GameManager.OnHealthChanged += OnHealthChanged;

    // Hide visual children but keep root active so Awake runs
    SetVisualsVisible(false);
}

private void OnDestroy()
{
    Core.GameManager.OnDeveloperRequestGranted -= OnRequestGranted;
    Core.GameManager.OnGameReady -= OnGameReady;
    Core.GameManager.OnHealthChanged -= OnHealthChanged;
}

private void SetVisualsVisible(bool visible)
{
    if (_healthSlider != null)
        _healthSlider.gameObject.SetActive(visible);
    if (_healthText != null)
        _healthText.gameObject.SetActive(visible);
}

        private void OnGameReady()
{
    if (Core.GameManager.Instance.HasHealthBar)
    {
        SetVisualsVisible(true);
        RefreshDisplay();
    }
}

private void OnRequestGranted(int newRequestCount)
{
    if (newRequestCount == 4)
    {
        SetVisualsVisible(true);
        RefreshDisplay();
        Debug.Log("[HealthBarController] Health bar unlocked.");
    }
}

        private void OnHealthChanged(float currentHp, float maxHp)
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            float currentHp = Core.SaveManager.Instance.Data.currentHp;
            float maxHp = Core.SaveManager.Instance.Data.maxHp;

            if (_healthSlider != null)
            {
                _healthSlider.minValue = 0f;
                _healthSlider.maxValue = maxHp;
                _healthSlider.value = currentHp;
            }

            if (_healthText != null)
            {
                _healthText.text = $"{Mathf.FloorToInt(currentHp)} / " +
                                   $"{Mathf.FloorToInt(maxHp)}";
            }
        }
    }
}
