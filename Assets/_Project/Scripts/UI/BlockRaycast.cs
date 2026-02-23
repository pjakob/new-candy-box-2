using UnityEngine;
using UnityEngine.EventSystems;

namespace KawaiiCandyBox.UI
{
    /// <summary>
    /// Attach to PanelContent to prevent clicks inside the panel
    /// from reaching the dim overlay behind it.
    /// </summary>
    public class BlockRaycast : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            // Consume the click — don't let it reach the overlay
            eventData.Use();
        }
    }
}
