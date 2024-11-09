using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Particle : MonoBehaviour, IPoolable {

    [SerializeField] private ParticleSystem _particleSystemMain;
    [SerializeField] private ParticleSystem[] _particleSystemsSub;

    public event Action<Particle> OnParticleFinishedEvent;
    private event Action _returnToPoolAction;

    public ParticleSystem[] GetAllParticalSystems() {
        var allSystems = new ParticleSystem[(_particleSystemsSub?.Length ?? 0) + 1];
        allSystems[0] = _particleSystemMain;
        if (_particleSystemsSub != null)
        {
            for (int i = 0; i < _particleSystemsSub.Length; i++)
            {
                allSystems[i + 1] = _particleSystemsSub[i];
            }
        }
        return allSystems;
    }

    public void Init(ParticleType particleType, Action<Particle, ParticleType> ReturnToPoolAction) {
        _returnToPoolAction = () => ReturnToPoolAction(this, particleType);
        gameObject.SetActive(true);
    }

    public void ReturnPoolable() {
        _returnToPoolAction.Invoke();
    }

    public void PlayAndReturn(Vector3 position) {
        transform.position = position;
        _particleSystemMain.Play();
        WaitTillExplosionEnd(_particleSystemMain);
    }

    private async UniTask WaitTillExplosionEnd(ParticleSystem fx) {
        await UniTask.Delay(Mathf.RoundToInt(fx.main.duration * 1000f * _particleSystemMain.main.simulationSpeed));
        await UniTask.WaitUntil(() => fx.particleCount == 0);
        gameObject.SetActive(false);

        OnParticleFinishedEvent?.Invoke(this);
        ReturnPoolable();
    }
}