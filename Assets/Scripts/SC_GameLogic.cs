using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class SC_GameLogic : MonoBehaviour {
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private GemPrefabRepository _gemPrefabRepository;

    private Dictionary<string, GameObject> unityObjects;
    private int score = 0;
    private float displayScore = 0;
    private GameBoard gameBoard;
    private GameObject[,] gems;
    private CancellationTokenSource _cts = new();
    private GemPool _gemPool;
    private bool _canMove;

    #region MonoBehaviour

    private void Awake() {
        Init();
    }

    private void Start() {
        StartGame();
        _canMove = true;
    }

    private void Update() {
        displayScore = Mathf.Lerp(displayScore, gameBoard.Score, SC_GameVariables.Instance.scoreSpeed * Time.deltaTime);
        unityObjects["Txt_Score"].GetComponent<TMPro.TextMeshProUGUI>().text = displayScore.ToString("0");
    }

    #endregion

    #region Logic

    private void Init() {
        _gemPool = new GemPool(_gemPrefabRepository);

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
        gameBoard.Initialize();
        Setup();
    }

    private void Setup() {
        gems = new GameObject[gameBoard.Width, gameBoard.Height];

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

    private GameObject SpawnGem(Vector2Int pos, GemType gemType) {
        var gem = _gemPool.GetGem(gemType);
        gem.transform.position = new Vector3(pos.x, pos.y, 0);
        gem.transform.SetParent(unityObjects["GemsHolder"].transform);
        gem.name = "Gem - " + pos.x + ", " + pos.y;
        return gem;
    }

    #endregion

    private void OnEnable() {
        _inputManager.Swiped += OnSwiped;
    }

    private async void OnSwiped(SwipeArgs args) {
        await HandleSwipe(args);
    }

    private async Task HandleSwipe(SwipeArgs args, CancellationToken ct = default) {
        if (!_canMove) {
            return;
        }

        _canMove = false;

        try {
            var selectedGemPos = args.startPos.ToVector2Int();

            var destGemPos = args.swipeDirection switch {
                SwipeDirection.Right => new Vector2Int(selectedGemPos.x + 1, selectedGemPos.y),
                SwipeDirection.Left => new Vector2Int(selectedGemPos.x - 1, selectedGemPos.y),
                SwipeDirection.Up => new Vector2Int(selectedGemPos.x, selectedGemPos.y + 1),
                SwipeDirection.Down => new Vector2Int(selectedGemPos.x, selectedGemPos.y - 1),
                _ => new Vector2Int(-1, -1)
            };

            if (!gameBoard.IsValidPos(selectedGemPos) || !gameBoard.IsValidPos(destGemPos)) {
                return;
            }

            var swapResult = await IsMatch(selectedGemPos, destGemPos, ct);

            if (swapResult) {
                await HandleMatches(ct);
            }
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
        finally {
            _canMove = true;
        }
    }

    private async Task<bool> IsMatch(Vector2Int pos1, Vector2Int pos2, CancellationToken ct = default) {
        if (!gameBoard.TrySwapGems(pos1, pos2)) {
            return false;
        }

        var gem1 = gems[pos1.x, pos1.y].transform;
        var gem2 = gems[pos2.x, pos2.y].transform;

        var isMatch = gameBoard.MatchesAt(pos1, gameBoard.GetAt(pos1)) ||
                      gameBoard.MatchesAt(pos2, gameBoard.GetAt(pos2));

        await SwapAnimation(gem1, gem2, ct);

        if (!isMatch) {
            gameBoard.TrySwapGems(pos1, pos2);
            await SwapAnimation(gem1, gem2, ct);
            return false;
        }

        (gems[pos1.x, pos1.y], gems[pos2.x, pos2.y]) = (gems[pos2.x, pos2.y], gems[pos1.x, pos1.y]);

        return true;
    }

    public async Task SwapAnimation(Transform gem1, Transform gem2, CancellationToken ct = default) {
        var gem1Pos = gem1.position;
        var gem2Pos = gem2.position;
        var selectedGemMoveTween = gem1
            .DOMove(gem2Pos, SC_GameVariables.Instance.gemSwapDuration)
            .SetEase(SC_GameVariables.Instance.gemSwapEase);
        var destGemGemMoveTween = gem2
            .DOMove(gem1Pos, SC_GameVariables.Instance.gemSwapDuration)
            .SetEase(SC_GameVariables.Instance.gemSwapEase);

        var completion = new TaskCompletionSource<bool>();

        var sequence = DOTween.Sequence()
            .Join(selectedGemMoveTween)
            .Join(destGemGemMoveTween)
            .OnUpdate(() => {
                if (ct.IsCancellationRequested) {
                    throw new OperationCanceledException();
                }
            })
            .OnComplete(() => completion.SetResult(true));

        await completion.Task;
    }

    private async Task HandleMatches(CancellationToken ct = default) {
        while (gameBoard.TryResolve(out var resolvedResult)) {
            if (ct.IsCancellationRequested) {
                throw new OperationCanceledException();
            }

            var destroySequence = DOTween.Sequence();

            foreach (var collected in resolvedResult.CollectedGems) {
                var gemType = collected.gemType;
                var pos = collected.position;

                _gemPool.ReleaseGem(gemType, gems[pos.x, pos.y]);

                var destroyEffect = _gemPool.GetDestroyEffect(gemType);
                var duration = _gemPrefabRepository.GetDestroyEffect(gemType).DestroyEffectDuration;

                destroyEffect.transform.position = new Vector3(pos.x, pos.y, 0);
                destroyEffect.SetActive(true);

                destroySequence.Join(
                    DOTween.Sequence()
                        .AppendInterval(duration)
                        .AppendCallback(() => { _gemPool.ReleaseDestroyEffect(gemType, destroyEffect); })
                );
            }

            var completion = new TaskCompletionSource<bool>();
            var sequence = DOTween.Sequence();
            foreach (var change in resolvedResult.Changes) {
                var gem = change.wasCreated
                    ? SpawnGem(change.toPos, change.gemType)
                    : gems[change.fromPos.x, change.fromPos.y];

                gems[change.toPos.x, change.toPos.y] = gem;
                var distance = Mathf.Abs(change.fromPos.y - change.toPos.y);
                var tween = gem.transform
                    .DOMove(new Vector3(change.toPos.x, change.toPos.y, 0), 0.25f + change.creationTime * .1f)
                    .From(new Vector3(change.fromPos.x, change.fromPos.y, 0))
                    .SetEase(Ease.OutSine);

                if (change.wasCreated) {
                    gem.gameObject.SetActive(false);
                    tween
                        .OnStart(() => gem.gameObject.SetActive(true));
                }

                sequence.Join(tween);
            }

            sequence
                .OnUpdate(() => {
                    if (ct.IsCancellationRequested) {
                        throw new OperationCanceledException();
                    }
                })
                .OnComplete(() => { completion.SetResult(true); });

            await completion.Task;

            await Task.Delay(100, ct);
        }
    }

    private void OnDisable() {
        _inputManager.Swiped -= OnSwiped;
    }

    private void OnDestroy() {
        try {
            _cts.Cancel();
            _cts.Dispose();
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }
}