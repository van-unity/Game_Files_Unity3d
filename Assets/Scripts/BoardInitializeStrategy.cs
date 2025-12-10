using UnityEngine;
using Zenject;

public interface IBoardInitializeStrategy {
    void Execute(Board board);
}

public class BoardInitializeStrategy : IBoardInitializeStrategy {
    private const int MAX_ITERATIONS = 100;

    private readonly IMatchCheckStrategy _matchCheckStrategy;
    private readonly GemRepository _gemRepository;

    [Inject]
    public BoardInitializeStrategy(IMatchCheckStrategy matchCheckStrategy, GemRepository gemRepository) {
        _matchCheckStrategy = matchCheckStrategy;
        _gemRepository = gemRepository;
    }

    public void Execute(Board board) {
        var width = board.Width;
        var height = board.Height;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                var pos = new Vector2Int(x, y);
                var gemConfig = _gemRepository.GetRandomRegularGem();
                var iterations = 0;

                while (_matchCheckStrategy.MatchesAtInitialization(board, pos, gemConfig.Color) &&
                       iterations < MAX_ITERATIONS) {
                    gemConfig = _gemRepository.GetRandomRegularGem();
                    iterations++;
                }

                var newGem = new Gem(gemConfig.Color, GemType.Regular, gemConfig.ScoreValue);
                board.SetAt(x, y, newGem);
            }
        }
    }
}