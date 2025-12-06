using UnityEngine;

public interface IGemGenerator {
    GemType Execute(Vector2Int pos, GameBoard board);
}

public class GemGenerator : IGemGenerator {
    private readonly SC_GameVariables _gameVariables;
    private readonly IMatchCheckStrategy _matchCheckStrategy;

    public GemGenerator(SC_GameVariables gameVariables, IMatchCheckStrategy matchCheckStrategy) {
        _gameVariables = gameVariables;
        _matchCheckStrategy = matchCheckStrategy;
    }

    public GemType Execute(Vector2Int pos, GameBoard board) {
        if (Random.Range(0, 100f) < _gameVariables.bombChance)
            return GemType.bomb;

        var gemType = GemTypeExtensions.AllGemTypes[Random.Range(0, GemTypeExtensions.GemTypeCount)];
        var iterations = 0;

        while (_matchCheckStrategy.MatchesAt(board, pos, gemType) && iterations < 100) {
            gemType = GemTypeExtensions.AllGemTypes[Random.Range(0, GemTypeExtensions.GemTypeCount)];
            iterations++;
        }
        
        return gemType;
    }
}