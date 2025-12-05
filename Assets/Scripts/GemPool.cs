using System.Collections.Generic;
using UnityEngine;

public class GemPool {
    private const int POOL_SIZE = 50;
    private const int POOL_GROW_AMOUNT = 10;

    private readonly Dictionary<GemType, GameObjectPool> _gemPools;
    private readonly Dictionary<GemType, GameObjectPool> _destroyEffectPools;
    private readonly GemPrefabRepository _gemPrefabRepository;

    public GemPool(GemPrefabRepository gemPrefabRepository) {
        _gemPrefabRepository = gemPrefabRepository;
        //no pool for GemType.None 
        _gemPools = new Dictionary<GemType, GameObjectPool>(GemTypeExtensions.GemTypeCount - 1);
        _destroyEffectPools = new Dictionary<GemType, GameObjectPool>(GemTypeExtensions.GemTypeCount - 1);
        Initialize();
    }

    public GameObject GetGem(GemType gemType) {
        if (!_gemPools.TryGetValue(gemType, out var pool)) {
            throw new KeyNotFoundException($"Gem {gemType} not found");
        }

        return pool.Get();
    }

    public void ReleaseGem(GemType gemType, GameObject gem) {
        if (!_gemPools.TryGetValue(gemType, out var pool)) {
            throw new KeyNotFoundException($"Gem {gemType} not found");
        }

        pool.Release(gem);
    }

    public GameObject GetDestroyEffect(GemType gemType) {
        if (!_destroyEffectPools.TryGetValue(gemType, out var pool)) {
            throw new KeyNotFoundException($"Gem {gemType} not found");
        }

        return pool.Get();
    }

    public void ReleaseDestroyEffect(GemType gemType, GameObject destroyEffect) {
        if (!_destroyEffectPools.TryGetValue(gemType, out var pool)) {
            throw new KeyNotFoundException($"Gem {gemType} not found");
        }

        pool.Release(destroyEffect);
    }

    private void Initialize() {
        foreach (var gemType in GemTypeExtensions.AllGemTypes) {
            if (gemType == GemType.none) {
                continue;
            }

            var prefab = _gemPrefabRepository.GetGemPrefab(gemType);
            if (prefab != null) {
                var pool = new GameObjectPool(prefab, POOL_SIZE, POOL_GROW_AMOUNT);
                pool.WarmUp();
                _gemPools.Add(gemType, pool);
            }

            var destroyEffect = _gemPrefabRepository.GetDestroyEffect(gemType);
            if (destroyEffect != null && destroyEffect.DestroyEffect != null) {
                var pool = new GameObjectPool(destroyEffect.DestroyEffect, POOL_SIZE, POOL_GROW_AMOUNT);
                pool.WarmUp();
                _destroyEffectPools.Add(gemType, pool);
            }
        }
    }
}