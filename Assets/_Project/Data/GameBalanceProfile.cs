using UnityEngine;

[CreateAssetMenu(menuName = "Game/Balance Profile")]
public class GameBalanceProfile : ScriptableObject
{
    [Header("Global Multipliers")]
    [Tooltip("Multiplier applied to candy gained per click.")]
    public float candyClickMultiplier = 1f;

    [Tooltip("Multiplier applied to candy gained per second.")]
    public float candyPerSecondMultiplier = 1f;

    [Tooltip("Multiplier applied to all enemy health values.")]
    public float enemyHealthMultiplier = 1f;

    [Tooltip("Multiplier applied to all enemy damage values.")]
    public float enemyDamageMultiplier = 1f;

    [Tooltip("Multiplier applied to all item costs.")]
    public float itemCostMultiplier = 1f;

    [Header("Idle / Offline Progression")]
    [Tooltip("Maximum number of hours offline progression can accumulate.")]
    public float offlineProgressionHoursCap = 8f;
}