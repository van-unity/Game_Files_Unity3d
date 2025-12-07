using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour {
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private GemRepository _gemRepository;
    [SerializeField] private BoardViewSettings _boardViewSettings;
    [SerializeField] private GameplayScreen _gameplayScreen;
    
    private Dictionary<string, GameObject> unityObjects;
    private int score = 0;
    private float displayScore = 0;
    private Board _board;
    private BoardView _boardView;

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
    }

    // private void Update() {
    //     displayScore = Mathf.Lerp(displayScore, gameBoard.Score, SC_GameVariables.Instance.scoreSpeed * Time.deltaTime);
    //     unityObjects["Txt_Score"].GetComponent<TMPro.TextMeshProUGUI>().text = displayScore.ToString("0");
    // }

    private void Init() {
        _gemAbilityProvider = new GemAbilityProvider();
        _gemPool = new GemPool(_gemRepository);

        if (_inputManager == null) {
            _inputManager = FindObjectOfType<InputManager>();
        }

        if (_inputManager == null) {
            throw new NullReferenceException($"[{nameof(GameController)}] InputManager is missing)");
        }

        unityObjects = new Dictionary<string, GameObject>();
        GameObject[] _obj = GameObject.FindGameObjectsWithTag("UnityObject");
        foreach (GameObject g in _obj)
            unityObjects.Add(g.name, g);

        _matchCheckStrategy = new MatchCheckStrategy();
        
        var boardInitializeStrategy = new BoardInitializeStrategy(_matchCheckStrategy, _gemRepository);
        var boardRefillStrategy = new BoardRefillStrategy(_matchCheckStrategy, _gemRepository);
        
        _board = new Board(
            SC_GameVariables.Instance.colsSize,
            SC_GameVariables.Instance.rowsSize,
            boardInitializeStrategy,
            boardRefillStrategy,
            _matchCheckStrategy
        );

        _boardView = new BoardView(_board, _gemPool, _boardViewSettings);

        Setup();
    }

    private void Setup() {
        _board.Initialize();
        _boardView.Initialize();
    }

    public void StartGame() {
        _canMove = true;
        score = 0;
        _gameplayScreen.UpdateScore(score);
    }

    private void OnEnable() {
        _inputManager.Swiped += OnSwiped;
    }

    private async void OnSwiped(SwipeArgs args) {
        await HandleSwipe(args, _cts.Token);
    }

    private async Task HandleSwipe(SwipeArgs args, CancellationToken ct = default) {
        if (!_canMove) {
            return;
        }

        _canMove = false;

        try {
            if (!_board.TryParseMousePos(args.startPos, out var selectedGemPos)) {
                return;
            }

            var destGemPos = args.swipeDirection switch {
                SwipeDirection.Right => new Vector2Int(selectedGemPos.x + 1, selectedGemPos.y),
                SwipeDirection.Left => new Vector2Int(selectedGemPos.x - 1, selectedGemPos.y),
                SwipeDirection.Up => new Vector2Int(selectedGemPos.x, selectedGemPos.y + 1),
                SwipeDirection.Down => new Vector2Int(selectedGemPos.x, selectedGemPos.y - 1),
                _ => new Vector2Int(-1, -1)
            };

            if (!_board.IsValidPos(destGemPos)) {
                return;
            }

            if (!_board.TrySwapGems(selectedGemPos, destGemPos)) {
                return;
            }

            var isMatch = _board.HasMatchAt(selectedGemPos) || _board.HasMatchAt(destGemPos);

            await _boardView.SwapGems(selectedGemPos, destGemPos, ct);

            if (!isMatch) {
                _board.TrySwapGems(selectedGemPos, destGemPos);
                await _boardView.SwapGems(selectedGemPos, destGemPos, ct);
                return;
            }

            await HandleMatches(ct);
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
        finally {
            _canMove = true;
        }
    }

    private async Task HandleMatches(CancellationToken ct = default) {
        while (_board.TryResolveMatches(out var matches)) {
            if (ct.IsCancellationRequested) {
                throw new OperationCanceledException();
            }

            var abilityTasks = new List<Task>();

            foreach (var collectedGems in matches) {
                var ability = _gemAbilityProvider.GetGemAbility(collectedGems.gem.Type);
                if (ability == null) {
                    continue;
                }

                abilityTasks.Add(ability.Execute(matches, _board, _boardView, ct));
            }

            await Task.WhenAll(abilityTasks);

            score  += matches.Sum(m => m.gem.ScoreValue);
            _gameplayScreen.UpdateScore(score);
            await _boardView.DestroyMatches(matches.Select(m => m.position), ct);

            //refil board and check for matches again
            var refillResult = _board.Refill(matches);
            await _boardView.UpdateGems(refillResult, ct);
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