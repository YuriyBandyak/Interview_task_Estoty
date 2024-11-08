using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {

    [SerializeField] private GameBalanceSO _gameBalanceSO;
    [SerializeField] private PowerUpsController _powerUpsController;
    [SerializeField] private Player _player;
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

    void Awake() {
        Application.targetFrameRate = 60;
    }

    void Start() {
        _enemiesPool.Init();
        _enemiesProjectilesPool.Init();
        _playerProjectilesPool.Init();
        _particlesPool.Init();
        _powerUpsPool.Init();

        _player.Init(_particlesPool, _playerProjectilesPool, OnPlayerDie, UpdatePlayerHealthOnUIAction, _playerAllowedMovementHorizontal, _playerAllowedMovementVertical);
        _powerUpsController.Init(_powerUpsPool, _player.GetParameterModifiers());

        _running = true;
        var gameStartTime = Time.time;
        _currentGameTimeGetter = () => Time.time - gameStartTime;

        _gameOverUI.Init(OnRetry);
        _gameOverUI.Close();
    }

    void Update() {
        if (!_running) return;
        _enemySpawnTimer += Time.deltaTime;
        if (_enemySpawnTimer >= _gameBalanceSO.EnemySpawnInterval)
        {
            SpawnEnemy();
        }
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
        enemy.Init(_enemiesProjectilesPool, _particlesPool, OnEnemyDie, OnEnemyDestroy, _currentGameTimeGetter);

        _enemySpawnTimer -= _gameBalanceSO.EnemySpawnInterval;
    }

    private void OnPlayerDie() {
        _gameOverUI.Open();
        _running = false;
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
