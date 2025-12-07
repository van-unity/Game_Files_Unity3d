using System.Collections.Generic;
using UnityEngine;

//created the MatchesAtInitialization to not make unnecessary checks on initialization
public interface IMatchCheckStrategy {
    bool MatchesAtInitialization(Board board, Vector2Int pos, GemColor gemColor);
    bool MatchesAtGameplay(Board board, Vector2Int pos);
    IEnumerable<Vector2Int> GetMatches(Board board);
}

public class MatchCheckStrategy : IMatchCheckStrategy {
    public bool MatchesAtInitialization(Board board, Vector2Int pos, GemColor gemColor) {
        if (pos.x >= 2) {
            var left1 = board.GetAt(pos.x - 1, pos.y);
            var left2 = board.GetAt(pos.x - 2, pos.y);
            if (left1 == null || left2 == null) {
                return false;
            }

            if (left1.Color == gemColor && IsMatch(left1, left2)) {
                return true;
            }
        }

        if (pos.y >= 2) {
            var top1 = board.GetAt(pos.x, pos.y - 1);
            var top2 = board.GetAt(pos.x, pos.y - 2);
            if (top1 == null || top2 == null) {
                return false;
            }

            if (top1.Color == gemColor && IsMatch(top1, top2)) {
                return true;
            }
        }

        return false;
    }

    public bool MatchesAtGameplay(Board board, Vector2Int pos) {
        if (!board.IsValidPos(pos)) {
            return false;
        }
        
        var targetGem = board.GetAt(pos.x, pos.y);
        
        // Check horizontal matches (3 possible formations)

        // 1. Formation: [Type] [Type] [POS] (Checks two left neighbors)
        var gem1 = board.GetAt(pos.x - 2, pos.y);
        var gem2 = board.GetAt(pos.x - 1, pos.y);

        if (pos.x >= 2 && IsMatch(targetGem, gem1) && IsMatch(targetGem, gem2)) {
            return true;
        }

        // 2. Formation: [Type] [POS] [Type] (Checks one left, one right neighbor)
        gem1 = board.GetAt(pos.x - 1, pos.y);
        gem2 = board.GetAt(pos.x + 1, pos.y);
        if (pos.x >= 1 && pos.x <= board.Width - 2 && IsMatch(targetGem, gem1) && IsMatch(targetGem, gem2)) {
            return true;
        }

        // 3. Formation: [POS] [Type] [Type] (Checks two right neighbors)
        // This formation is redundant for initialization but necessary for gameplay.
        gem1 = board.GetAt(pos.x + 1, pos.y);
        gem2 = board.GetAt(pos.x + 2, pos.y);
        if (pos.x <= board.Width - 3 && IsMatch(targetGem, gem1) && IsMatch(targetGem, gem2)) {
            return true;
        }

        // Check vertical matches (3 possible formations)

        // 4. Formation: [Type] [Type] [POS] (Checks two down neighbors)
        gem2 = board.GetAt(pos.x, pos.y - 1);
        gem1 = board.GetAt(pos.x, pos.y - 2);
        if (pos.y >= 2 && IsMatch(targetGem, gem1) && IsMatch(targetGem, gem2)) {
            return true;
        }

        // 5. Formation: [Type] [POS] [Type] (Checks one down, one up neighbor)
        gem1 = board.GetAt(pos.x, pos.y + 1);
        gem2 = board.GetAt(pos.x, pos.y - 1);
        if (pos.y >= 1 && pos.y <= board.Height - 2 && IsMatch(targetGem, gem1) && IsMatch(targetGem, gem2)) {
            return true;
        }

        // 6. Formation: [POS] [Type] [Type] (Checks two up neighbors)
        gem1 = board.GetAt(pos.x, pos.y + 1);
        gem2 = board.GetAt(pos.x, pos.y + 2);
        if (pos.y <= board.Height - 3 && IsMatch(targetGem, gem1) && IsMatch(targetGem, gem2)) {
            return true;
        }

        return false;
    }

    private bool IsMatch(Gem gem1, Gem gem2) {
        if (gem1 == null || gem2 == null) {
            return false;
        }
        
        //if any of the gems are regular then we check only by color
        if (gem1.Type == GemType.Regular || gem2.Type == GemType.Regular) {
            return gem1.Color == gem2.Color;
        }
        //otherwise we check by types only 
        return gem1.Type == gem2.Type;
    }

    public IEnumerable<Vector2Int> GetMatches(Board board) {
        var length = board.Width;
        var height = board.Height;

        var matchedPositions = new HashSet<Vector2Int>();

        for (int x = 0; x < length; x++) {
            for (int y = 0; y < height; y++) {
                var gem = board.GetAt(x, y);
                if (gem == null) {
                    continue;
                }

                //horizontal 
                if (x < length - 2) {
                    if (IsMatch(board.GetAt(x + 1, y), gem) && IsMatch(board.GetAt(x + 2, y), gem)) {
                        matchedPositions.Add(new Vector2Int(x, y));
                        matchedPositions.Add(new Vector2Int(x + 1, y));
                        matchedPositions.Add(new Vector2Int(x + 2, y));
                    }
                }

                // vertical check 
                if (y < height - 2) {
                    if (IsMatch(board.GetAt(x, y + 1), gem) && IsMatch(board.GetAt(x, y + 2), gem)) {
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