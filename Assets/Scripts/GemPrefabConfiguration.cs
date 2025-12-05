using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Gem-Prefab-Configuration", menuName = "Match3/Gem Prefab Configuration")]
public class GemPrefabConfiguration : ScriptableObject {
    [field: SerializeField] public GemType GemType { get; private set; }
    [field: SerializeField] public GameObject GemPrefab { get; private set; }
    [field: SerializeField] public DestroyEffectConfiguration DestroyEffectConfiguration { get; private set; }
}

[Serializable]
public class DestroyEffectConfiguration {
    [field: SerializeField] public GameObject DestroyEffect { get; private set; }
    [field: SerializeField] public float DestroyEffectDuration { get; private set; }
}