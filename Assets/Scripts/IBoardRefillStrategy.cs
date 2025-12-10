using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public interface IBoardRefillStrategy {
    List<ChangeInfo> Execute(Board board, IEnumerable<CollectedGemInfo> collectedGems = null);
}

public class BoardRefillStrategy : IBoardRefillStrategy {
    private const int MAX_ITERATIONS = 100;

    private readonly IMatchCheckStrategy _matchCheckStrategy;
    private readonly GemRepository _gemRepository;

    [Inject]
    public BoardRefillStrategy(IMatchCheckStrategy matchCheckStrategy, GemRepository gemRepository) {
        _matchCheckStrategy = matchCheckStrategy;
        _gemRepository = gemRepository;
    }

    public List<ChangeInfo> Execute(Board board, IEnumerable<CollectedGemInfo> collectedGems = null) {
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
                var config = _gemRepository.GetConfig(collectedGem.Key, GemType.Bomb);
                var bombGem = new Gem(collectedGem.Key, GemType.Bomb, config.ScoreValue);
                board.SetAt(pos, bombGem);
                var changeInfo = new ChangeInfo(bombGem, true, 0, pos, pos);
                changes.Add(pos, changeInfo);
            }
        }

        var creationTimes = new Dictionary<int, int>();

        MoveGemsDown(board, changes, creationTimes);


        for (int x = 0; x < width; x++) {
            if (!creationTimes.TryGetValue(x, out var creationTime)) {
                creationTime = 0;
            }

            for (int y = 0; y < height; y++) {
                if (board.GetAt(x, y) != null) {
                    continue;
                }

                var pos = new Vector2Int(x, y);
                var gemConfig = _gemRepository.GetRandomRegularGem();
                var newGem = new Gem(gemConfig.Color, GemType.Regular, gemConfig.ScoreValue);
                board.SetAt(x, y, newGem);
                var iterations = 0;

                while (_matchCheckStrategy.MatchesAtGameplay(board, pos) && iterations < MAX_ITERATIONS) {
                    gemConfig = _gemRepository.GetRandomRegularGem();
                    newGem.Color = gemConfig.Color;
                    newGem.ScoreValue = gemConfig.ScoreValue;
                    board.SetAt(x, y, newGem);
                    iterations++;
                }

                var changeInfo = new ChangeInfo(newGem, true, creationTime++, new Vector2Int(x, height), pos);
                changes.Add(pos, changeInfo);
            }

            creationTimes[x] = creationTime;
        }

        return changes.Values.ToList();
    }

    private void MoveGemsDown(Board board, Dictionary<Vector2Int, ChangeInfo> changes,
        Dictionary<int, int> creationTimes) {
        if (creationTimes == null) {
            creationTimes = new Dictionary<int, int>();
        }

        var width = board.Width;
        var height = board.Height;

        for (int x = 0; x < width; x++) {
            if (!creationTimes.TryGetValue(x, out var creationTime)) {
                creationTime = 0;
            }

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

                var changeInfo = new ChangeInfo(board.GetAt(finalPos.x, finalPos.y), false, creationTime++,
                    fromPos,
                    finalPos);
                changes.Add(finalPos, changeInfo);
            }

            creationTimes[x] = creationTime;
        }
    }
}