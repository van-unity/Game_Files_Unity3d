using System.Threading.Tasks;
using UnityEngine;

public interface IGemAbility {
    Task Execute(GameBoard board);
}

public class BombAbility : IGemAbility {
    private readonly Vector2Int[] _explosionOffsets = {
        new(0, 0), // The Bomb itself

        // 3x3 square (all neighbors)
        new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
        new(1, 1), new Vector2Int(-1, 1), new(1, -1), new(-1, -1),

        // Outer orthogonal tiles (the "arms" of the cross)
        new(2, 0), new(-2, 0), new(0, 2), new(0, -2)
    };


    public Task Execute(GameBoard board) {
        return Task.CompletedTask;
    }
}