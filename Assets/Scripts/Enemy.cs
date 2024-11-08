using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using static Enemy;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour {

    private const float OutOfScreenOffset = .1f;

    [SerializeField] private EnemyBalanceSO _enemyBalance;
    [SerializeField] private Rigidbody _body;
    [SerializeField] private EnemyType _enemyType;

    private ProjectilesPool _projectilesPool;
    private ParticlesPool _particlesPool;

    private bool _canFire;
    private int _health;
    private float _fireTimer;

    private event Action<Enemy, DeathType> _onDieAction;
    private event Action<Enemy> _onDestroyAction;

    public EnemyType EnemyType => _enemyType;

    public void Init(ProjectilesPool projectilesPool, ParticlesPool particlesPool, Action<Enemy, DeathType> OnDeathAction, Action<Enemy> OnDestroyAction, Func<float> currentGameTimeGetter) {
        _canFire = Random.value < _enemyBalance.CanFireChance;
        _health = _enemyBalance.MinimalHealth + Mathf.Min(Mathf.FloorToInt(currentGameTimeGetter.Invoke() / _enemyBalance.TimeOfHealthIncrease), _enemyBalance.MaximalHealth);

        _projectilesPool = projectilesPool;
        this._particlesPool = particlesPool;
        this._onDieAction = OnDeathAction;
        this._onDestroyAction = OnDestroyAction;
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

    private void Update() {

        if (_canFire)
        {
            _fireTimer += Time.deltaTime;
            if (_fireTimer >= _enemyBalance.FireInterval)
            {
                var projectile = _projectilesPool.Get();
                projectile.gameObject.SetActive(true);
                projectile.Init(1, OnProjectileDestroy);
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
        fx.transform.position = transform.position;
        fx.gameObject.SetActive(true);
        fx.Play();
        WaitTillExplosionEnd(fx);

        _onDieAction.Invoke(this, deathType);
        Destroy();
    }

    private void Destroy() {
        _onDestroyAction.Invoke(this);
    }

    private async UniTask WaitTillExplosionEnd(ParticleSystem fx) {
        await UniTask.Delay(Mathf.RoundToInt(fx.main.duration * 1000)); // TODO: check cancellation on aplication quit and similar stuff 
        await UniTask.WaitUntil(() => fx.particleCount == 0);
        fx.gameObject.SetActive(false);
        _particlesPool.Return(fx, ParticleType.EXPLOSION_VFX);
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
