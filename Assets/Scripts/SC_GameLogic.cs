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
    private GemView[,] gems;
    private CancellationTokenSource _cts = new();
    private GemPool _gemPool;
    private bool _canMove;
    private IMatchCheckStrategy _matchCheckStrategy;
    private GemAbilityProvider _gemAbilityProvider;

    private void Awake() {
        Init();
    }

    private void Start() {
        StartGame();
        _canMove = true;
    }

    // private void Update() {
    //     displayScore = Mathf.Lerp(displayScore, gameBoard.Score, SC_GameVariables.Instance.scoreSpeed * Time.deltaTime);
    //     unityObjects["Txt_Score"].GetComponent<TMPro.TextMeshProUGUI>().text = displayScore.ToString("0");
    // }

    private void Init() {
        _gemAbilityProvider = new GemAbilityProvider();
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

        _matchCheckStrategy = new MatchCheckStrategy();

        gameBoard = new GameBoard(
            SC_GameVariables.Instance.colsSize,
            SC_GameVariables.Instance.rowsSize,
            new BoardInitializeStrategy(_matchCheckStrategy),
            new BoardRefillStrategy(_matchCheckStrategy),
            _matchCheckStrategy
        );

        Setup();
    }

    private void Setup() {
        gems = new GemView[gameBoard.Width, gameBoard.Height];

        for (int x = 0; x < gameBoard.Width; x++)
            for (int y = 0; y < gameBoard.Height; y++) {
                var gem = gameBoard.GetAt(x, y);
                if (gem == null) {
                    continue;
                }

                var pos = new Vector2Int(x, y);
                gems[x, y] = SpawnGem(pos, gem);
            }
    }

    public void StartGame() {
        unityObjects["Txt_Score"].GetComponent<TextMeshProUGUI>().text = score.ToString("0");
    }

    private GemView SpawnGem(Vector2Int pos, Gem gem) {
        var gemView = _gemPool.GetGem(gem);
        gemView.Bind(gem);
        gemView.Transform.position = new Vector3(pos.x, pos.y, 0);
        gemView.Transform.SetParent(unityObjects["GemsHolder"].transform);
        gemView.name = "Gem - " + pos.x + ", " + pos.y;
        return gemView;
    }

    public GemView GetGemAt(int x, int y) => !gameBoard.IsValidPos(x, y) ? null : gems[x, y];

    public GemView GetGemAt(Vector2Int pos) => !gameBoard.IsValidPos(pos.x, pos.y) ? null : gems[pos.x, pos.y];


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
            if (!gameBoard.TryParseMousePos(args.startPos, out var selectedGemPos)) {
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

            if (!gameBoard.TrySwapGems(selectedGemPos, destGemPos)) {
                return;
            }

            var gem1 = gems[selectedGemPos.x, selectedGemPos.y];
            var gem2 = gems[destGemPos.x, destGemPos.y];

            var isMatch = gameBoard.IsValidMove(selectedGemPos, destGemPos);

            await SwapAnimation(gem1.Transform, gem2.Transform, ct);

            if (!isMatch) {
                gameBoard.TrySwapGems(selectedGemPos, destGemPos);
                await SwapAnimation(gem1.Transform, gem2.Transform, ct);
                return;
            }

            (gems[selectedGemPos.x, selectedGemPos.y], gems[destGemPos.x, destGemPos.y]) = (
                gems[destGemPos.x, destGemPos.y], gems[selectedGemPos.x, selectedGemPos.y]);

            await HandleMatches(ct);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
        finally {
            _canMove = true;
        }
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
        while (gameBoard.TryResolveMatches(out var matches)) {
            if (ct.IsCancellationRequested) {
                throw new OperationCanceledException();
            }

            var abilityTasks = new List<Task>();

            foreach (var collectedGems in matches) {
                var ability = _gemAbilityProvider.GetGemAbility(collectedGems.gem.Type);
                if (ability == null) {
                    continue;
                }

                abilityTasks.Add(ability.Execute(matches, gameBoard, this, ct));
            }

            await Task.WhenAll(abilityTasks);

            await DestroyMatches(matches, ct);

            //refil board and check for matches again
            var refilResult = gameBoard.Refill(matches);
            var completion = new TaskCompletionSource<bool>();
            var sequence = DOTween.Sequence();
            foreach (var change in refilResult) {
                var gem = change.wasCreated
                    ? SpawnGem(change.toPos, change.gem)
                    : gems[change.fromPos.x, change.fromPos.y];

                gems[change.toPos.x, change.toPos.y] = gem;

                //abilities may have already destroyed the gem
                if (!gem) {
                    continue;
                }

                var tween = gem.Transform
                    .DOMove(new Vector3(change.toPos.x, change.toPos.y, 0), 0.25f + change.creationTime * .1f)
                    .From(new Vector3(change.fromPos.x, change.fromPos.y, 0))
                    .SetEase(Ease.OutSine);

                if (change.wasCreated) {
                    gem.GameObject.SetActive(false);
                    tween.OnStart(() => gem.GameObject.SetActive(true));
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

    public async Task DestroyMatches(IEnumerable<CollectedGemInfo> collectedGems, CancellationToken ct = default) {
        foreach (var collected in collectedGems) {
            var pos = collected.position;
            var gemView = GetGemAt(pos);

            if (!gemView) {
                continue;
            }

            var destroyEffect = _gemPool.GetDestroyEffect(gemView.Gem);
            destroyEffect.Transform.position = new Vector3(pos.x, pos.y, 0);

            _gemPool.ReleaseGem(gems[pos.x, pos.y]);
            gems[pos.x, pos.y] = null;
        }

        await Task.Delay(100, ct);
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