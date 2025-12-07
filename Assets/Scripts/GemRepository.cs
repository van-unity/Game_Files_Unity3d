using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Match3/Gem Repository", fileName = "Gem-Repository")]
public class GemRepository : ScriptableObject {
    [SerializeField] private GemPrefabConfiguration[] _gemPrefabs;

    public IEnumerable<GemPrefabConfiguration> AllGems() => _gemPrefabs;

    public GemPrefabConfiguration GetConfig(GemColor color, GemType type) {
        foreach (var config in _gemPrefabs) {
            if (config.Color == color && config.Type == type) {
                return config;
            }
        }

        return null;
    }

    public GemPrefabConfiguration GetRandomRegularGem() {
        var regularGems = AllGems().Where(g => g.Type == GemType.Regular).ToArray();
        return regularGems[Random.Range(0, regularGems.Length)];
    }
}