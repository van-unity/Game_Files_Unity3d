using System;
using Pools;
using UnityEngine;

public class GameInitializer : MonoBehaviour {
    [SerializeField] private int _boardWidth = 7;
    [SerializeField] private int _boardHeight = 7;
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private GemRepository _gemRepository;
    [SerializeField] private BoardViewSettings _boardViewSettings;
    [SerializeField] private GameplayScreen _gameplayScreen;
    [SerializeField] private SpawnableGameObject _gemBackgroundPrefab;

    private GameController _controller;
    
    private void Start() {
        var matchCheckStrategy = new MatchCheckStrategy();
        var boardInitializeStrategy = new BoardInitializeStrategy(matchCheckStrategy, _gemRepository);
        var boardRefillStrategy = new BoardRefillStrategy(matchCheckStrategy, _gemRepository);
        var gemPool = new GemPool(_gemRepository);
        var gemBackgroundPool = new GameObjectPool(_gemBackgroundPrefab, _boardWidth * _boardHeight, 10);
        
        var board = new Board(_boardWidth, _boardHeight, boardInitializeStrategy, boardRefillStrategy, matchCheckStrategy);
        var boardView = new BoardView(board, gemPool, _boardViewSettings, gemBackgroundPool);
        var abilityProvider = new GemAbilityProvider();
        
        _controller = new GameController(board, boardView, abilityProvider, _inputManager, _gameplayScreen);
    }

    private void OnDestroy() {
        _controller?.Dispose();
    }
}