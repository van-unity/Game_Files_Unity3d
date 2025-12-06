using System.Collections.Generic;
using UnityEngine;

public interface IMatchCheckStrategy {
    bool MatchesAt(GameBoard board, Vector2Int pos, GemColor gemColor);
    IEnumerable<Vector2Int> GetMatches(GameBoard board);
}

public class MatchCheckStrategy : IMatchCheckStrategy {
    public bool MatchesAt(GameBoard board, Vector2Int pos, GemColor gemColor) {
        if (!board.IsValidPos(pos)) {
            return false;
        }

        // Check horizontal matches (3 possible formations)

        // 1. Formation: [Type] [Type] [POS] (Checks two left neighbors)
        if (pos.x >= 2 && board.GetAt(pos.x - 2, pos.y).Color == gemColor &&
            board.GetAt(pos.x - 1, pos.y).Color == gemColor) {
            return true;
        }

        // 2. Formation: [Type] [POS] [Type] (Checks one left, one right neighbor)
        if (pos.x >= 1 && pos.x <= board.Width - 2 && board.GetAt(pos.x - 1, pos.y).Color == gemColor &&
            board.GetAt(pos.x + 1, pos.y).Color == gemColor) {
            return true;
        }

        // 3. Formation: [POS] [Type] [Type] (Checks two right neighbors)
        // This formation is redundant for initialization but necessary for gameplay.
        if (pos.x <= board.Width - 3 && board.GetAt(pos.x + 1, pos.y).Color == gemColor &&
            board.GetAt(pos.x + 2, pos.y).Color == gemColor) {
            return true;
        }

        // Check vertical matches (3 possible formations)

        // 4. Formation: [Type] [Type] [POS] (Checks two down neighbors)
        if (pos.y >= 2 && board.GetAt(pos.x, pos.y - 2).Color == gemColor &&
            board.GetAt(pos.x, pos.y - 1).Color == gemColor) {
            return true;
        }

        // 5. Formation: [Type] [POS] [Type] (Checks one down, one up neighbor)
        if (pos.y >= 1 && pos.y <= board.Height - 2 && board.GetAt(pos.x, pos.y - 1).Color == gemColor &&
            board.GetAt(pos.x, pos.y + 1).Color == gemColor) {
            return true;
        }

        // 6. Formation: [POS] [Type] [Type] (Checks two up neighbors)
        if (pos.y <= board.Height - 3 && board.GetAt(pos.x, pos.y + 1).Color == gemColor &&
            board.GetAt(pos.x, pos.y + 2).Color == gemColor) {
            return true;
        }

        return false;
    }

    private bool IsMatch(Gem gem1, Gem gem2) {
        return gem1.Color == gem2.Color && gem1.Type == gem2.Type;
    }

    public IEnumerable<Vector2Int> GetMatches(GameBoard board) {
        var length = board.Width;
        var height = board.Height;

        var matchedPositions = new HashSet<Vector2Int>();

        for (int x = 0; x < length; x++) {
            for (int y = 0; y < height; y++) {
                var gem =  board.GetAt(x, y);
                if (gem == null) {
                    continue;
                }
                var color = board.GetAt(x, y).Color;

                //horizontal 
                if (x < length - 2) {
                    if (board.GetAt(x + 1, y).Color == color && board.GetAt(x + 2, y).Color == color) {
                        matchedPositions.Add(new Vector2Int(x, y));
                        matchedPositions.Add(new Vector2Int(x + 1, y));
                        matchedPositions.Add(new Vector2Int(x + 2, y));
                    }
                }

                // vertical check 
                if (y < height - 2) {
                    if (board.GetAt(x, y + 1).Color == color && board.GetAt(x, y + 2).Color == color) {
                        matchedPositions.Add(new Vector2Int(x, y));
                        matchedPositions.Add(new Vector2Int(x, y + 1));
                        matchedPositions.Add(new Vector2Int(x, y + 2));
                    }
                }
            }
        }

        return matchedPositions;
    }
}