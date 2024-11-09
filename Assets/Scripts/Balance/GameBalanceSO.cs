using UnityEngine;

[CreateAssetMenu(menuName = "Balance/" + nameof(GameBalanceSO))]
public class GameBalanceSO : ScriptableObject {
    [SerializeField] private float _enemySpawnInterval;
    [SerializeField] private int _scoreForKilledEnemy;
    [SerializeField] private bool _increaseScoreByEnemyPlayerCollision;

    public float EnemySpawnInterval => _enemySpawnInterval;
    public int ScoreForKilledEnemy => _scoreForKilledEnemy;
    public bool IncreaseScoreByEnemyPlayerCollision => _increaseScoreByEnemyPlayerCollision;
}