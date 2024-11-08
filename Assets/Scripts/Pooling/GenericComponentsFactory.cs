using UnityEngine;

public abstract class GenericComponentsFactory<T> : GenericFactory<T>
    where T : Component {
    [SerializeField] private T _prefab;

    public override T GetNew() {
        return Object.Instantiate(_prefab, transform);
    }
}
