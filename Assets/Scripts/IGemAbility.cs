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
    private const int DESTRUCTION_DELAY_MS = 250; // Reduced for snappier feel

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

        // 1. Identify Bombs
        foreach (var info in collectedGems) {
            if (info.gem != null && info.gem.Type == GemType.Bomb) {
                bombPositions.Add(info.position);
            }
        }

        // 2. Identify Valid Neighbors
        foreach (var bombPos in bombPositions) {
            foreach (var offset in _explosionOffsets) {
                var targetPos = bombPos + offset;

                // Skip if it's another bomb in the current match set (handled later)
                if (bombPositions.Contains(targetPos)) continue;

                // CRITICAL FIX: Check if the gem exists before adding it.
                // If GetAt returns null, it was already cleared by the match logic.
                var targetGem = board.GetAt(targetPos);
                if (board.IsValidPos(targetPos) && targetGem != null) {
                    destroyPositions.Add(targetPos);
                }
            }
        }

        // 3. Collect Neighbors
        var neighborInfos = destroyPositions
            .Distinct()
            .Select(p => new CollectedGemInfo(p, board.GetAt(p)))
            .ToList();

        // Clear neighbors from board immediately
        foreach (var info in neighborInfos) {
            board.SetAt(info.position, null);
        }

        // 4. Visuals: Destroy Neighbors
        await boardView.DestroyMatches(neighborInfos.Select(n => n.position), ct);
        await Task.Delay(DESTRUCTION_DELAY_MS, ct);

        // 5. Visuals: Destroy Bombs (They were already cleared from model in TryResolveMatches)
        // We reconstruct the info to ensure we pass valid data to DestroyMatches
        var bombInfos = collectedGems
            .Where(c => c.gem.Type == GemType.Bomb)
            .ToList();

        await boardView.DestroyMatches(bombInfos.Select(b => b.position), ct);
    }
}