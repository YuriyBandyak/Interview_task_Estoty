using System.Collections.Generic;
using UnityEngine;

public abstract class GenericPool<T> : MonoBehaviour {

    [SerializeField] private GenericFactory<T> _factory;

    private Queue<T> _poolableObjects;

    public void Init() {
        OnInit();
    }

    protected virtual void OnInit() {
        _poolableObjects = new Queue<T>();
    }

    public T Get() {
        if (_poolableObjects.TryDequeue(out T result))
        {
            return result;
        }
        else
        {
            return GetNew();
        }
    }

    public void Return(T poolable) {
        _poolableObjects.Enqueue(poolable);
    }

    private T GetNew() {
        return _factory.GetNew();
    }
}
