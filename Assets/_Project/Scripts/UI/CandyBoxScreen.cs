using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KawaiiCandyBox.Core;
using KawaiiCandyBox.Economy;

namespace KawaiiCandyBox.UI
{
    /// <summary>
    /// The main screen the player sees from the very start of the game.
    /// Manages the candy counter display and the early-game buttons
    /// that are revealed progressively as developer requests are granted.
    ///
    /// Button visibility is driven entirely by GameManager's progression
    /// state — this class never makes unlock decisions itself.
    ///
    /// Scene hierarchy expected:
    ///   CandyBoxScreen
    ///     ├── CandyCounterLabel      (TextMeshProUGUI)
    ///     ├── EatAllButton           (Button)
    ///     │     └── Text            (TextMeshProUGUI)
    ///     ├── ThrowCandyButton       (Button)
    ///     │     └── Text            (TextMeshProUGUI)
    ///     └── DeveloperRequestButton (Button)
    ///           └── Text            (TextMeshProUGUI)
    /// </summary>
    public class CandyBoxScreen : MonoBehaviour
    {
        [Header("Candy Counter")]
        [SerializeField] private TextMeshProUGUI _candyCounterLabel;
        [Header("Messages")]
        [SerializeField] private TextMeshProUGUI _candiesEatenLabel;
        [SerializeField] private TextMeshProUGUI _candiesThrownLabel;

        [Header("Buttons")]
        [SerializeField] private Button _eatAllButton;
        [SerializeField] private TextMeshProUGUI _eatAllButtonText;

        [SerializeField] private Button _throwCandyButton;
        [SerializeField] private TextMeshProUGUI _throwCandyButtonText;

        [SerializeField] private Button _developerRequestButton;
        [SerializeField] private TextMeshProUGUI _developerRequestButtonText;

        // Candy costs for each developer request in order
        // Index 0 = first request (30 candy), then 5, 5, 5, 10
        private static readonly long[] RequestCosts = { 30, 5, 5, 5, 10 };

        private void OnEnable()
        {
            ResourceManager.OnCandyChanged += UpdateCandyDisplay;
            GameManager.OnDeveloperRequestGranted += OnRequestGranted;
            GameManager.OnGameReady += OnGameReady;
            ResourceManager.OnCandyThrown += OnCandyThrown;
            ResourceManager.OnChocolateBarEarned += OnChocolateBarEarned;
        }

        private void OnDisable()
        {
            ResourceManager.OnCandyChanged -= UpdateCandyDisplay;
            GameManager.OnDeveloperRequestGranted -= OnRequestGranted;
            GameManager.OnGameReady -= OnGameReady;
            ResourceManager.OnCandyThrown -= OnCandyThrown;
            ResourceManager.OnChocolateBarEarned -= OnChocolateBarEarned;
        }

        private void Start()
{
    SetButtonLabels();
    RefreshButtonVisibility();
    UpdateCandyDisplay(ResourceManager.Instance.CandyCount);
    RefreshPersistentLabels();  // ← add this
}

private void RefreshPersistentLabels()
{
    var loc = Localisation.LocalizationManager.Instance;
    long totalEaten = ResourceManager.Instance.CandyEatenTotal;
    long totalThrown = Core.SaveManager.Instance.Data.totalCandiesThrown;

    if (_candiesEatenLabel != null)
    {
        bool hasEaten = totalEaten > 0;
        _candiesEatenLabel.gameObject.SetActive(hasEaten);
        if (hasEaten)
            _candiesEatenLabel.text = loc.Get("ui.eat.message")
                .Replace("{0}", totalEaten.ToString());
    }

    if (_candiesThrownLabel != null)
    {
        bool hasThrown = totalThrown > 0;
        _candiesThrownLabel.gameObject.SetActive(hasThrown);
        if (hasThrown)
            _candiesThrownLabel.text = loc.Get("ui.throw.message")
                .Replace("{0}", totalThrown.ToString());
    }
}

        // ── Event handlers ───────────────────────────────────────────
private void OnCandyThrown(long totalThrown, bool earnedChocolate)
{
    if (_candiesThrownLabel == null) return;

    _candiesThrownLabel.gameObject.SetActive(true);
    _candiesThrownLabel.text = Localisation.LocalizationManager.Instance
        .Get("ui.throw.message")
        .Replace("{0}", totalThrown.ToString());

    if (earnedChocolate)
    {
        // TODO: Show chocolate bar notification separately
        // For now just append to the thrown label
        _candiesThrownLabel.text += "\n" + Localisation.LocalizationManager.Instance
            .Get("ui.throw.chocolate_bar_earned");
    }
}

private void OnChocolateBarEarned()
{
    // Handled inside OnCandyThrown above
}
        private void OnGameReady()
        {
            RefreshButtonVisibility();
            UpdateCandyDisplay(ResourceManager.Instance.CandyCount);
        }

        private void OnRequestGranted(int newRequestCount)
        {
            RefreshButtonVisibility();
            UpdateDeveloperRequestButton(newRequestCount);
        }

        // ── Display updates ──────────────────────────────────────────

        private void UpdateCandyDisplay(long candyCount)
{
    if (_candyCounterLabel == null) return;

    // Easter eggs from the original game
    if (candyCount == 42)
        _candyCounterLabel.text = "42 \\o/";
    else if (candyCount == 1337)
        _candyCounterLabel.text = "leet";
    else
        _candyCounterLabel.text = FormatCandyCount(candyCount);

    // Show throw button once player has 10+ candy
    if (_throwCandyButton != null)
        _throwCandyButton.gameObject.SetActive(candyCount >= 10);

    // Show developer request button once player has 30+ candy
    // and not all requests have been granted yet
    if (_developerRequestButton != null)
    {
        int requestCount = GameManager.Instance.DeveloperRequestCount;
        bool shouldShow = candyCount >= 30 && requestCount < 5;
        _developerRequestButton.gameObject.SetActive(shouldShow);

        if (shouldShow)
            UpdateDeveloperRequestButton(requestCount);
    }
}

private void RefreshButtonVisibility()
{
    int requestCount = GameManager.Instance.DeveloperRequestCount;
    long candyCount = ResourceManager.Instance.CandyCount;

    // Eat button always visible
    if (_eatAllButton != null)
        _eatAllButton.gameObject.SetActive(true);

    // Throw button needs 10+ candy
    if (_throwCandyButton != null)
        _throwCandyButton.gameObject.SetActive(candyCount >= 10);

    // Developer request button needs 30+ candy and requests remaining
    if (_developerRequestButton != null)
    {
        bool shouldShow = candyCount >= 30 && requestCount < 5;
        _developerRequestButton.gameObject.SetActive(shouldShow);

        if (shouldShow)
            UpdateDeveloperRequestButton(requestCount);
    }
}

        private void UpdateDeveloperRequestButton(int currentRequestCount)
        {
            if (_developerRequestButton == null) return;
            if (currentRequestCount >= 5)
            {
                _developerRequestButton.gameObject.SetActive(false);
                return;
            }

            // Show the correct label for the next request
            string key = currentRequestCount == 0
                ? "developer.request_button.0"
                : $"developer.request_button.{currentRequestCount}";

            // Fall back to generic repeat label if specific key missing
            string label = Localisation.LocalizationManager.Instance.Get(key);
            if (_developerRequestButtonText != null)
                _developerRequestButtonText.text = label;
        }

        private void SetButtonLabels()
        {
            var loc = Localisation.LocalizationManager.Instance;

            if (_eatAllButtonText != null)
                _eatAllButtonText.text = loc.Get("ui.button.eat_all");

            if (_throwCandyButtonText != null)
                _throwCandyButtonText.text = loc.Get("ui.button.throw_candies");
        }

        // ── Button handlers ──────────────────────────────────────────

        /// <summary>
        /// Called by the Eat All button's OnClick event.
        /// </summary>
        public void OnEatAllPressed()
{
    long candyCount = ResourceManager.Instance.CandyCount;
    ResourceManager.Instance.EatAllCandy();

    if (_candiesEatenLabel != null && candyCount > 0)
    {
        _candiesEatenLabel.gameObject.SetActive(true);
        _candiesEatenLabel.text = Localisation.LocalizationManager.Instance
            .Get("ui.eat.message")
            .Replace("{0}", ResourceManager.Instance.CandyEatenTotal.ToString());
    }
}

        /// <summary>
        /// Called by the Throw Candy button's OnClick event.
        /// </summary>
        public void OnThrowCandyPressed()
        {
            ResourceManager.Instance.ThrowCandy(10);
        }

        /// <summary>
        /// Called by the Developer Request button's OnClick event.
        /// </summary>
        public void OnDeveloperRequestPressed()
        {
            int currentCount = GameManager.Instance.DeveloperRequestCount;
            if (currentCount >= RequestCosts.Length) return;

            long cost = RequestCosts[currentCount];
            GameManager.Instance.TryGrantDeveloperRequest(cost);
        }

        // ── Helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Formats large candy counts readably.
        /// e.g. 1500000 → "1.5M"
        /// </summary>
        private string FormatCandyCount(long count)
        {
            if (count >= 1_000_000_000)
                return $"{count / 1_000_000_000.0:F1}B";
            if (count >= 1_000_000)
                return $"{count / 1_000_000.0:F1}M";
            if (count >= 1_000)
                return $"{count / 1_000.0:F1}K";
            return count.ToString();
        }
    }
}