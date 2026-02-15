using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private int baseMaxHP = 10;

    private int maxHP;
    private int currentHP;

    public event Action<int, int> OnHPChanged; // (currentHP, maxHP)

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;

    private void Awake()
    {
        maxHP = baseMaxHP;
        currentHP = maxHP;
    }

    public void IncreaseMaxHP(int amount)
    {
        if (amount <= 0) return;

        maxHP += amount;
        currentHP = maxHP;

        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(int amount)
    {
        currentHP = Mathf.Max(0, currentHP - amount);
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    public void HealToFull()
    {
        currentHP = maxHP;
        OnHPChanged?.Invoke(currentHP, maxHP);
    }
}