using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IMatchCheckStrategy {
    bool MatchesAt(GameBoard board, Vector2Int pos, GemType gemType);
    IEnumerable<Vector2Int> GetMatches(GameBoard board);
}

public class MatchCheckStrategy : IMatchCheckStrategy{
    public bool MatchesAt(GameBoard board, Vector2Int pos, GemType gemType) {
        if (!board.IsValidPos(pos)) {
            return false;
        }

        // Check horizontal matches (3 possible formations)

        // 1. Formation: [Type] [Type] [POS] (Checks two left neighbors)
        if (pos.x >= 2 && board.GetAt(pos.x - 2, pos.y) == gemType && board.GetAt(pos.x - 1, pos.y) == gemType) {
            return true;
        }

        // 2. Formation: [Type] [POS] [Type] (Checks one left, one right neighbor)
        if (pos.x >= 1 && pos.x <= board.Width - 2 && board.GetAt(pos.x - 1, pos.y) == gemType &&
            board.GetAt(pos.x + 1, pos.y) == gemType) {
            return true;
        }

        // 3. Formation: [POS] [Type] [Type] (Checks two right neighbors)
        // This formation is redundant for initialization but necessary for gameplay.
        if (pos.x <= board.Width - 3 && board.GetAt(pos.x + 1, pos.y) == gemType && board.GetAt(pos.x + 2, pos.y) == gemType) {
            return true;
        }

        // Check vertical matches (3 possible formations)

        // 4. Formation: [Type] [Type] [POS] (Checks two down neighbors)
        if (pos.y >= 2 && board.GetAt(pos.x, pos.y - 2) == gemType && board.GetAt(pos.x, pos.y - 1) == gemType) {
            return true;
        }

        // 5. Formation: [Type] [POS] [Type] (Checks one down, one up neighbor)
        if (pos.y >= 1 && pos.y <= board.Height - 2 && board.GetAt(pos.x, pos.y - 1) == gemType &&
            board.GetAt(pos.x, pos.y + 1) == gemType) {
            return true;
        }

        // 6. Formation: [POS] [Type] [Type] (Checks two up neighbors)
        if (pos.y <= board.Height - 3 && board.GetAt(pos.x, pos.y + 1) == gemType && board.GetAt(pos.x, pos.y + 2) == gemType) {
            return true;
        }

        return false;
    }

    public IEnumerable<Vector2Int> GetMatches(GameBoard  board) {
        var length = board.Width;
        var height = board.Height;
        
        var matchedPositions = new HashSet<Vector2Int>();

        for (int x = 0; x < length; x++) {
            for (int y = 0; y < height; y++) {
                var currentType = board.GetAt(x, y);
                if (currentType == GemType.none) {
                    continue;
                }

                //horizontal 
                if (x < length - 2) {
                    if (board.GetAt(x + 1, y) == currentType && board.GetAt(x + 2, y) == currentType) {
                        matchedPositions.Add(new Vector2Int(x, y));
                        matchedPositions.Add(new Vector2Int(x + 1, y));
                        matchedPositions.Add(new Vector2Int(x + 2, y));
                    }
                }

                // vertical check 
                if (y < height - 2) {
                    if (board.GetAt(x, y + 1) == currentType && board.GetAt(x, y + 2) == currentType) {
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

public class GameBoard {
    #region Variables

    private readonly IGemGenerator _gemGenerator;
    private readonly IMatchCheckStrategy  _matchCheckStrategy;
    private readonly GemType[,] _state;

    public int Height { get; }

    public int Width { get; }


    private int score = 0;

    public int Score {
        get { return score; }
        set { score = value; }
    }

    #endregion

    public GameBoard(int width, int height, IGemGenerator gemGenerator, IMatchCheckStrategy matchCheckStrategy) {
        Width = width;
        Height = height;
        _gemGenerator = gemGenerator;
        _matchCheckStrategy = matchCheckStrategy;
        _state = new GemType[width, height];
        Initialize();
    }

    public void Initialize() {
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                _state[x, y] = _gemGenerator.Execute(new Vector2Int(x, y), this);
            }
        }
    }

    // public void SetGem(int _X, int _Y, SC_Gem _Gem) {
    //     allGems[_X, _Y] = _Gem;
    // }


    // public void FindAllMatches() {
    //     currentMatches.Clear();
    //     
    //     for (int x = 0; x < width; x++)
    //         for (int y = 0; y < height; y++) {
    //             SC_Gem currentGem = allGems[x, y];
    //             if (currentGem != null) {
    //                 if (x > 0 && x < width - 1) {
    //                     SC_Gem leftGem = allGems[x - 1, y];
    //                     SC_Gem rightGem = allGems[x + 1, y];
    //                     //checking no empty spots
    //                     if (leftGem != null && rightGem != null) {
    //                         //Match
    //                         if (leftGem.type == currentGem.type && rightGem.type == currentGem.type) {
    //                             currentGem.isMatch = true;
    //                             leftGem.isMatch = true;
    //                             rightGem.isMatch = true;
    //                             currentMatches.Add(currentGem);
    //                             currentMatches.Add(leftGem);
    //                             currentMatches.Add(rightGem);
    //                         }
    //                     }
    //                 }
    //     
    //                 if (y > 0 && y < height - 1) {
    //                     SC_Gem aboveGem = allGems[x, y - 1];
    //                     SC_Gem bellowGem = allGems[x, y + 1];
    //                     //checking no empty spots
    //                     if (aboveGem != null && bellowGem != null) {
    //                         //Match
    //                         if (aboveGem.type == currentGem.type && bellowGem.type == currentGem.type) {
    //                             currentGem.isMatch = true;
    //                             aboveGem.isMatch = true;
    //                             bellowGem.isMatch = true;
    //                             currentMatches.Add(currentGem);
    //                             currentMatches.Add(aboveGem);
    //                             currentMatches.Add(bellowGem);
    //                         }
    //                     }
    //                 }
    //             }
    //         }
    //     
    //     if (currentMatches.Count > 0)
    //         currentMatches = currentMatches.Distinct().ToList();
    //
    //     CheckForBombs();
    // }

    // public void CheckForBombs() {
    //     for (int i = 0; i < currentMatches.Count; i++) {
    //         GlobalEnums.GemType gem = currentMatches[i];
    //         int x = gem.posIndex.x;
    //         int y = gem.posIndex.y;
    //
    //         if (gem.posIndex.x > 0) {
    //             if (allGems[x - 1, y] != GlobalEnums.GemType.none && allGems[x - 1, y] == GlobalEnums.GemType.bomb)
    //                 MarkBombArea(new Vector2Int(x - 1, y), allGems[x - 1, y].blastSize);
    //         }
    //
    //         if (gem.posIndex.x + 1 < width) {
    //             if (allGems[x + 1, y] != null && allGems[x + 1, y].type == GlobalEnums.GemType.bomb)
    //                 MarkBombArea(new Vector2Int(x + 1, y), allGems[x + 1, y].blastSize);
    //         }
    //
    //         if (gem.posIndex.y > 0) {
    //             if (allGems[x, y - 1] != null && allGems[x, y - 1].type == GlobalEnums.GemType.bomb)
    //                 MarkBombArea(new Vector2Int(x, y - 1), allGems[x, y - 1].blastSize);
    //         }
    //
    //         if (gem.posIndex.y + 1 < height) {
    //             if (allGems[x, y + 1] != null && allGems[x, y + 1].type == GlobalEnums.GemType.bomb)
    //                 MarkBombArea(new Vector2Int(x, y + 1), allGems[x, y + 1].blastSize);
    //         }
    //     }
    // }
    //
    // public void MarkBombArea(Vector2Int bombPos, int _BlastSize) {
    //     string _print = "";
    //     for (int x = bombPos.x - _BlastSize; x <= bombPos.x + _BlastSize; x++) {
    //         for (int y = bombPos.y - _BlastSize; y <= bombPos.y + _BlastSize; y++) {
    //             if (x >= 0 && x < width && y >= 0 && y < height) {
    //                 if (allGems[x, y] != null) {
    //                     _print += "(" + x + "," + y + ")" + System.Environment.NewLine;
    //                     allGems[x, y].isMatch = true;
    //                     currentMatches.Add(allGems[x, y]);
    //                 }
    //             }
    //         }
    //     }
    //
    //     currentMatches = currentMatches.Distinct().ToList();
    // }

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
            _state[match.x, match.y] = GemType.none;
        }

        //apply special gems(like bomb) 
        var changes = MoveGemsDown(_state).Concat(GenerateNewGems(_state));
        //generate new gems 
        result = new ResolveResult(collectedGems, changes);
        return true;
    }

    private IEnumerable<ChangeInfo> MoveGemsDown(GemType[,] board) {
        for (int x = 0; x < Width; x++) {
            int resolveStep = 0;
            for (int y = 0; y < Height; y++) {
                if (board[x, y] == GemType.none) {
                    continue;
                }

                var fromPos = new Vector2Int(x, y);
                var finalPos = fromPos;
                var belowPos = new Vector2Int(x, y - 1);
                while (IsValidPos(belowPos) && board[belowPos.x, belowPos.y] == GemType.none) {
                    finalPos = belowPos;
                    belowPos = new Vector2Int(belowPos.x, belowPos.y - 1);
                }

                if (finalPos.y == fromPos.y) {
                    continue;
                }

                board[finalPos.x, finalPos.y] = board[fromPos.x, fromPos.y];
                board[fromPos.x, fromPos.y] = GemType.none;

                yield return new ChangeInfo(board[finalPos.x, finalPos.y], false, resolveStep++, fromPos, finalPos);
            }
        }
    }

    private IEnumerable<ChangeInfo> GenerateNewGems(GemType[,] board) {
        for (int x = 0; x < Width; x++) {
            int resolveStep = 0;
            for (int y = 0; y < Height; y++) {
                if (board[x, y] != GemType.none) {
                    continue;
                }

                var pos = new Vector2Int(x, y);
                var newGemType = _gemGenerator.Execute(pos, this);
                board[x, y] = newGemType;
                yield return new ChangeInfo(newGemType, true, resolveStep++, new Vector2Int(x, Height), pos);
            }
        }
    }

    public bool TryGetGem(Vector2Int pos, out GemType gem) {
        if (!IsValidPos(pos)) {
            gem = GemType.none;
            return false;
        }

        gem = _state[pos.x, pos.y];
        return true;
    }

    public GemType GetAt(Vector2Int pos) => _state[pos.x, pos.y];
    public GemType GetAt(int x, int y) => _state[x, y];

    public bool TryParseMousePos(Vector3 mousePos, out Vector2Int boardPos) {
        boardPos = new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));

        return IsValidPos(boardPos);
    }

    public bool IsValidPos(Vector2Int boardPos) =>
        boardPos.x >= 0 && boardPos.x < Width && boardPos.y >= 0 && boardPos.y < Height;
}