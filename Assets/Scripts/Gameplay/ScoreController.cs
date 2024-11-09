using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreController : MonoBehaviour {
    private int _enemiesKilledCount;
    private int _totalEnemiesCount;
    private int _collisionWithEnemiesCount;
    private int _totalShotsCount;
    private int _successfulShotsCount;
    private int _effectiveHealthPowerUpsCount;

    private Func<IReadOnlyList<KeyValuePair<PowerUpType, int>>> _powerUpsCountGetter;

    public IReadOnlyList<KeyValuePair<PowerUpType, int>> AllPowerUpsCount => _powerUpsCountGetter.Invoke();

    public int EnemiesKilledCount => _enemiesKilledCount;
    public int TotalEnemiesCount => _totalEnemiesCount;
    public int CollisionWithEnemiesCount => _collisionWithEnemiesCount;
    public float ShotsAccuracy => (float)_successfulShotsCount / _totalShotsCount;
    public int EffectiveHealthPowerUpsCount => _effectiveHealthPowerUpsCount;
    public int GetPickedPowerUpsCount() {
        var totalCount = 0;
        foreach (var kvp in AllPowerUpsCount)
        {
            if (kvp.Key != PowerUpType.HEATH)
            {
                totalCount += kvp.Value;
            }
        }
        return totalCount;
    }

    public void SetGetters(Func<IReadOnlyList<KeyValuePair<PowerUpType, int>>> powerUpsCountGetter) {
        this._powerUpsCountGetter = powerUpsCountGetter;
    }

    public void OnSuccessfulShot() {
        _successfulShotsCount++;
    }

    public void OnPlayerProjectileFired() {
        _totalShotsCount++;
    }

    public void OnEffectiveHealthPowerUp() {
        _effectiveHealthPowerUpsCount++;
    }

    public void OnEnemySpawn() {
        _totalEnemiesCount++;
    }

    public void OnEnemyDie(Enemy.DeathType deathType) {
        switch (deathType)
        {
            case Enemy.DeathType.BY_PROJECTILE:
                _enemiesKilledCount++;
                break;
            case Enemy.DeathType.BY_PLAYER_COLLISION:
                _collisionWithEnemiesCount++;
                break;
            default:
                Debug.LogError($"Score not handled for type of enemy death: {deathType}");
                break;
        }
    }
}