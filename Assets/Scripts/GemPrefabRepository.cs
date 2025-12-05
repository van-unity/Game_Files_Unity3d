using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Match3/Gem Prefab Repository", fileName = "Gem-Prefab-Repository")]
public class GemPrefabRepository : ScriptableObject {
    [SerializeField] private GemPrefabConfiguration[] _gemPrefabs;

    private Dictionary<GlobalEnums.GemType, GemPrefabConfiguration> _prefabsLookup;

    private void OnEnable() {
        if (_gemPrefabs == null) {
            return;
        }

        _prefabsLookup = _gemPrefabs.ToDictionary(gemPrefab => gemPrefab.GemType, gemPrefab => gemPrefab);
    }

    public GameObject GetGemPrefab(GlobalEnums.GemType gemType) {
        if (!_prefabsLookup.TryGetValue(gemType, out var prefabConfig)) {
            return null;
        }

        return prefabConfig.GemPrefab;
    }

    public DestroyEffectConfiguration GetDestroyEffect(GlobalEnums.GemType gemType) {
        if (!_prefabsLookup.TryGetValue(gemType, out var prefabConfig)) {
            return null;
        }

        return prefabConfig.DestroyEffectConfiguration;
    }
}