using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using Pools;
using UnityEngine;

public class BoardView {
    private readonly Board _board;
    private readonly GemView[,] _gems;
    private readonly GemPool _gemPool;
    private readonly BoardViewSettings _settings;
    private readonly IObjectPool<SpawnableGameObject> _gemBackgroundPool;
    private readonly Transform _container;

    public BoardView(Board board, GemPool gemPool, BoardViewSettings settings,
        IObjectPool<SpawnableGameObject> gemBackgroundPool) {
        _board = board;
        _gemPool = gemPool;
        _settings = settings;
        _gemBackgroundPool = gemBackgroundPool;

        _container = new GameObject("BoardView").transform;

        _gems = new GemView[board.Width, board.Height];
    }

    public void Initialize() {
        _gemBackgroundPool.WarmUp();

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
                SpawnGemBackground(pos);
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

    public async Task DestroyMatches(IEnumerable<CollectedGemInfo> infos, CancellationToken ct = default) {
        var tasks = new List<Task>();

        foreach (var info in infos) {
            var gemView = GetGemAt(info.position);
            if (!gemView) continue;

            SetGemAt(info.position, null);

            var tcs = new TaskCompletionSource<bool>();

            TweenCallback callback = () => {
                _gemPool.ReleaseGem(gemView);
                var destroyEffect = _gemPool.GetDestroyEffect(gemView.Gem);
                destroyEffect.Transform.position = gemView.Transform.position;

                tcs.SetResult(true);
            };

            if (info.destroyDelay > 0) {
                DOTween.Sequence()
                    .AppendInterval(info.destroyDelay / 1000f)
                    .AppendCallback(callback);
            } else {
                callback();
            }

            tasks.Add(tcs.Task);
        }

        await Task.WhenAll(tasks);
    }


    public async Task UpdateGems(List<ChangeInfo> changes, CancellationToken ct = default) {
        var completion = new TaskCompletionSource<bool>();
        var sequence = DOTween.Sequence();

        foreach (var change in changes) {
            GemView gemView;
            if (change.wasCreated) {
                SpawnGem(change.toPos, change.gem);
                gemView = GetGemAt(change.toPos);
                gemView.GameObject.SetActive(false);
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
                .SetEase(_settings.GemFallEase);

            if (change.wasCreated) {
                tween
                    .OnStart(() => gemView.GameObject.SetActive(true));
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

    private void SpawnGemBackground(Vector2Int pos) {
        var gemBg = _gemBackgroundPool.Get();
        gemBg.Transform.position = new Vector3(pos.x, pos.y, 0);
        gemBg.name = "Gem - " + pos.x + ", " + pos.y + " - Background";
        gemBg.Transform.SetParent(_container);
    }
}