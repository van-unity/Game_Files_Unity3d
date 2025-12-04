using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class SC_GameLogic : MonoBehaviour {
    [SerializeField] private InputManager _inputManager;

    private Dictionary<string, GameObject> unityObjects;
    private int score = 0;
    private float displayScore = 0;
    private GameBoard gameBoard;
    private GlobalEnums.GameState currentState = GlobalEnums.GameState.move;

    private SC_Gem[,] gems;

    public GlobalEnums.GameState CurrentState {
        get { return currentState; }
    }

    #region MonoBehaviour

    private void Awake() {
        Init();
    }

    private void Start() {
        StartGame();
    }

    private void Update() {
        displayScore = Mathf.Lerp(displayScore, gameBoard.Score, SC_GameVariables.Instance.scoreSpeed * Time.deltaTime);
        unityObjects["Txt_Score"].GetComponent<TMPro.TextMeshProUGUI>().text = displayScore.ToString("0");
    }

    #endregion

    #region Logic

    private void Init() {
        if (_inputManager == null) {
            _inputManager = FindObjectOfType<InputManager>();
        }

        if (_inputManager == null) {
            throw new NullReferenceException($"[{nameof(SC_GameLogic)}] InputManager is missing)");
        }

        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _obj = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _obj)
            unityObjects.Add(g.name, g);

        gameBoard = new GameBoard(SC_GameVariables.Instance.colsSize, SC_GameVariables.Instance.rowsSize,
            new GemGenerator(SC_GameVariables.Instance));

        Setup();
    }

    private void Setup() {
        gems = new SC_Gem[gameBoard.Width, gameBoard.Height];

        for (int x = 0; x < gameBoard.Width; x++)
            for (int y = 0; y < gameBoard.Height; y++) {
                var pos = new Vector2Int(x, y);
                if (gameBoard.TryGetGem(pos, out var gemType)) {
                    gems[x, y] = SpawnGem(pos, gemType);
                }
            }
    }

    public void StartGame() {
        unityObjects["Txt_Score"].GetComponent<TextMeshProUGUI>().text = score.ToString("0");
    }

    private SC_Gem SpawnGem(Vector2Int pos, GlobalEnums.GemType gemType) {
        var gemPrefab = gemType == GlobalEnums.GemType.bomb
            ? SC_GameVariables.Instance.bomb
            : SC_GameVariables.Instance.gems.First(g => g.type == gemType);

        var gem = Instantiate(gemPrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
        gem.transform.SetParent(unityObjects["GemsHolder"].transform);
        gem.name = "Gem - " + pos.x + ", " + pos.y;
        return gem;
    }

    public void SetState(GlobalEnums.GameState _CurrentState) {
        currentState = _CurrentState;
    }

    // public void DestroyMatches() {
    //     for (int i = 0; i < gameBoard.CurrentMatches.Count; i++)
    //         if (gameBoard.CurrentMatches[i] != null) {
    //             ScoreCheck(gameBoard.CurrentMatches[i]);
    //             DestroyMatchedGemsAt(gameBoard.CurrentMatches[i].posIndex);
    //         }
    //
    //     StartCoroutine(DecreaseRowCo());
    // }
    //
    // private IEnumerator DecreaseRowCo() {
    //     yield return new WaitForSeconds(.2f);
    //
    //     int nullCounter = 0;
    //     for (int x = 0; x < gameBoard.Width; x++) {
    //         for (int y = 0; y < gameBoard.Height; y++) {
    //             SC_Gem _curGem = gameBoard.GetGem(x, y);
    //             if (_curGem == null) {
    //                 nullCounter++;
    //             } else if (nullCounter > 0) {
    //                 _curGem.posIndex.y -= nullCounter;
    //                 SetGem(x, y - nullCounter, _curGem);
    //                 SetGem(x, y, null);
    //             }
    //         }
    //
    //         nullCounter = 0;
    //     }
    //
    //     StartCoroutine(FilledBoardCo());
    // }

    public void ScoreCheck(SC_Gem gemToCheck) {
        gameBoard.Score += gemToCheck.scoreValue;
    }

    // private void DestroyMatchedGemsAt(Vector2Int _Pos) {
    //     SC_Gem _curGem = gameBoard.GetGem(_Pos.x, _Pos.y);
    //     if (_curGem != null) {
    //         Instantiate(_curGem.destroyEffect, new Vector2(_Pos.x, _Pos.y), Quaternion.identity);
    //
    //         Destroy(_curGem.gameObject);
    //         SetGem(_Pos.x, _Pos.y, null);
    //     }
    // }

    // private IEnumerator FilledBoardCo() {
    //     yield return new WaitForSeconds(0.5f);
    //     RefillBoard();
    //     yield return new WaitForSeconds(0.5f);
    //     gameBoard.FindAllMatches();
    //     if (gameBoard.CurrentMatches.Count > 0) {
    //         yield return new WaitForSeconds(0.5f);
    //         DestroyMatches();
    //     } else {
    //         yield return new WaitForSeconds(0.5f);
    //         currentState = GlobalEnums.GameState.move;
    //     }
    // }

    // private void RefillBoard() {
    //     for (int x = 0; x < gameBoard.Width; x++) {
    //         for (int y = 0; y < gameBoard.Height; y++) {
    //             SC_Gem _curGem = gameBoard.GetGem(x, y);
    //             if (_curGem == null) {
    //                 // int gemToUse = Random.Range(0, SC_GameVariables.Instance.gems.Length);
    //                 // SpawnGem(new Vector2Int(x, y), SC_GameVariables.Instance.gems[gemToUse]);
    //                 int _gemToUse = Random.Range(0, SC_GameVariables.Instance.gems.Length);
    //
    //                 int iterations = 0;
    //                 while (gameBoard.MatchesAt(new Vector2Int(x, y), SC_GameVariables.Instance.gems[_gemToUse]) &&
    //                        iterations < 100) {
    //                     _gemToUse = Random.Range(0, SC_GameVariables.Instance.gems.Length);
    //                     iterations++;
    //                 }
    //
    //                 SpawnGem(new Vector2Int(x, y), SC_GameVariables.Instance.gems[_gemToUse]);
    //             }
    //         }
    //     }
    //
    //     CheckMisplacedGems();
    // }
    //
    // private void CheckMisplacedGems() {
    //     List<SC_Gem> foundGems = new List<SC_Gem>();
    //     foundGems.AddRange(FindObjectsOfType<SC_Gem>());
    //     for (int x = 0; x < gameBoard.Width; x++) {
    //         for (int y = 0; y < gameBoard.Height; y++) {
    //             SC_Gem _curGem = gameBoard.GetGem(x, y);
    //             if (foundGems.Contains(_curGem))
    //                 foundGems.Remove(_curGem);
    //         }
    //     }
    //
    //     foreach (SC_Gem g in foundGems)
    //         Destroy(g.gameObject);
    // }

    // public void FindAllMatches() {
    //     gameBoard.FindAllMatches();
    // }

    #endregion

    private void OnEnable() {
        _inputManager.Swiped += OnSwiped;
    }

    private async void OnSwiped(SwipeArgs args) {
        var selectedGemPos = args.startPos.ToVector2Int();
        if (!gameBoard.IsValidPos(selectedGemPos)) {
            return;
        }

        var destGemPos = args.swipeDirection switch {
            SwipeDirection.Right => new Vector2Int(selectedGemPos.x + 1, selectedGemPos.y),
            SwipeDirection.Left => new Vector2Int(selectedGemPos.x - 1, selectedGemPos.y),
            SwipeDirection.Up => new Vector2Int(selectedGemPos.x, selectedGemPos.y + 1),
            SwipeDirection.Down => new Vector2Int(selectedGemPos.x, selectedGemPos.y - 1),
            _ => new Vector2Int(-1, -1)
        };

        if (!gameBoard.IsValidPos(destGemPos)) {
            return;
        }

        var selectedGem = gems[selectedGemPos.x, selectedGemPos.y];
        var destGem = gems[destGemPos.x, destGemPos.y];

        var hasMatch = gameBoard.TryResolve(selectedGemPos, destGemPos, out var resolvedResult);

        await SwapGems(selectedGem, destGem);

        if (!hasMatch) {
            await SwapGems(selectedGem, destGem);
            return;
        }

        (gems[selectedGemPos.x, selectedGemPos.y], gems[destGemPos.x, destGemPos.y]) = (
            gems[destGemPos.x, destGemPos.y], gems[selectedGemPos.x, selectedGemPos.y]);

        foreach (var collected in resolvedResult.CollectedGems) {
            Destroy(gems[collected.x, collected.y].gameObject);
        }

        foreach (var change in resolvedResult.Changes) {
            var gem = change.wasCreated
                ? SpawnGem(change.toPos, change.gemType)
                : gems[change.fromPos.x, change.fromPos.y];

            gems[change.toPos.x, change.toPos.y] = gem;

            var tween = gem.transform
                .DOMove(new Vector3(change.toPos.x, change.toPos.y, 0), .15f + change.toPos.x * .1f)
                .From(new Vector3(change.fromPos.x, change.fromPos.y, 0))
                .SetEase(Ease.InOutQuad);

            if (change.wasCreated) {
                gem.gameObject.SetActive(false);
                tween.SetDelay(change.creationTime * .1f)
                    .OnStart(() => gem.gameObject.SetActive(true));
            }
        }
    }

    public async Task SwapGems(SC_Gem gem1, SC_Gem gem2, CancellationToken ct = default) {
        var gem1Pos = gem1.transform.position;
        var gem2Pos = gem2.transform.position;
        var selectedGemMoveTween = gem1.transform
            .DOMove(gem2Pos, SC_GameVariables.Instance.gemSwapDuration)
            .SetEase(SC_GameVariables.Instance.gemSwapEase);
        var destGemGemMoveTween = gem2.transform
            .DOMove(gem1Pos, SC_GameVariables.Instance.gemSwapDuration)
            .SetEase(SC_GameVariables.Instance.gemSwapEase);

        var completion = new TaskCompletionSource<bool>();

        DOTween.Sequence()
            .Join(selectedGemMoveTween)
            .Join(destGemGemMoveTween)
            .OnComplete(() => completion.SetResult(true));

        await completion.Task;
    }

    private void OnDisable() {
        _inputManager.Swiped -= OnSwiped;
    }
}