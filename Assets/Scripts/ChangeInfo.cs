using UnityEngine;

public class ChangeInfo {
    public Gem gem;
    public bool wasCreated;
    public int creationTime;
    public Vector2Int fromPos;
    public Vector2Int toPos;

    public ChangeInfo(Gem gem, bool wasCreated, int creationTime, Vector2Int fromPos,
        Vector2Int toPos) {
        this.gem = gem;
        this.wasCreated = wasCreated;
        this.creationTime = creationTime;
        this.fromPos = fromPos;
        this.toPos = toPos;
    }
}