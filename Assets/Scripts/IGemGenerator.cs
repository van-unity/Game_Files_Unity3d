using UnityEngine;

public interface IGemGenerator {
    Gem Execute(Vector2Int pos, GameBoard board);
}

public class GemGenerator : IGemGenerator {
    private readonly SC_GameVariables _gameVariables;
    private readonly IMatchCheckStrategy _matchCheckStrategy;

    public GemGenerator(SC_GameVariables gameVariables, IMatchCheckStrategy matchCheckStrategy) {
        _gameVariables = gameVariables;
        _matchCheckStrategy = matchCheckStrategy;
    }

    public Gem Execute(Vector2Int pos, GameBoard board) {
        if (Random.Range(0, 100f) < _gameVariables.bombChance)
            return new Gem(GemColor.Blue, GemType.Bomb, 10);

        var gemType = GemTypeExtensions.GemColors[Random.Range(0, GemTypeExtensions.GemColors.Count)];
        var iterations = 0;

        while (_matchCheckStrategy.MatchesAt(board, pos, gemType) && iterations < 100) {
            gemType = GemTypeExtensions.GemColors[Random.Range(0, GemTypeExtensions.GemColors.Count)];
            iterations++;
        }
        
        return new Gem(gemType, GemType.Regular, 10);
    }
}