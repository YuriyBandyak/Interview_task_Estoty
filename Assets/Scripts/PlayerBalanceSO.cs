using UnityEngine;

[CreateAssetMenu(menuName = "Balance/" + nameof(PlayerBalanceSO))]
public class PlayerBalanceSO : ScriptableObject {
    [SerializeField] private int _maxHealth;
    [SerializeField] private float _defaultSpeed;
    [SerializeField] private float _defaultFireInterval;
    [Header("Movement")]
    [SerializeField] private AnimationCurve _speedCurve;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _distanceForMaxSpeed;

    public int MaxHealth => _maxHealth;
    public float DefaultSpeed => _defaultSpeed;
    public float DefaultFireInterval => _defaultFireInterval;
    public float MaxSpeed => _maxSpeed;
    public float DistanceForMaxSpeed => _distanceForMaxSpeed;
    public AnimationCurve SpeedCurve => _speedCurve;
}