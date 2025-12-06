using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Match3/Gem Prefab Repository", fileName = "Gem-Prefab-Repository")]
public class GemPrefabRepository : ScriptableObject {
    [SerializeField] private GemPrefabConfiguration[] _gemPrefabs;

    public IEnumerable<GemPrefabConfiguration> IteratePrefabs() => _gemPrefabs;

    public GemPrefabConfiguration GetConfig(GemColor color, GemType type) {
        foreach (var config in _gemPrefabs) {
            if (config.Color == color && config.Type == type) {
                return config;
            }
        }

        return null;
    }
}