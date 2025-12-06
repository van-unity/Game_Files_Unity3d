using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IBoardInitializeStrategy {
    void Execute(GameBoard board);
}

public class BoardInitializeStrategy : IBoardInitializeStrategy {
    private const int MAX_ITERATIONS = 100;

    private readonly IMatchCheckStrategy _matchCheckStrategy;

    public BoardInitializeStrategy(IMatchCheckStrategy matchCheckStrategy) {
        _matchCheckStrategy = matchCheckStrategy;
    }

    public void Execute(GameBoard board) {
        var width = board.Width;
        var height = board.Height;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                var pos = new Vector2Int(x, y);
                var gemColor = GemTypeExtensions.GemColors[Random.Range(0, GemTypeExtensions.GemColors.Count)];
                var iterations = 0;

                while (_matchCheckStrategy.MatchesAtInitialization(board, pos, gemColor) &&
                       iterations < MAX_ITERATIONS) {
                    gemColor = GemTypeExtensions.GemColors[Random.Range(0, GemTypeExtensions.GemColors.Count)];
                    iterations++;
                }

                var newGem = new Gem(gemColor, GemType.Regular, 10);
                board.SetAt(x, y, newGem);
            }
        }
    }
}

public class GameBoard {
    private readonly IBoardInitializeStrategy _initializeStrategy;
    private readonly IBoardRefillStrategy _refillStrategy;
    private readonly IMatchCheckStrategy _matchCheckStrategy;
    private readonly Gem[,] _state;

    public int Height { get; }

    public int Width { get; }


    public GameBoard(
        int width,
        int height,
        IBoardInitializeStrategy initializeStrategy,
        IBoardRefillStrategy refillStrategy,
        IMatchCheckStrategy matchCheckStrategy
    ) {
        Width = width;
        Height = height;

        _initializeStrategy = initializeStrategy;
        _refillStrategy = refillStrategy;
        _matchCheckStrategy = matchCheckStrategy;

        _state = new Gem[width, height];

        initializeStrategy.Execute(this);
    }

    public bool TrySwapGems(Vector2Int pos1, Vector2Int pos2) {
        if (!IsValidPos(pos1) || !IsValidPos(pos2)) {
            return false;
        }

        (_state[pos1.x, pos1.y], _state[pos2.x, pos2.y]) = (_state[pos2.x, pos2.y], _state[pos1.x, pos1.y]);
        return true;
    }

    public bool IsValidMove(Vector2Int from, Vector2Int to) {
        if (!IsValidPos(from) || !IsValidPos(to)) {
            return false;
        }

        var gem1 = _state[from.x, from.y];
        var gem2 = _state[to.x, to.y];
        if (gem1 == null || gem2 == null) {
            return false;
        }

        return _matchCheckStrategy.MatchesAtGameplay(this, to) || _matchCheckStrategy.MatchesAtGameplay(this, from);
    }

    public bool TryResolveMatches(out List<CollectedGemInfo> result) {
        var matchPositions = _matchCheckStrategy.GetMatches(this).ToList();

        if (matchPositions.Count == 0) {
            result = null;
            return false;
        }

        result = new List<CollectedGemInfo>();

        foreach (var match in matchPositions) {
            var info = new CollectedGemInfo(match, GetAt(match));
            result.Add(info);

            _state[match.x, match.y] = null;
        }

        return true;
    }

    public List<ChangeInfo> Refill(IEnumerable<CollectedGemInfo> collectedGemInfos)
        => _refillStrategy.Execute(this, collectedGemInfos);

    public Gem GetAt(Vector2Int pos) => !IsValidPos(pos) ? null : _state[pos.x, pos.y];

    public Gem GetAt(int x, int y) => !IsValidPos(x, y) ? null : _state[x, y];

    public void SetAt(Vector2Int pos, Gem gem) {
        if (!IsValidPos(pos)) {
            return;
        }

        _state[pos.x, pos.y] = gem;
    }

    public void SetAt(int x, int y, Gem gem) {
        if (!IsValidPos(x, y)) {
            return;
        }

        _state[x, y] = gem;
    }

    public bool TryParseMousePos(Vector2 mousePos, out Vector2Int boardPos) {
        boardPos = mousePos.ToVector2Int();

        return IsValidPos(boardPos);
    }

    public bool IsValidPos(Vector2Int boardPos) =>
        boardPos.x >= 0 && boardPos.x < Width && boardPos.y >= 0 && boardPos.y < Height;

    public bool IsValidPos(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;
}