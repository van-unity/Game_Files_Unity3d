using UnityEngine;

public class GameObjectPool : ObjectPoolBase<GameObject> {
    private readonly GameObject _prefab;
    private readonly Transform _container;

    public GameObjectPool(GameObject prefab, int size, int growAmount) : base(size, growAmount) {
        _prefab = prefab;
        _container = new GameObject($"{prefab.name}-POOL").transform;
    }

    protected override void OnSpawn(GameObject obj) {
        obj.transform.SetParent(null);
        obj.SetActive(true);
    }

    protected override void OnDespawn(GameObject obj) {
        obj.transform.SetParent(_container);
        obj.SetActive(false);
    }

    protected override void Destroy(GameObject obj) {
        Object.Destroy(obj);
    }

    protected override void OnInitialize(GameObject obj) {
        obj.transform.SetParent(_container);
        obj.SetActive(false);
    }

    protected override GameObject Create() => Object.Instantiate(_prefab, _container);

    protected override void GrowPool() {
        base.GrowPool();
        Debug.LogError($"growing pool: {_prefab.name}");
    }
}