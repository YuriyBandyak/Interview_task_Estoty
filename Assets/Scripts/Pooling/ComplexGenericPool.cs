using System;
using System.Collections.Generic;
using UnityEngine;

public class ComplexGenericPool<T, TType> : MonoBehaviour
    where TType : Enum
    where T : class {
    [SerializeField] private List<SerializableKeyValuePair<TType, GenericFactory<T>>> _factories;

    private Dictionary<TType, Queue<T>> _queuesPerType;

    public void Init() {
        _queuesPerType = new Dictionary<TType, Queue<T>>();
        foreach (var kvp in _factories)
        {
            _queuesPerType.Add(kvp.Key, new Queue<T>());
        }
    }

    public T Get(TType type) {
        if (_queuesPerType.TryGetValue(type, out Queue<T> queue) && queue.TryDequeue(out T poolable))
        {
            return poolable;
        }
        else
        {
            return GetNew(type);
        }
    }

    public void Return(T poolable, TType type) {
        _queuesPerType[type].Enqueue(poolable);
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
