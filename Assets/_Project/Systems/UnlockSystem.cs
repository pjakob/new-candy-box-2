using System;
using UnityEngine;

public class UnlockSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CandySystem candySystem;

    // Unlock Flags
    public bool saveUnlocked { get; private set; }
    public bool lollipopFarmUnlocked { get; private set; }

    // Developer request counter (important for original pacing)
    private int developerRequests = 0;

    public event Action<string> OnFeatureUnlocked;

    private void Start()
    {
        // Subscribe to relevant events
        candySystem.OnCandyChanged += EvaluateUnlocks;
        OnFeatureUnlocked += message => Debug.Log(message);

    }

    private void OnDestroy()
    {
        candySystem.OnCandyChanged -= EvaluateUnlocks;
    }

    public void RequestFeature()
    {
        if (candySystem.CurrentCandy < 30) return;

        candySystem.SpendCandy(30);
        developerRequests++;

        EvaluateUnlocks(0);
    }

    private void EvaluateUnlocks(int _)
    {
        // Save unlock — first developer request
        if (!saveUnlocked && developerRequests >= 1)
        {
            saveUnlocked = true;
            OnFeatureUnlocked?.Invoke("Save Unlocked");
        }

        // Lollipop farm unlock condition (replicating original logic structure)
        if (!lollipopFarmUnlocked &&
            developerRequests >= 2 &&
            candySystem.CandiesOnGround >= 100)
        {
            lollipopFarmUnlocked = true;
            OnFeatureUnlocked?.Invoke("Lollipop Farm Unlocked");
        }
    }
}
