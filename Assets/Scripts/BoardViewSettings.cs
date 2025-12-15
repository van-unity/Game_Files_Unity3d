using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "Board-View-Settings", menuName = "Match3/Board View Settings")]
public class BoardViewSettings : ScriptableObject {
    [field: SerializeField] public float GemFallDuration { get; private set; } = .1f;
    [field: SerializeField] public float GemSwapDuration { get; private set; } = 0.25f;
    [field: SerializeField] public float GemFallDelay { get; private set; } = .1f;
    [field: SerializeField] public Ease GemFallEase { get; private set; } = Ease.OutSine;
    [field: SerializeField] public Ease GemSwapEase { get; private set; } = Ease.OutCirc;
    [field: SerializeField] public int DestroyDelayMS { get; private set; } = 100;
    [field: SerializeField] public int UpdateDelayMS { get; private set; } = 100;
}