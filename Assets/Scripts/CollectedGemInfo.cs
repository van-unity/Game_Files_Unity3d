using System;
using UnityEngine;

public struct CollectedGemInfo : IEquatable<CollectedGemInfo> {
    public Vector2Int position;
    public Gem gem;
    public int destroyDelay;

    public CollectedGemInfo(Vector2Int position, Gem gem, int destroyDelay = 0) {
        this.position = position;
        this.gem = gem;
        this.destroyDelay = destroyDelay;
    }

    public bool Equals(CollectedGemInfo other) {
        return position.Equals(other.position) && Equals(gem, other.gem);
    }

    public override bool Equals(object obj) {
        return obj is CollectedGemInfo other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(position, gem);
    }
}