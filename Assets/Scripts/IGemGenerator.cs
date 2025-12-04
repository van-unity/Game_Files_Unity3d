using UnityEngine;

public interface IGemGenerator {
    GlobalEnums.GemType Execute(Vector2Int pos, GameBoard board);
}

public class GemGenerator : IGemGenerator {
    private readonly SC_GameVariables _gameVariables;

    public GemGenerator(SC_GameVariables gameVariables) {
        _gameVariables = gameVariables;
    }

    public GlobalEnums.GemType Execute(Vector2Int pos, GameBoard board) {
        if (Random.Range(0, 100f) < _gameVariables.bombChance)
            return GlobalEnums.GemType.bomb;

        var gemType = GemTypeExtensions.AllGemTypes[Random.Range(0, GemTypeExtensions.GemTypeCount)];
        var iterations = 0;

        while (board.MatchesAt(pos, gemType) && iterations < 100) {
            gemType = GemTypeExtensions.AllGemTypes[Random.Range(0, GemTypeExtensions.GemTypeCount)];
            iterations++;
        }

        return gemType;
    }
}