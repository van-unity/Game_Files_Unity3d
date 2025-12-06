using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard {
    private readonly IGemGenerator _gemGenerator;
    private readonly IMatchCheckStrategy  _matchCheckStrategy;
    private readonly Gem[,] _state;

    public int Height { get; }

    public int Width { get; }
    

    public GameBoard(int width, int height, IGemGenerator gemGenerator, IMatchCheckStrategy matchCheckStrategy) {
        Width = width;
        Height = height;
        _gemGenerator = gemGenerator;
        _matchCheckStrategy = matchCheckStrategy;
        _state = new Gem[width, height];
        Initialize();
    }

    public void Initialize() {
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                _state[x, y] = _gemGenerator.Execute(new Vector2Int(x, y), this);
            }
        }
    }

    public bool TrySwapGems(Vector2Int pos1, Vector2Int pos2) {
        if (!IsValidPos(pos1) || !IsValidPos(pos2)) {
            return false;
        }

        (_state[pos1.x, pos1.y], _state[pos2.x, pos2.y]) = (_state[pos2.x, pos2.y], _state[pos1.x, pos1.y]);
        return true;
    }

    public bool TryResolve(out ResolveResult result) {
        var matches = _matchCheckStrategy.GetMatches(this).ToList();
        if (matches.Count == 0) {
            result = null;
            return false;
        }

        var collectedGems = new List<CollectedGemInfo>();
        //remove matches 
        foreach (var match in matches) {
            collectedGems.Add(new CollectedGemInfo(match, GetAt(match)));
            _state[match.x, match.y] = null;
        }

        //apply special gems(like bomb) 
        var changes = MoveGemsDown(_state).Concat(GenerateNewGems(_state));
        //generate new gems 
        result = new ResolveResult(collectedGems, changes);
        return true;
    }

    private IEnumerable<ChangeInfo> MoveGemsDown(Gem[,] board) {
        for (int x = 0; x < Width; x++) {
            int resolveStep = 0;
            for (int y = 0; y < Height; y++) {
                if (board[x, y] == null) {
                    continue;
                }

                var fromPos = new Vector2Int(x, y);
                var finalPos = fromPos;
                var belowPos = new Vector2Int(x, y - 1);
                while (IsValidPos(belowPos) && board[belowPos.x, belowPos.y] == null) {
                    finalPos = belowPos;
                    belowPos = new Vector2Int(belowPos.x, belowPos.y - 1);
                }

                if (finalPos.y == fromPos.y) {
                    continue;
                }

                board[finalPos.x, finalPos.y] = board[fromPos.x, fromPos.y];
                board[fromPos.x, fromPos.y] = null;

                yield return new ChangeInfo(board[finalPos.x, finalPos.y], false, resolveStep++, fromPos, finalPos);
            }
        }
    }

    private IEnumerable<ChangeInfo> GenerateNewGems(Gem[,] board) {
        for (int x = 0; x < Width; x++) {
            int resolveStep = 0;
            for (int y = 0; y < Height; y++) {
                if (board[x, y] != null) {
                    continue;
                }

                var pos = new Vector2Int(x, y);
                var newGemType = _gemGenerator.Execute(pos, this);
                board[x, y] = newGemType;
                yield return new ChangeInfo(newGemType, true, resolveStep++, new Vector2Int(x, Height), pos);
            }
        }
    }

    public bool TryGetGem(Vector2Int pos, out Gem gem) {
        if (!IsValidPos(pos)) {
            gem = null;
            return false;
        }

        gem = _state[pos.x, pos.y];
        return true;
    }

    public Gem GetAt(Vector2Int pos) => _state[pos.x, pos.y];
    public Gem GetAt(int x, int y) => _state[x, y];

    public bool TryParseMousePos(Vector3 mousePos, out Vector2Int boardPos) {
        boardPos = new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));

        return IsValidPos(boardPos);
    }

    public bool IsValidPos(Vector2Int boardPos) =>
        boardPos.x >= 0 && boardPos.x < Width && boardPos.y >= 0 && boardPos.y < Height;
}