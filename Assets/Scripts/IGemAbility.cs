using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GemAbilityProvider {
    private readonly Dictionary<GemType, IGemAbility> _gemAbilities;

    public GemAbilityProvider() {
        _gemAbilities = new Dictionary<GemType, IGemAbility>() {
            { GemType.Bomb, new BombAbility() }
        };
    }

    public IGemAbility GetGemAbility(GemType gemType) => _gemAbilities.GetValueOrDefault(gemType);
}

public interface IGemAbility {
    Task Execute(List<CollectedGemInfo> collectedGems, GameBoard board, SC_GameLogic gameLogic,
        CancellationToken ct = default);
}

public class BombAbility : IGemAbility {
    private const int DESTRUCTION_DELAY_MS = 500;

    private readonly Vector2Int[] _explosionOffsets = {
        // 3x3 square (all neighbors)
        new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
        new(1, 1), new(-1, 1), new(1, -1), new(-1, -1),

        // Outer orthogonal tiles (the "arms" of the cross)
        new(2, 0), new(-2, 0), new(0, 2), new(0, -2)
    };

    public async Task Execute(List<CollectedGemInfo> collectedGems, GameBoard board, SC_GameLogic gameLogic,
        CancellationToken ct = default) {
        var destroyPositions = new List<Vector2Int>();
        var bombPositions = new List<Vector2Int>();

        foreach (var collectedGemInfo in collectedGems) {
            if (collectedGemInfo.gem.Type != GemType.Bomb) {
                continue;
            }

            bombPositions.Add(collectedGemInfo.position);

            var positions = _explosionOffsets
                .Select(p => p + collectedGemInfo.position)
                .Where(p => board.IsValidPos(p) && board.GetAt(p) != null)
                .ToList();

            destroyPositions.AddRange(positions);
        }

        var collectedInfos = destroyPositions
            .Except(bombPositions)
            .Distinct()
            .Select(p => new CollectedGemInfo(p, board.GetAt(p)))
            .ToList();

        foreach (var info in collectedInfos) {
            board.SetAt(info.position, null);
        }

        await gameLogic.DestroyMatches(collectedInfos, ct);

        await Task.Delay(DESTRUCTION_DELAY_MS, ct);
        
        var bombInfos = bombPositions
            .Select(p => new CollectedGemInfo(p, board.GetAt(p)))
            .ToList();

        foreach (var info in bombInfos) {
            board.SetAt(info.position, null);
        }
        
        await gameLogic.DestroyMatches(bombInfos, ct);
        
        collectedInfos.AddRange(collectedGems);
        collectedInfos.AddRange(bombInfos);
    }
}