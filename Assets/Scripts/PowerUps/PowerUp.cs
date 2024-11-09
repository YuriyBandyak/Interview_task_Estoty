using System;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class PowerUp : MonoBehaviour, IPoolable {

    private const float OutOfScreenOffset = .1f;

    [SerializeField] private PowerUpType _type;

    private float _speed = 3.0f;

    private event Action<PowerUp> _onPowerUpPickedUp;
    private event Action _returnToPoolAction;

    public PowerUpType PowerUpType => _type;

    public void Init(PowerUpType type, Action<PowerUp> onPowerUpPickedUp, Action<PowerUp, PowerUpType> ReturnToPoolAction, float speed) {
        _type = type;
        this._onPowerUpPickedUp = onPowerUpPickedUp;
        _speed = speed;
        _returnToPoolAction = () => ReturnToPoolAction.Invoke(this, type);
        gameObject.SetActive(true);
    }

    public void ReturnPoolable() {
        gameObject.SetActive(false);
        _returnToPoolAction.Invoke();
    }

    private void Update() {

        Move();
        if (transform.IsOutOfScreen(OutOfScreenOffset))
        {
            ReturnPoolable();
        }
    }

    private void Move() {
        var p = transform.position;
        p += Vector3.down * (_speed * Time.deltaTime);
        transform.position = p;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent<Player>(out var player))
        {
            OnPickedUp();
        }
    }

    private void OnPickedUp() {
        _onPowerUpPickedUp.Invoke(this);
        ReturnPoolable();
    }
}
