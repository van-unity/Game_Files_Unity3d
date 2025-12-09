using UnityEngine;

namespace Pools {
    public abstract class SpawnableMonoBehaviour<T> : MonoBehaviour, ISpawnable<T> where T : SpawnableMonoBehaviour<T>, ISpawnable<T> {
        protected IObjectPool<T>  _pool;
        
        public GameObject GameObject { get; private set; }
        public Transform Transform { get; private set; }
        
        public virtual void Initialize(IObjectPool<T> pool) {
            _pool = pool;
            GameObject = gameObject;
            Transform = GameObject.transform;
        }
        
        public virtual void OnSpawn() {
            GameObject.SetActive(true);
        }
        
        public virtual void OnDespawn() {
            GameObject.SetActive(false);
        }
        
        public virtual void Return() {
            _pool.Return((T)this);
        }
        
        public virtual void Destroy() {
            Destroy(GameObject);
        }
    }
}