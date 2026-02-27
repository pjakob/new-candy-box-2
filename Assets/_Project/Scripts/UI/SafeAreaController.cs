using UnityEngine;

namespace KawaiiCandyBox.UI
{
    /// <summary>
    /// Adjusts a RectTransform to stay within the device's safe area.
    /// Handles iPhone notches, Dynamic Island, and home indicators.
    /// Attach to a child GameObject inside the Canvas that wraps all
    /// game content. The RectTransform will be inset automatically.
    /// </summary>
    [ExecuteAlways]
    public class SafeAreaController : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private Vector2 _lastScreenSize;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            // Only recalculate if safe area or screen size changed
            // (handles rotation, resolution changes in editor)
            Rect safeArea = Screen.safeArea;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            if (safeArea != _lastSafeArea || screenSize != _lastScreenSize)
            {
                ApplySafeArea(safeArea);
                _lastSafeArea = safeArea;
                _lastScreenSize = screenSize;
            }
        }

        private void ApplySafeArea(Rect safeArea)
        {
            if (_rectTransform == null) return;

            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            // Convert safe area to anchor values (0-1 range)
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= screenSize.x;
            anchorMin.y /= screenSize.y;
            anchorMax.x /= screenSize.x;
            anchorMax.y /= screenSize.y;

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            Debug.Log($"[SafeAreaController] Applied safe area: " +
                      $"anchorMin={anchorMin}, anchorMax={anchorMax}");
        }
    }
}