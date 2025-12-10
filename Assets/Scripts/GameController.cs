using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class GameController : IDisposable{
    private readonly CancellationTokenSource _cts;
    private readonly Board _board;
    private readonly BoardView _boardView;
    private readonly GemAbilityProvider _abilityProvider;
    private readonly InputManager _inputManager;
    private readonly GameplayScreen _gameplayScreen;
    
    private bool _canMove;
    private int _score;

    public GameController(Board board, BoardView boardView, GemAbilityProvider abilityProvider, InputManager inputManager, GameplayScreen gameplayScreen) {
        _cts = new CancellationTokenSource();
        _board = board;
        _boardView = boardView;
        _abilityProvider = abilityProvider;
        _inputManager = inputManager;
        _gameplayScreen = gameplayScreen;

        Setup();
        StartGame();
        _inputManager.Swiped += OnSwiped;
    }

    private void Setup() {
        _board.Initialize();
        _boardView.Initialize();
    }

    public void StartGame() {
        _canMove = true;
        _score = 0;
        _gameplayScreen.UpdateScore(_score);
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
                var ability = _abilityProvider.GetGemAbility(collectedGems.gem.Type);
                if (ability == null) {
                    continue;
                }
            
                abilityTasks.Add(ability.Execute(matches, _board, _boardView, ct));
            }
            
            await Task.WhenAll(abilityTasks);

            _score  += matches.Sum(m => m.gem.ScoreValue);
            _gameplayScreen.UpdateScore(_score);
            await _boardView.DestroyMatches(matches.Select(m => m.position), ct);

            //refil board and check for matches again
            var refillResult = _board.Refill(matches);
            await _boardView.UpdateGems(refillResult, ct);
        }
    }

    public void Dispose() {
        try {
            _cts.Cancel();
            _cts.Dispose();
            _inputManager.Swiped -= OnSwiped;
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }
}