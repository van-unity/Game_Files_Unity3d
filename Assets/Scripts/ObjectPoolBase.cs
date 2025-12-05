using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectPoolBase<T> {
    private readonly Queue<T> _pool;
    private readonly int _size;
    private readonly int _growAmount;
    private readonly Transform _container;

    protected ObjectPoolBase(int size = 30, int growAmount = 10) {
        _size = size;
        _growAmount = growAmount;
        _pool = new Queue<T>(size);
    }

    public T Get() {
        if (_pool.Count == 0) {
            GrowPool();
        }

        var obj = _pool.Dequeue();
        OnSpawn(obj);
        return obj;
    }

    protected abstract void OnSpawn(T obj);
    
    public void Release(T obj) {
        if (obj == null) {
            return;
        }
        
        _pool.Enqueue(obj);
        OnDespawn(obj);
    }

    protected abstract void OnDespawn(T obj);

    public void Clear() {
        while (_pool.Count > 0) {
            var obj = _pool.Dequeue();
            Destroy(obj);
        }
    }

    protected abstract void Destroy(T obj);
    
    protected virtual void GrowPool() {
        for (int i = 0; i < _growAmount; i++) {
            var obj = Create();
            _pool.Enqueue(obj);
            OnInitialize(obj);
        }
    }

    protected abstract void OnInitialize(T obj);
    
    public void WarmUp() {
        for (int i = 0; i < _size; i++) {
            var obj = Create();
            _pool.Enqueue(obj);
            OnInitialize(obj);
        }
    }

    protected abstract T Create();
}