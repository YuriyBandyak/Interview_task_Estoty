using Cysharp.Threading.Tasks;
using PrimeTween;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {

    [SerializeField] private GameBalanceSO _gameBalanceSO;
    [SerializeField] private PowerUpsController _powerUpsController;
    [SerializeField] private Player _player;
    [SerializeField] private ScoreController _scoreController;
    [SerializeField] private Vector3 _spawnPosition;
    [SerializeField] private Vector3 _spawnOffsets;
    [SerializeField] private Vector2 _playerAllowedMovementHorizontal;
    [SerializeField] private Vector2 _playerAllowedMovementVertical;
    [Header("Pools")]
    [SerializeField] private EnemiesPool _enemiesPool;
    [SerializeField] private ProjectilesPool _enemiesProjectilesPool;
    [SerializeField] private ProjectilesPool _playerProjectilesPool;
    [SerializeField] private ParticlesPool _particlesPool;
    [SerializeField] private PowerUpsPool _powerUpsPool;
    [Header("UI")]
    [SerializeField] private GameplayUi _gameplayUI;
    [SerializeField] private GameOverUi _gameOverUI;

    private float _enemySpawnTimer = 0.0f;
    private bool _running = true;
    private Func<float> _currentGameTimeGetter;

    private void Awake() {
        Application.targetFrameRate = 60;
    }

    private void Start() {
        _enemiesPool.Init();
        _enemiesProjectilesPool.Init();
        _playerProjectilesPool.Init();
        _particlesPool.Init();
        _powerUpsPool.Init();

        _player.Init(_particlesPool, _playerProjectilesPool, _playerAllowedMovementHorizontal, _playerAllowedMovementVertical);
        _powerUpsController.Init(_powerUpsPool, _player.GetParameterModifiers());

        _running = true;
        var gameStartTime = Time.time;
        _currentGameTimeGetter = () => Time.time - gameStartTime;

        _gameOverUI.Init(OnRetry);
        _gameOverUI.SetGetters(() => _scoreController.EnemiesKilledCount, () => _scoreController.TotalEnemiesCount, () => _scoreController.CollisionWithEnemiesCount, () => _scoreController.ShotsAccuracy, () => _scoreController.EffectiveHealthPowerUpsCount, () => _scoreController.GetPickedPowerUpsCount());
        _gameOverUI.Close();

        SubscribeToPlayerEvents();
        _scoreController.SetGetters(() => _powerUpsController.PowerUpsCount);
    }

    private void Update() {
        if (!_running) return;
        _enemySpawnTimer += Time.deltaTime;
        if (_enemySpawnTimer >= _gameBalanceSO.EnemySpawnInterval)
        {
            SpawnEnemy();
        }
    }

    private void SubscribeToPlayerEvents() {
        _player.OnDieEvent += OnPlayerDie;
        _player.OnPlayerHealthUpdateEvent += UpdatePlayerHealthOnUIAction;

        _player.OnProjectileFiredEvent += _scoreController.OnPlayerProjectileFired;
        _player.OnEffectiveHealEvent += _scoreController.OnEffectiveHealthPowerUp;
        _player.OnProjectileHitEvent += _scoreController.OnSuccessfulShot;
    }

    private void SpawnEnemy() {
        var enemy = _enemiesPool.Get();
        var position = _spawnPosition + new Vector3(
            Random.Range(-_spawnOffsets.x, _spawnOffsets.x),
            Random.Range(-_spawnOffsets.y, _spawnOffsets.y),
            0.0f
        );
        enemy.transform.position = position;
        enemy.gameObject.SetActive(true);
        enemy.Init(_enemiesProjectilesPool, _particlesPool, _currentGameTimeGetter, _enemiesPool.Return);

        _enemySpawnTimer -= _gameBalanceSO.EnemySpawnInterval;

        enemy.OnDieEvent += (_, deathType) => _scoreController.OnEnemyDie(deathType);
        enemy.OnDieEvent += OnEnemyDie;
        _scoreController.OnEnemySpawn();
    }

    private void OnPlayerDie() {
        _running = false;

        var playerExplosion = _particlesPool.Get(ParticleType.EXPLOSION_VFX);
        playerExplosion.Init(ParticleType.EXPLOSION_VFX, _particlesPool.Return);
        foreach (var subEmitter in playerExplosion.GetAllParticalSystems())
        {
            var subMain = subEmitter.main;
            subMain.simulationSpeed = .1f;
        }

        playerExplosion.OnParticleFinishedEvent += OnAfterPlayerExplosion;
        playerExplosion.PlayAndReturn(_player.transform.position);

        _enemiesPool.ReturnAllActivePoolables();
        _enemiesProjectilesPool.ReturnAllActivePoolables();
        _playerProjectilesPool.ReturnAllActivePoolables();
        _particlesPool.ReturnAllActivePoolables();
        _powerUpsPool.ReturnAllActivePoolables();

        _gameplayUI.FadeOut();
    }

    private void OnAfterPlayerExplosion(Particle explosionParticle) {
        foreach (var subEmitter in explosionParticle.GetAllParticalSystems())
        {
            var subMain = subEmitter.main;
            subMain.simulationSpeed = 1f;
        }

        var gameOverRenderer = _gameOverUI.GetComponent<CanvasRenderer>();
        gameOverRenderer.SetAlpha(0);
        _gameOverUI.Open();
        Tween.Custom(0, 1f, duration: 1f, onValueChange: newVal => gameOverRenderer.SetAlpha(newVal));
    }

    private void UpdatePlayerHealthOnUIAction(int currentHealth) {
        _gameplayUI.UpdateHealth(currentHealth);
    }

    private void OnEnemyDie(Enemy enemy, Enemy.DeathType deathType) {
        if (deathType == Enemy.DeathType.BY_PROJECTILE)
        {
            _powerUpsController.OnEnemyDeath(enemy.EnemyType, enemy.transform.position);
        }

        if (deathType != Enemy.DeathType.BY_PLAYER_COLLISION || _gameBalanceSO.IncreaseScoreByEnemyPlayerCollision)
        {
            _gameplayUI.AddScore(_gameBalanceSO.ScoreForKilledEnemy);
        }
    }

    private void OnEnemyDestroy(Enemy enemy) {
        enemy.gameObject.SetActive(false);
        _enemiesPool.Return(enemy);
    }

    private void OnRetry() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDrawGizmos() {
        Gizmos.DrawLine(_spawnPosition - _spawnOffsets, _spawnPosition + _spawnOffsets);

        var corners = new Vector3[]
        {
            new Vector3(_playerAllowedMovementHorizontal.x, _playerAllowedMovementVertical.x, 0),
            new Vector3(_playerAllowedMovementHorizontal.y, _playerAllowedMovementVertical.x, 0),
            new Vector3(_playerAllowedMovementHorizontal.y, _playerAllowedMovementVertical.y, 0),
            new Vector3(_playerAllowedMovementHorizontal.x, _playerAllowedMovementVertical.y, 0)
        };

        Gizmos.color = Color.yellow;
        Gizmos.DrawLineStrip(corners, true);
    }
}
