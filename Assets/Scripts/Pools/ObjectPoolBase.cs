using System.Collections.Generic;
using UnityEngine;

namespace Pools {
    public interface ISpawnable<T> where T: ISpawnable<T> {
        void Initialize(IObjectPool<T>  pool);
        void OnSpawn();
        void OnDespawn();
        void Return();
        void Destroy();
    }

    public interface IObjectPool<T> where T : ISpawnable<T>{
        void WarmUp();
        T Get();
        void Return(T obj);
        void Clear();
    }
    
    public abstract class ObjectPoolBase<T> : IObjectPool<T> where T : ISpawnable<T> {
        private readonly Queue<T> _pool;
        private readonly int _size;
        private readonly int _growAmount;

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
            obj.OnSpawn();
            OnGet(obj);
            return obj;
        }

        protected virtual void OnGet(T item) {
        
        }
    
        public void Return(T obj) {
            if (obj == null) {
                return;
            }
        
            _pool.Enqueue(obj);
            obj.OnDespawn();
            OnReturn(obj);
        }

        protected virtual void OnReturn(T obj) {
        
        }
    
        public void Clear() {
            while (_pool.Count > 0) {
                var obj = _pool.Dequeue();
                obj.Destroy();
            }
        }
    
        public void WarmUp() {
            for (int i = 0; i < _size; i++) {
                var obj = Create();
                obj.Initialize(this);
                _pool.Enqueue(obj);
            }
        }
    
        private void GrowPool() {
            for (int i = 0; i < _growAmount; i++) {
                var obj = Create();
                obj.Initialize(this);
                _pool.Enqueue(obj);
            }
        }

        protected abstract T Create();
    }
}