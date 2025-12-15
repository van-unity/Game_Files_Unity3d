using System.Collections.Generic;
using UnityEngine;

public class GemAbilityProvider {
    private readonly Dictionary<GemType, IGemAbility> _gemAbilities;

    public GemAbilityProvider() {
        _gemAbilities = new Dictionary<GemType, IGemAbility>() {
            { GemType.Regular , new RegularGemAbility() },
            { GemType.Bomb, new BombAbility() }
        };
    }

    public IGemAbility GetGemAbility(GemType gemType) => _gemAbilities.GetValueOrDefault(gemType);
}

public interface IGemAbility {
    void Execute(Board board, CollectedGemInfo self, HashSet<CollectedGemInfo> collectedGems);
}

public class RegularGemAbility : IGemAbility {
    public void Execute(Board board, CollectedGemInfo self, HashSet<CollectedGemInfo> collectedGems) {
        collectedGems.Add(self);
    }
}

public class BombAbility : IGemAbility {
    private const int DESTRUCTION_DELAY_MS = 2000;

    private readonly Vector2Int[] _explosionOffsets = {
        // 3x3 square
        new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
        new(1, 1), new(-1, 1), new(1, -1), new(-1, -1),
        // Arms
        new(2, 0), new(-2, 0), new(0, 2), new(0, -2)
    };

    public void Execute(Board board, CollectedGemInfo self, HashSet<CollectedGemInfo> collectedGems) {
        self.destroyDelay = DESTRUCTION_DELAY_MS;
        collectedGems.Add(self);

        foreach (var offset in _explosionOffsets) {
            var pos = self.position + offset;
            if (!board.IsValidPos(pos)) {
                continue;
            }

            var gem = board.GetAt(pos);
            if (gem == null || gem.Type == GemType.Bomb) {
                continue;
            }

            var info = new CollectedGemInfo(pos, gem, DESTRUCTION_DELAY_MS);
            collectedGems.Add(info);
        }
    }
}