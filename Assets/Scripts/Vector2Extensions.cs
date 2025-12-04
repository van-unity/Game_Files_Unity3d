using UnityEngine;

public static class Vector2Extensions {
    public static Vector2Int ToVector2Int(this Vector2 vector2) =>
        new(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.y));
}