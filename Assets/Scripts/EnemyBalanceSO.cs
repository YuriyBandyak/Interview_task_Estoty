using UnityEngine;

[CreateAssetMenu(menuName = "Balance/" + nameof(EnemyBalanceSO))]
public class EnemyBalanceSO : ScriptableObject {
    [SerializeField] private float _powerUpSpawnChance = .1f;
    [SerializeField] private int _minimalHealth = 2;
    [SerializeField] private int _maximalHealth = 5;
    [SerializeField] private float _timeOfHealthIncrease = 15f;
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _fireInterval = 2.5f;
    [SerializeField] private float _canFireChance = .4f;

    public float PowerUpSpawnChance => _powerUpSpawnChance;
    public int MinimalHealth => _minimalHealth;
    public int MaximalHealth => _maximalHealth;
    public float TimeOfHealthIncrease => _timeOfHealthIncrease;
    public float Speed => _speed;
    public float FireInterval => _fireInterval;
    public float CanFireChance => _canFireChance;
}