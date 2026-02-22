using UnityEngine;

namespace KawaiiCandyBox.UI
{
    /// <summary>
    /// Marks the persistent UI Canvas as DontDestroyOnLoad so the
    /// menu bar and other persistent UI survive scene transitions.
    /// Attach to the root Canvas GameObject in the Bootstrap scene.
    /// </summary>
    public class PersistentUICanvas : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
