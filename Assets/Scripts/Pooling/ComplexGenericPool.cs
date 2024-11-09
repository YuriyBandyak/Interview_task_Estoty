using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComplexGenericPool<T, TType> : GenericPool
    where TType : Enum
    where T : class, IPoolable {
    [SerializeField] private List<SerializableKeyValuePair<TType, GenericFactory<T>>> _factories;

    private Dictionary<TType, Queue<T>> _queuesPerType;
    private HashSet<T> _activePoolableObjects;

    public override void Init() {
        _queuesPerType = new Dictionary<TType, Queue<T>>();
        foreach (var kvp in _factories)
        {
            _queuesPerType.Add(kvp.Key, new Queue<T>());
        }
        _activePoolableObjects = new HashSet<T>();
    }

    public T Get(TType type) {
        if (_queuesPerType.TryGetValue(type, out Queue<T> queue) && queue.TryDequeue(out T poolable))
        {
            _activePoolableObjects.Add(poolable);
            return poolable;
        }
        else
        {
            var newPoolable = GetNew(type);
            _activePoolableObjects.Add(newPoolable);
            return newPoolable;
        }
    }

    public void Return(T poolable, TType type) {
        _queuesPerType[type].Enqueue(poolable);
    }

    public override void ReturnAllActivePoolables() {
        foreach (var poolable in _activePoolableObjects.ToList())
        {
            poolable.ReturnPoolable();
        }
        _activePoolableObjects.Clear();
    }

    private T GetNew(TType type) {
        foreach (var kvp in _factories)
        {
            if (kvp.Key.Equals(type))
            {
                return kvp.Value.GetNew();
            }
        }

        Debug.LogError($"Trying to get {typeof(T)} of type {type}. There is no such factory for this type");
        return null;
    }
}
