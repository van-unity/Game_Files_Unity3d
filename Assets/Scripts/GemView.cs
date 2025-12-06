using Pools;
using UnityEngine;

public class GemView : SpawnableMonoBehaviour<GemView> {
    [field: SerializeField] public GameObject DestroyEffect { get; private set; }
    
    public Gem Gem { get; private set; }

    public void Bind(Gem gem) {
        Gem = gem;
    }
}