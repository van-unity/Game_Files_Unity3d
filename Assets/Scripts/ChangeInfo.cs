using UnityEngine;

public class ChangeInfo {
    public readonly GemType gemType;
    public readonly bool wasCreated;
    public readonly int creationTime;
    public readonly Vector2Int fromPos;
    public readonly Vector2Int toPos;

    public ChangeInfo(GemType gemType, bool wasCreated, int creationTime, Vector2Int fromPos,
        Vector2Int toPos) {
        this.gemType = gemType;
        this.wasCreated = wasCreated;
        this.creationTime = creationTime;
        this.fromPos = fromPos;
        this.toPos = toPos;
    }
}