using UnityEngine;

public class ChangeInfo {
    public readonly Gem gem;
    public readonly bool wasCreated;
    public readonly int creationTime;
    public readonly Vector2Int fromPos;
    public readonly Vector2Int toPos;

    public ChangeInfo(Gem gem, bool wasCreated, int creationTime, Vector2Int fromPos,
        Vector2Int toPos) {
        this.gem = gem;
        this.wasCreated = wasCreated;
        this.creationTime = creationTime;
        this.fromPos = fromPos;
        this.toPos = toPos;
    }
}