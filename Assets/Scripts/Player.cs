using Cysharp.Threading.Tasks;
using PrimeTween;
using System;
using UnityEngine;

public class Player : MonoBehaviour {

    [SerializeField] private Transform _projectileSpawnLocation;
    [SerializeField] private PlayerBalanceSO _playerBalanceSO;
    [SerializeField] private Rigidbody _body;

    private ParticlesPool _particlesPool;
    private ProjectilesPool _projectilesPool;

    private int _currentHealth;
    private float _fireTimer = 0.0f;

    private bool _hasInput = false;
    private Vector2 _lastInput;

    private Vector2 _playerAllowedMovementHorizontal;
    private Vector2 _playerAllowedMovementVertical;

    private float _currentFireInterval;

    private event Action _onDieAction;
    private event Action<int> _onPlayerHealthUpdateAction;

    public void Init(ParticlesPool particlesPool, ProjectilesPool projectilesPool, Action onPlayerDie, Action<int> updatePlayerHealthOnUIAction, Vector2 playerAllowedMovementHorizontal, Vector2 playerAllowedMovementVertical) {
        this._onDieAction = onPlayerDie;
        this._onPlayerHealthUpdateAction = updatePlayerHealthOnUIAction;
        _particlesPool = particlesPool;
        _projectilesPool = projectilesPool;
        _playerAllowedMovementHorizontal = playerAllowedMovementHorizontal;
        _playerAllowedMovementVertical = playerAllowedMovementVertical;
        _currentHealth = _playerBalanceSO.MaxHealth;
        _currentFireInterval = _playerBalanceSO.DefaultFireInterval;

        _onPlayerHealthUpdateAction.Invoke(_currentHealth);
        _projectilesPool.Init();
    }

    public PlayerParametersModifiers GetParameterModifiers() {
        var modifiers = new PlayerParametersModifiers()
        {
            FireRateSetter = (float modifier) => _currentFireInterval = _playerBalanceSO.DefaultFireInterval * modifier,
            IncreaseHealthAction = IncreaseHealth,
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
            Hit();
            enemy.OnHitByPlayer();
        }
    }

    public void Hit() {
        _currentHealth--;
        if (_currentHealth <= 0)
        {
            OnDie();
            return;
        }
        _onPlayerHealthUpdateAction.Invoke(_currentHealth);
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
        projectileGO.Init(1, OnProjectileDestoyAction);
        projectileGO.transform.position = _projectileSpawnLocation.position;
        _fireTimer -= _currentFireInterval;
    }

    private void IncreaseHealth() {
        _currentHealth = Mathf.Min(_currentHealth + 1, _playerBalanceSO.MaxHealth);
        _onPlayerHealthUpdateAction.Invoke(_currentHealth);
    }

    private void OnDie() {
        // TODO: idk maybe some slow motion (particles speed change) on death, current ui hide, then fading game over UI with score 
        var fx = _particlesPool.Get(ParticleType.EXPLOSION_VFX);
        fx.transform.position = transform.position;
        WaitTillExplosionEnd(fx);

        Destroy(gameObject); // TODO: dont destroy, just hide and show after level restart. Should I realy do that? Currently its reloading level by ui (that 100% need refactoring)
        _onDieAction?.Invoke();
    }

    private async UniTask WaitTillExplosionEnd(ParticleSystem fx) {
        await UniTask.Delay(Mathf.RoundToInt(fx.main.duration * 1000)); // TODO: check cancellation on aplication quit and similar stuff 
        await UniTask.WaitUntil(() => fx.particleCount == 0);
        fx.gameObject.SetActive(false);
        _particlesPool.Return(fx, ParticleType.EXPLOSION_VFX);
    }

    private void OnProjectileDestoyAction(Projectile projectile) {
        projectile.gameObject.SetActive(false);
        _projectilesPool.Return(projectile);
    }
}
