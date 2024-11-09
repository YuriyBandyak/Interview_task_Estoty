using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] private Transform _projectileSpawnLocation;
    [SerializeField] private PlayerBalanceSO _playerBalanceSO;
    [SerializeField] private Rigidbody _body;
    [SerializeField] private MeshRenderer _shieldRenderer;

    private ParticlesPool _particlesPool;
    private ProjectilesPool _projectilesPool;

    private int _currentHealth;
    private float _fireTimer = 0.0f;
    private bool _hasShield;

    private bool _hasInput = false;
    private Vector2 _lastInput;

    private Vector2 _playerAllowedMovementHorizontal;
    private Vector2 _playerAllowedMovementVertical;

    private float _currentFireInterval;

    public event Action OnDieEvent;
    public event Action<int> OnPlayerHealthUpdateEvent;
    public event Action OnProjectileFiredEvent;
    public event Action OnEffectiveHealEvent;
    public event Action OnProjectileHitEvent;

    public void Init(ParticlesPool particlesPool, ProjectilesPool projectilesPool, Vector2 playerAllowedMovementHorizontal, Vector2 playerAllowedMovementVertical) {
        _particlesPool = particlesPool;
        _projectilesPool = projectilesPool;
        _playerAllowedMovementHorizontal = playerAllowedMovementHorizontal;
        _playerAllowedMovementVertical = playerAllowedMovementVertical;
        _currentHealth = _playerBalanceSO.MaxHealth;
        _currentFireInterval = _playerBalanceSO.DefaultFireInterval;

        OnPlayerHealthUpdateEvent?.Invoke(_currentHealth);
        _projectilesPool.Init();
        ToggleShield(false);
    }

    public PlayerParametersModifiers GetParameterModifiers() {
        var modifiers = new PlayerParametersModifiers()
        {
            FireRateSetter = (float modifier) => _currentFireInterval = _playerBalanceSO.DefaultFireInterval * modifier,
            IncreaseHealthAction = IncreaseHealth,
            ReceiveShieldAction = () => ToggleShield(true),
        };
        return modifiers;
    }

    private void Update() {
        CheckInputs();

        _fireTimer += Time.deltaTime;
        if (_fireTimer >= _currentFireInterval)
        {
            FireProjectile();
        }
    }

    private void FixedUpdate() {
        if (_hasInput)
        {
            Move();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.OnHitByPlayer();
            Hit();
        }
    }

    public void Hit() {
        if (_hasShield)
        {
            ToggleShield(false);
            return;
        }
        _currentHealth--;
        if (_currentHealth <= 0)
        {
            OnDie();
            return;
        }
        OnPlayerHealthUpdateEvent?.Invoke(_currentHealth);
    }

    private void CheckInputs() {
        if (Input.GetMouseButtonDown(0)) _hasInput = true;
        if (Input.GetMouseButtonUp(0)) _hasInput = false;
        if (Input.GetMouseButton(0))
        {
            _lastInput = Input.mousePosition;
        }
    }

    private void Move() {
        var mousePositionWorld = Camera.main.ScreenToWorldPoint(_lastInput);
        var targetPositionX = Mathf.Clamp(mousePositionWorld.x, _playerAllowedMovementHorizontal.x, _playerAllowedMovementHorizontal.y);
        var targetPositionY = Mathf.Clamp(mousePositionWorld.y, _playerAllowedMovementVertical.y, _playerAllowedMovementVertical.x);
        var targetPosition = new Vector3(targetPositionX, targetPositionY, 0); // mouse position in 3D clamped to allowed space

        var distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        var distanceNormalized = Mathf.InverseLerp(0, _playerBalanceSO.DistanceForMaxSpeed, distanceToTarget);
        var allowedDistance = _playerBalanceSO.SpeedCurve.Evaluate(distanceNormalized) * _playerBalanceSO.MaxSpeed;
        allowedDistance = Mathf.Clamp(allowedDistance, 0, distanceToTarget);

        _body.MovePosition(transform.position + (targetPosition - transform.position).normalized * allowedDistance);
    }

    private void FireProjectile() {
        var projectileGO = _projectilesPool.Get();
        projectileGO.gameObject.SetActive(true);
        projectileGO.Init(1, OnProjectileDestoyAction, _playerBalanceSO.ProjectileSpeed, OnProjectileHitEvent);
        projectileGO.transform.position = _projectileSpawnLocation.position;
        _fireTimer -= _currentFireInterval;

        OnProjectileFiredEvent?.Invoke();
    }

    private void IncreaseHealth() {
        var newHealth = Mathf.Min(_currentHealth + 1, _playerBalanceSO.MaxHealth);
        if (newHealth != _currentHealth)
        {
            _currentHealth = newHealth;
            OnPlayerHealthUpdateEvent?.Invoke(_currentHealth);
            OnEffectiveHealEvent?.Invoke();
        }
    }

    private void ToggleShield(bool state) {
        _hasShield = state;
        _shieldRenderer.enabled = _hasShield;
    }

    private void OnDie() {
        Destroy(gameObject);
        OnPlayerHealthUpdateEvent?.Invoke(_currentHealth);
        OnDieEvent?.Invoke();

        OnDieEvent = null;
        OnPlayerHealthUpdateEvent = null;
        OnProjectileFiredEvent = null;
        OnEffectiveHealEvent = null;
        OnProjectileHitEvent = null;
    }
    private void OnProjectileDestoyAction(Projectile projectile) {
        projectile.gameObject.SetActive(false);
        _projectilesPool.Return(projectile);
    }
}
