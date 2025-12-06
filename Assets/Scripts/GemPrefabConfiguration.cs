using UnityEngine;

[CreateAssetMenu(fileName = "Gem-Prefab-Configuration", menuName = "Match3/Gem Prefab Configuration")]
public class GemPrefabConfiguration : ScriptableObject {
    [field: SerializeField] public GemColor Color { get; private set; }
    [field: SerializeField] public GemType Type { get; private set; }
    [field: SerializeField] public GemView GemPrefab { get; private set; }
    [field: SerializeField] public GemDestroyEffect DestroyEffect { get; private set; }
}