using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PowerUpsController : MonoBehaviour {
    [SerializeField] private PowerUpsBalanceSO _powerUpsBalance;
    private PowerUpsPool _powerUpsPool;
    private PlayerParametersModifiers _playerParametersModifiers;

    private Dictionary<PowerUpType, int> _powerUpsCount;

    public IReadOnlyList<KeyValuePair<PowerUpType, int>> PowerUpsCount => _powerUpsCount.ToList();

    public void Init(PowerUpsPool powerUpsPool, PlayerParametersModifiers playerParametersModifiers) {
        this._powerUpsPool = powerUpsPool;
        _powerUpsCount = new Dictionary<PowerUpType, int>();
        _playerParametersModifiers = playerParametersModifiers;
    }

    public void OnEnemyDeath(EnemyType enemyType, Vector3 enemyDeathPosition) {
        if (Random.value < _powerUpsBalance.GetPowerUpDropChanceByEnemyType(enemyType))
        {
            var randomPowerUpType = GetRandomPowerUpType();
            if (randomPowerUpType == PowerUpType.NONE)
            {
                return;
            }

            var powerup = _powerUpsPool.Get(randomPowerUpType);
            powerup.transform.position = enemyDeathPosition;
            powerup.gameObject.SetActive(true);
            powerup.Init(randomPowerUpType, OnPowerUpPickedUp, _powerUpsPool.Return, _powerUpsBalance.Speed);
        }
    }

    private PowerUpType GetRandomPowerUpType() {
        var dropChances = _powerUpsBalance.PowerUpDropChances;

        float dropValueSum = 0;
        for (int i = 0; i < dropChances.Count; i++)
        {
            dropValueSum += dropChances[i].Value;
        }

        var randomizedValue = Random.value * dropValueSum;
        for (int i = 0; i < dropChances.Count; i++)
        {
            randomizedValue -= dropChances[i].Value;
            if (randomizedValue < 0)
            {
                return dropChances[i].Key;
            }
        }

        Debug.LogError("Cannot get random power up type");
        return PowerUpType.NONE;
    }

    private void OnPowerUpPickedUp(PowerUp powerUp) {

        var powerUpType = powerUp.PowerUpType;
        if (_powerUpsCount.ContainsKey(powerUpType))
        {
            _powerUpsCount[powerUpType]++;
        }
        else
        {
            _powerUpsCount.Add(powerUpType, 1);
        }

        switch (powerUpType)
        {
            case PowerUpType.FIRE_RATE:
                _playerParametersModifiers.FireRateSetter.Invoke(Mathf.Pow(_powerUpsBalance.FireRateIncrease, _powerUpsCount[powerUpType]));
                break;
            case PowerUpType.HEATH:
                _playerParametersModifiers.IncreaseHealthAction.Invoke();
                break;

            default:
                Debug.LogError($"There is no behaviour for power up of type: {powerUpType}");
                break;
        }
    }

    private void OnPowerUpDestroy(PowerUp powerUp) {
        powerUp.gameObject.SetActive(false);
        _powerUpsPool.Return(powerUp, powerUp.PowerUpType);
    }
}
