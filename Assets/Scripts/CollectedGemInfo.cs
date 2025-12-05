using UnityEngine;

public readonly struct CollectedGemInfo {
    public readonly Vector2Int position;
    public readonly GemType gemType;

    public CollectedGemInfo(Vector2Int position, GemType gemType) {
        this.position = position;
        this.gemType = gemType;
    }
}