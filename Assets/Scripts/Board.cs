using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class Board {
    private readonly IBoardInitializeStrategy _initializeStrategy;
    private readonly IBoardRefillStrategy _refillStrategy;
    private readonly IMatchCheckStrategy _matchCheckStrategy;
    private readonly Gem[,] _state;

    public int Height { get; }
    public int Width { get; }
    
    public Board(
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
    }

    public void Initialize() {
        _initializeStrategy.Execute(this);
    }
    
    public bool TrySwapGems(Vector2Int pos1, Vector2Int pos2) {
        if (!IsValidPos(pos1) || !IsValidPos(pos2)) {
            return false;
        }

        (_state[pos1.x, pos1.y], _state[pos2.x, pos2.y]) = (_state[pos2.x, pos2.y], _state[pos1.x, pos1.y]);
        return true;
    }

    public bool HasMatchAt(Vector2Int pos) => _matchCheckStrategy.MatchesAtGameplay(this, pos);

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