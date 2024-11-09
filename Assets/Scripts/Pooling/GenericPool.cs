using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GenericPool : MonoBehaviour {
    public abstract void Init();
    public abstract void ReturnAllActivePoolables();
}

public abstract class GenericPool<T> : GenericPool
    where T : IPoolable {

    [SerializeField] private GenericFactory<T> _factory;

    private Queue<T> _poolableObjects;
    private HashSet<T> _activePoolableObjects;

    public override void Init() {
        OnInit();
        _poolableObjects = new Queue<T>();
        _activePoolableObjects = new HashSet<T>();
    }

    public T Get() {
        if (_poolableObjects.TryDequeue(out T result))
        {
            _activePoolableObjects.Add(result);
            return result;
        }
        else
        {
            var newPoolable = GetNew();
            _activePoolableObjects.Add(newPoolable);
            return newPoolable;
        }
    }

    public void Return(T poolable) {
        _poolableObjects.Enqueue(poolable);
        _activePoolableObjects.Remove(poolable);
    }

    public override void ReturnAllActivePoolables() {
        foreach (var poolable in _activePoolableObjects.ToList())
        {
            poolable.ReturnPoolable();
        }
        _activePoolableObjects.Clear();
    }

    protected virtual void OnInit() { }


    private T GetNew() {
        return _factory.GetNew();
    }
}
