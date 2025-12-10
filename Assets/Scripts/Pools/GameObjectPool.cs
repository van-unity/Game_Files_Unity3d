using UnityEngine;
using Zenject;

namespace Pools {
    public class GameObjectPool : ObjectPoolBase<SpawnableGameObject> {
        private readonly SpawnableGameObject _prefab;
        private readonly Transform _container;
        
        public GameObjectPool(SpawnableGameObject prefab, int size, int growAmount) : base(size, growAmount) {
            _prefab = prefab;
            _container = new GameObject($"{prefab.name}-POOL").transform;
        }
        
        protected override SpawnableGameObject Create() {
            var item = Object.Instantiate(_prefab, _container);
            item.gameObject.SetActive(false);
            return item;
        }

        protected override void OnReturn(SpawnableGameObject obj) {
            base.OnReturn(obj);
            obj.Transform.SetParent(_container);
        }
    }
}