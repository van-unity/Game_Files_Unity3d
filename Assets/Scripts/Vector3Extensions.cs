using UnityEngine;

public static class Vector3Extensions {
    public static Vector2Int Vector2Int(this Vector3 vector3) =>
        new(Mathf.RoundToInt(vector3.x), Mathf.RoundToInt(vector3.y));
}