using System;
using UnityEngine;

public partial class PowerUp : MonoBehaviour {

    private const float OutOfScreenOffset = .1f;

    private float _speed = 3.0f;

    [SerializeField] private PowerUpType _type;

    private event Action<PowerUp> _onPowerUpDestoyedAction;
    private event Action<PowerUp> _onPowerUpPickedUp;

    public PowerUpType PowerUpType => _type;

    public void Init(PowerUpType type, Action<PowerUp> OnPowerUpDestoyedAction, Action<PowerUp> onPowerUpPickedUp) {
        _type = type;
        this._onPowerUpDestoyedAction = OnPowerUpDestoyedAction;
        this._onPowerUpPickedUp = onPowerUpPickedUp;
    }

    private void Update() {

        Move();
        if (transform.IsOutOfScreen(OutOfScreenOffset))
        {
            Destroy();
        }
    }

    private void Move() {
        var p = transform.position;
        p += Vector3.down * (_speed * Time.deltaTime);
        transform.position = p;
    }

    private void OnTriggerEnter(Collider other) {

        var player = other.GetComponent<Player>();
        if (player == null) return;

        OnPickedUp(player);
    }

    private void OnPickedUp(Player player) {
        //player.AddPowerUp(_type);
        _onPowerUpPickedUp.Invoke(this);
        Destroy();
    }

    private void Destroy() {
        _onPowerUpDestoyedAction.Invoke(this);
    }
}
