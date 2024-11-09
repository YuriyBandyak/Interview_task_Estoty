using System;
using UnityEngine;

public class Projectile : MonoBehaviour, IPoolable {

    private const float OutOfScreenOffset = .1f;

    [SerializeField] private float _speed = 0.0f;
    [SerializeField] private Vector3 _direction = Vector3.up;
    private int _damage = 1;

    private event Action _onHitAction;
    private event Action _returnToPoolAction;

    public void Init(int damage, Action<Projectile> ReturnToPoolAction, Action OnHitAction = null) {
        _damage = damage;
        _returnToPoolAction = () => ReturnToPoolAction(this);
        this._onHitAction = OnHitAction;
        gameObject.SetActive(true);
    }

    public void ReturnPoolable() {
        gameObject.SetActive(false);
        _returnToPoolAction.Invoke();
    }

    private void Update() {

        Move();
        CheckBounds();
    }

    private void Move() {
        var p = transform.position;
        p += _direction * (_speed * Time.deltaTime);
        transform.position = p;
    }

    private void CheckBounds() {
        if (transform.IsOutOfScreen(OutOfScreenOffset))
        {
            Destroy();
        }
    }

    private void OnTriggerEnter(Collider other) {

        bool destroy = false;
        if (other.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.OnProjectileHit(_damage);
            destroy = true;
        }
        else if (other.TryGetComponent<Player>(out var player))
        {
            player.Hit();
            destroy = true;
        }

        if (destroy)
        {
            Die();
        }
    }

    private void Die() {
        _onHitAction?.Invoke();
        Destroy();
    }

    private void Destroy() {
        var trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.Clear();
        ReturnPoolable();
    }
}
