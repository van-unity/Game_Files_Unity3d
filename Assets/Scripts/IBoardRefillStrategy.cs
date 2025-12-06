using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IBoardRefillStrategy {
    List<ChangeInfo> Execute(GameBoard board, IEnumerable<CollectedGemInfo> collectedGems = null);
}

public class BoardRefillStrategy : IBoardRefillStrategy {
    private const int MAX_ITERATIONS = 100;

    private readonly IMatchCheckStrategy _matchCheckStrategy;

    public BoardRefillStrategy(IMatchCheckStrategy matchCheckStrategy) {
        _matchCheckStrategy = matchCheckStrategy;
    }

    public List<ChangeInfo> Execute(GameBoard board, IEnumerable<CollectedGemInfo> collectedGems = null) {
        var width = board.Width;
        var height = board.Height;
        var changes = new Dictionary<Vector2Int, ChangeInfo>();
        
        //assign special pieces 
        if (collectedGems != null) {
            var collectedGemsByColor = new Dictionary<GemColor, List<CollectedGemInfo>>();

            foreach (var gemInfo in collectedGems) {
                if (collectedGemsByColor.ContainsKey(gemInfo.gem.Color)) {
                    collectedGemsByColor[gemInfo.gem.Color].Add(gemInfo);
                } else {
                    collectedGemsByColor.Add(gemInfo.gem.Color, new List<CollectedGemInfo> { gemInfo });
                }
            }

            foreach (var collectedGem in collectedGemsByColor) {
                if (collectedGem.Value.Count < 4) {
                    continue;
                }

                var pos = collectedGem.Value[0].position.x == collectedGem.Value[1].position.x
                    ? collectedGem.Value.OrderBy(x => x.position.x).First().position
                    : collectedGem.Value.OrderBy(x => x.position.y).First().position;

                var bombGem = new Gem(collectedGem.Key, GemType.Bomb, 10);
                board.SetAt(pos, bombGem);
                var changeInfo = new ChangeInfo(bombGem, true, 0, new Vector2Int(pos.x, height), pos);
                changes.Add(pos, changeInfo);
            }
        }
        
        MoveGemsDown(board, changes);
        
        for (int x = 0; x < width; x++) {
            var resolveStep = 0;
            for (int y = 0; y < height; y++) {
                if (board.GetAt(x, y) != null) {
                    continue;
                }

                var pos = new Vector2Int(x, y);
                var gemType = GemTypeExtensions.GemColors[Random.Range(0, GemTypeExtensions.GemColors.Count)];
                var iterations = 0;

                while (_matchCheckStrategy.MatchesAtGameplay(board, pos) && iterations < MAX_ITERATIONS) {
                    gemType = GemTypeExtensions.GemColors[Random.Range(0, GemTypeExtensions.GemColors.Count)];
                    iterations++;
                }

                var newGem = new Gem(gemType, GemType.Regular, 10);
                board.SetAt(x, y, newGem);
                var changeInfo = new ChangeInfo(newGem, true, resolveStep++, new Vector2Int(x, height), pos);
                changes.Add(pos, changeInfo);
            }
        }

        return changes.Values.ToList();
    }

    private void MoveGemsDown(GameBoard board, Dictionary<Vector2Int, ChangeInfo> changes) {
        var width = board.Width;
        var height = board.Height;

        for (int x = 0; x < width; x++) {
            int resolveStep = 0;
            for (int y = 0; y < height; y++) {
                if (board.GetAt(x, y) == null) {
                    continue;
                }

                var fromPos = new Vector2Int(x, y);
                if (changes.ContainsKey(fromPos)) {
                    continue;
                }

                var finalPos = fromPos;
                var belowPos = new Vector2Int(x, y - 1);
                while (board.IsValidPos(belowPos) && board.GetAt(belowPos.x, belowPos.y) == null) {
                    finalPos = belowPos;
                    belowPos = new Vector2Int(belowPos.x, belowPos.y - 1);
                }

                if (finalPos.y == fromPos.y) {
                    continue;
                }

                board.SetAt(finalPos.x, finalPos.y, board.GetAt(fromPos.x, fromPos.y));
                board.SetAt(fromPos.x, fromPos.y, null);

                var changeInfo = new ChangeInfo(board.GetAt(finalPos.x, finalPos.y), false, resolveStep++, fromPos,
                    finalPos);
                changes.Add(finalPos, changeInfo);
            }
        }
    }
}