using UnityEngine;

namespace Pools {
    public class MonoBehaviourObjectPool<T> : ObjectPoolBase<T> where T : SpawnableMonoBehaviour<T> {
        private readonly T _prefab;
        private readonly Transform _container;

        public MonoBehaviourObjectPool(T prefab, int size, int growAmount) : base(size, growAmount) {
            _prefab = prefab;
            _container = new GameObject($"{prefab.name}-POOL").transform;
        }

        protected override T Create() {
            var item =  Object.Instantiate(_prefab, _container);
            item.gameObject.SetActive(false);
            return item;
        }

        protected override void OnReturn(T obj) {
            base.OnReturn(obj);
            obj.Transform.SetParent(_container);
        }
    }
}