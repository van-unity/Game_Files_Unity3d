using UnityEngine;

public readonly struct CollectedGemInfo {
    public readonly Vector2Int position;
    public readonly Gem gem;

    public CollectedGemInfo(Vector2Int position, Gem gem) {
        this.position = position;
        this.gem = gem;
    }
}