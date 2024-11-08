using System;
using UnityEngine;

public class Projectile : MonoBehaviour {

    private const float OutOfScreenOffset = .1f;

    [SerializeField] private float _speed = 0.0f;
    [SerializeField] private Vector3 _direction = Vector3.up;
    private int _damage = 1;

    private event Action<Projectile> OnDestroyAction;

    public void Init(int damage, Action<Projectile> OnDestroyAction) {
        _damage = damage;
        this.OnDestroyAction = OnDestroyAction;
    }

    void Update() {

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
            OnDie();
        }
    }

    private void OnTriggerEnter(Collider other) {

        bool destroy = false;
        var enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {

            enemy.OnProjectileHit(_damage);
            destroy = true;
        }
        else
        {
            var player = other.GetComponent<Player>();
            if (player != null)
            {

                player.Hit();
                destroy = true;
            }
        }

        if (destroy)
        {
            OnDie();
        }
    }

    private void OnDie() {
        var trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.Clear();
        OnDestroyAction.Invoke(this);
    }
}
