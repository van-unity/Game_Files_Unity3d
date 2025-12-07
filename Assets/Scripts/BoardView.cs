using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "Board-View-Settings", menuName = "Match3/Board View Settings")]
public class BoardViewSettings : ScriptableObject {
    [field: SerializeField] public float GemFallDuration { get; private set; } = 0.25f;
    [field: SerializeField] public float GemSwapDuration { get; private set; } = 0.25f;
    [field: SerializeField] public float GemFallDelay { get; private set; } = .1f;
    [field: SerializeField] public Ease GemFallEase { get; private set; } = Ease.OutSine;
    [field: SerializeField] public Ease GemSwapEase { get; private set; } = Ease.OutCirc;
    [field: SerializeField] public int DestroyDelayMS { get; private set; } = 100;
    [field: SerializeField] public int UpdateDelayMS { get; private set; } = 100;
}

public class BoardView {
    private readonly Board _board;
    private readonly GemView[,] _gems;
    private readonly GemPool _gemPool;
    private readonly BoardViewSettings _settings;
    private readonly Transform _container;

    public BoardView(Board board, GemPool gemPool, BoardViewSettings settings) {
        _board = board;
        _gemPool = gemPool;
        _settings = settings;

        _container = new GameObject("BoardView").transform;

        _gems = new GemView[board.Width, board.Height];
    }

    public void Initialize() {
        var width = _board.Width;
        var height = _board.Height;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                var gem = _board.GetAt(x, y);
                if (gem == null) {
                    continue;
                }

                var pos = new Vector2Int(x, y);
                SpawnGem(pos, gem);
            }
        }
    }

    public GemView GetGemAt(int x, int y) => !_board.IsValidPos(x, y) ? null : _gems[x, y];

    public GemView GetGemAt(Vector2Int pos) => !_board.IsValidPos(pos.x, pos.y) ? null : _gems[pos.x, pos.y];

    public void SetGemAt(int x, int y, GemView gemView) {
        if (!_board.IsValidPos(x, y)) {
            return;
        }
        
        _gems[x, y] = gemView;
    }

    public void SetGemAt(Vector2Int pos, GemView gemView) {
        if (!_board.IsValidPos(pos.x, pos.y)) {
            return;
        }
        
        _gems[pos.x, pos.y] = gemView;
    }
    
    public async Task SwapGems(Vector2Int gem1Indices, Vector2Int gem2Indices, CancellationToken ct = default) {
        var gem1 = GetGemAt(gem1Indices);
        var gem2 = GetGemAt(gem2Indices);

        if (!gem1 || !gem2) {
            return;
        }

        var gem1Pos = gem1.Transform.position;
        var gem2Pos = gem2.Transform.position;
        var selectedGemMoveTween = gem1
            .Transform
            .DOMove(gem2Pos, _settings.GemSwapDuration)
            .SetEase(_settings.GemSwapEase);
        var destGemGemMoveTween = gem2
            .Transform
            .DOMove(gem1Pos, _settings.GemSwapDuration)
            .SetEase(_settings.GemSwapEase);

        var completion = new TaskCompletionSource<bool>();

        DOTween.Sequence()
            .Join(selectedGemMoveTween)
            .Join(destGemGemMoveTween)
            .OnUpdate(() => {
                if (ct.IsCancellationRequested) {
                    throw new OperationCanceledException();
                }
            })
            .OnComplete(() => completion.SetResult(true));

        await completion.Task;
        
        (_gems[gem1Indices.x, gem1Indices.y], _gems[gem2Indices.x, gem2Indices.y]) = (
            _gems[gem2Indices.x, gem2Indices.y], _gems[gem1Indices.x, gem1Indices.y]);
    }
    
    public async Task DestroyMatches(IEnumerable<Vector2Int> positions, CancellationToken ct = default) {
        foreach (var pos in positions) {
            var gemView = GetGemAt(pos);

            if (!gemView) {
                continue;
            }

            var destroyEffect = _gemPool.GetDestroyEffect(gemView.Gem);
            destroyEffect.Transform.position = gemView.Transform.position;

            _gemPool.ReleaseGem(gemView);
            _gems[pos.x, pos.y] = null;
        }

        await Task.Delay(_settings.DestroyDelayMS, ct);
    }

    public async Task UpdateGems(List<ChangeInfo> changes, CancellationToken ct = default) {
        var completion = new TaskCompletionSource<bool>();
        var sequence = DOTween.Sequence();
        foreach (var change in changes) {
            GemView gemView;
            if (change.wasCreated) {
                SpawnGem(change.toPos, change.gem);
                gemView = GetGemAt(change.toPos);
            } else {
                gemView = GetGemAt(change.fromPos.x, change.fromPos.y);
            }
            
            if (!gemView) {
                continue;
            }
            
            SetGemAt(change.toPos, gemView);
            
            var tween = gemView.Transform
                .DOMove(new Vector3(change.toPos.x, change.toPos.y, 0), _settings.GemFallDuration)
                .SetDelay(change.creationTime * _settings.GemFallDelay)
                .From(new Vector3(change.fromPos.x, change.fromPos.y, 0))
                .SetEase(Ease.OutSine);

            if (change.wasCreated) {
                gemView.GameObject.SetActive(false);
                tween.OnStart(() => gemView.GameObject.SetActive(true));
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

        await Task.Delay(_settings.UpdateDelayMS, ct);
    }

    public void SpawnGem(Vector2Int pos, Gem gem) {
        var gemView = _gemPool.GetGem(gem);
        gemView.Bind(gem);
        gemView.Transform.position = new Vector3(pos.x, pos.y, 0);
        gemView.Transform.SetParent(_container);
        gemView.name = "Gem - " + pos.x + ", " + pos.y;
        _gems[pos.x, pos.y] = gemView;
    }
}