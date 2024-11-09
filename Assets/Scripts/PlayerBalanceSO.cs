using UnityEngine;

[CreateAssetMenu(menuName = "Balance/" + nameof(PlayerBalanceSO))]
public class PlayerBalanceSO : ScriptableObject {
    [SerializeField] private int _maxHealth;
    [SerializeField] private float _defaultFireInterval;
    [SerializeField] private float _projectileSpeed;
    [Header("Movement")]
    [SerializeField] private AnimationCurve _speedCurve;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _distanceForMaxSpeed;

    public int MaxHealth => _maxHealth;
    public float DefaultFireInterval => _defaultFireInterval;
    public float ProjectileSpeed => _projectileSpeed;
    public float MaxSpeed => _maxSpeed;
    public float DistanceForMaxSpeed => _distanceForMaxSpeed;
    public AnimationCurve SpeedCurve => _speedCurve;
}