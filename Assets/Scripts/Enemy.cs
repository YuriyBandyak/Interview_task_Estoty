using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour, IPoolable {

    private const float OutOfScreenOffset = .1f;

    [SerializeField] private EnemyBalanceSO _enemyBalance;
    [SerializeField] private Rigidbody _body;
    [SerializeField] private EnemyType _enemyType;

    private ProjectilesPool _projectilesPool;
    private ParticlesPool _particlesPool;

    private bool _canFire;
    private int _health;
    private float _fireTimer;

    public event Action<Enemy, DeathType> OnDieEvent;
    private event Action<Enemy> _returnToPoolAction;

    public EnemyType EnemyType => _enemyType;

    public void Init(ProjectilesPool projectilesPool, ParticlesPool particlesPool, Func<float> currentGameTimeGetter, Action<Enemy> ReturnToPoolAction) {
        _canFire = Random.value < _enemyBalance.CanFireChance;
        _returnToPoolAction = ReturnToPoolAction;
        _health = _enemyBalance.MinimalHealth + Mathf.Min(Mathf.FloorToInt(currentGameTimeGetter.Invoke() / _enemyBalance.TimeOfHealthIncrease), _enemyBalance.MaximalHealth);

        _projectilesPool = projectilesPool;
        this._particlesPool = particlesPool;
        gameObject.SetActive(true);
    }

    public void OnProjectileHit(int damage) {
        _health -= damage;
        if (_health <= 0)
        {
            Die(DeathType.BY_PROJECTILE);
        }
    }

    public void OnHitByPlayer() {
        Die(DeathType.BY_PLAYER_COLLISION);
    }

    public void ReturnPoolable() {
        gameObject.SetActive(false);
        _returnToPoolAction.Invoke(this);
    }

    private void Update() {

        if (_canFire)
        {
            _fireTimer += Time.deltaTime;
            if (_fireTimer >= _enemyBalance.FireInterval)
            {
                var projectile = _projectilesPool.Get();
                projectile.gameObject.SetActive(true);
                projectile.Init(1, OnProjectileDestroy, _enemyBalance.ProjectileSpeed);
                projectile.transform.position = transform.position;
                _fireTimer -= _enemyBalance.FireInterval;
            }
        }

        CheckSreenBounds();
    }

    private void FixedUpdate() {
        var p = _body.position;
        p += Vector3.down * (_enemyBalance.Speed * Time.deltaTime);
        _body.MovePosition(p);
    }

    private void CheckSreenBounds() {
        if (transform.IsOutOfScreen(OutOfScreenOffset))
        {
            Destroy();
        }
    }

    private void Die(DeathType deathType) {
        var fx = _particlesPool.Get(ParticleType.EXPLOSION_VFX);
        fx.Init(ParticleType.EXPLOSION_VFX, _particlesPool.Return);
        fx.PlayAndReturn(transform.position);

        OnDieEvent.Invoke(this, deathType);
        Destroy();
    }

    private void Destroy() {
        ReturnPoolable();
        OnDieEvent = null;
    }

    private void OnProjectileDestroy(Projectile projectile) {
        projectile.gameObject.SetActive(false);
        _projectilesPool.Return(projectile);
    }

    public enum DeathType {
        BY_PROJECTILE = 0,
        BY_PLAYER_COLLISION = 1,
    }
}
