using System;
using UnityEngine;

public readonly struct CollectedGemInfo : IEquatable<CollectedGemInfo> {
    public readonly Vector2Int position;
    public readonly Gem gem;

    public CollectedGemInfo(Vector2Int position, Gem gem) {
        this.position = position;
        this.gem = gem;
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