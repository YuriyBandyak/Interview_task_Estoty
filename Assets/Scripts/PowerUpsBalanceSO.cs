using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Balance/" + nameof(PowerUpsBalanceSO))]
public class PowerUpsBalanceSO : ScriptableObject {

    [SerializeField] private List<SerializableKeyValuePair<EnemyType, float>> _powerUpsDropChancesByEnemyType;
    [SerializeField] private List<SerializableKeyValuePair<PowerUpType, float>> _powerUpsDropChances;

    [SerializeField] private float _fireRateIncrease;

    public float FireRateIncrease => _fireRateIncrease;

    public float GetPowerUpDropChanceByEnemyType(EnemyType enemyType) {
        foreach (var kvp in _powerUpsDropChancesByEnemyType)
        {
            if (kvp.Key == enemyType)
            {
                return kvp.Value;
            }
        }
        Debug.LogError($"There is no difined drop chance for enemy with type: {enemyType}");
        return 0;
    }

    public IReadOnlyList<SerializableKeyValuePair<PowerUpType, float>> PowerUpDropChances => _powerUpsDropChances;

    public float GetDropChance(PowerUpType powerUpType) {
        foreach (var kvp in _powerUpsDropChances)
        {
            if (kvp.Key == powerUpType)
            {
                return (float)kvp.Value;
            }
        }
        Debug.LogError($"There is no difined drop chance for power up with type: {powerUpType}");
        return 0;
    }
}