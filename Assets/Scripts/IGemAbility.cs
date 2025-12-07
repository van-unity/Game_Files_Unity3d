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
    Task Execute(List<CollectedGemInfo> collectedGems, Board board, BoardView boardView,
        CancellationToken ct = default);
}

public class BombAbility : IGemAbility {
    private const int DESTRUCTION_DELAY_MS = 250;

    private readonly Vector2Int[] _explosionOffsets = {
        // 3x3 square
        new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
        new(1, 1), new(-1, 1), new(1, -1), new(-1, -1),
        // Arms
        new(2, 0), new(-2, 0), new(0, 2), new(0, -2)
    };

    public async Task Execute(List<CollectedGemInfo> collectedGems, Board board, BoardView boardView,
        CancellationToken ct = default) {
        var destroyPositions = new List<Vector2Int>();
        var bombPositions = new HashSet<Vector2Int>(); // Use HashSet for fast lookup
        
        foreach (var info in collectedGems) {
            if (info.gem != null && info.gem.Type == GemType.Bomb) {
                bombPositions.Add(info.position);
            }
        }

        //identify valid neighbors
        foreach (var bombPos in bombPositions) {
            foreach (var offset in _explosionOffsets) {
                var targetPos = bombPos + offset;
                
                if (bombPositions.Contains(targetPos)) continue;
                
                var targetGem = board.GetAt(targetPos);
                if (board.IsValidPos(targetPos) && targetGem != null) {
                    destroyPositions.Add(targetPos);
                }
            }
        }
        
        var neighborInfos = destroyPositions
            .Distinct()
            .Select(p => new CollectedGemInfo(p, board.GetAt(p)))
            .ToList();
        
        foreach (var info in neighborInfos) {
            board.SetAt(info.position, null);
        }
        
        await boardView.DestroyMatches(neighborInfos.Select(n => n.position), ct);
        await Task.Delay(DESTRUCTION_DELAY_MS, ct);
        
        var bombInfos = collectedGems
            .Where(c => c.gem.Type == GemType.Bomb)
            .ToList();

        await boardView.DestroyMatches(bombInfos.Select(b => b.position), ct);
    }
}