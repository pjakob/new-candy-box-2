using System;
using UnityEngine;

public class CandySystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameBalanceProfile balanceProfile;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private UnlockSystem unlockSystem;

    [SerializeField] private int candiesOnGround = 0;

    [Header("Base Production")]
    [SerializeField] private float baseProductionPerSecond = 1f;

    private int currentCandy = 0;

    private float candyRemainder = 0f;

    public event Action<int> OnCandyChanged;

    public int CurrentCandy => currentCandy;

    // Example production sources (expand later)
    private float lollipopProduction = 0f;
    private float itemProduction = 0f;

    public int CandiesOnGround => candiesOnGround;

    private void Update()
    {
        float totalProduction = GetTotalProductionPerSecond();

        float candyThisFrame = totalProduction * Time.deltaTime;
        candyRemainder += candyThisFrame;
        if (Input.GetKeyDown(KeyCode.E))
        {
            EatAllCandy();
            Debug.Log("Max HP: " + playerStats.MaxHP);
        }
        if (Input.GetKeyDown(KeyCode.T))
{
    ThrowTenCandies();
}

if (Input.GetKeyDown(KeyCode.R))
{
    unlockSystem.RequestFeature();
}


        if (candyRemainder >= 1f)
        {
            int wholeCandy = Mathf.FloorToInt(candyRemainder);
            candyRemainder -= wholeCandy;
            AddCandy(wholeCandy);
        }
    }

    private float GetTotalProductionPerSecond()
    {
        float total =
            baseProductionPerSecond
            + lollipopProduction
            + itemProduction;

        total *= balanceProfile.candyPerSecondMultiplier;

        return total;
    }

    private void AddCandy(int amount)
    {
        if (amount <= 0) return;

        currentCandy += amount;
        OnCandyChanged?.Invoke(currentCandy);
    }

    public void EatAllCandy()
    {
        int candyToEat = currentCandy;

        if (candyToEat <= 0) return;

        currentCandy = 0;
        playerStats.IncreaseMaxHP(candyToEat);
        OnCandyChanged?.Invoke(currentCandy);
    }
    public void SpendCandy(int amount)
    {
        if (amount <= 0) return;
        if (currentCandy < amount) return;

        currentCandy -= amount;
        OnCandyChanged?.Invoke(currentCandy);
    }
    public void ThrowTenCandies()
    {
        if (currentCandy < 10) return;

        SpendCandy(10);
        candiesOnGround += 10;
    }
}
