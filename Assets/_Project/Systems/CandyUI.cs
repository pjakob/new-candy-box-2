using TMPro;
using UnityEngine;

public class CandyUI : MonoBehaviour
{
    [SerializeField] private CandySystem candySystem;
    [SerializeField] private TextMeshProUGUI candyText;

    private void Start()
    {
        candySystem.OnCandyChanged += UpdateCandyText;
        UpdateCandyText(candySystem.CurrentCandy);
    }

    private void OnDestroy()
    {
        candySystem.OnCandyChanged -= UpdateCandyText;
    }

    private void UpdateCandyText(int amount)
    {
        candyText.text = $"Candy: {amount}";
    }
}
