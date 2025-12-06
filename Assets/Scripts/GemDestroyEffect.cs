using System.Collections;
using Pools;
using UnityEngine;

public class GemDestroyEffect : SpawnableMonoBehaviour<GemDestroyEffect> {
    [SerializeField] private float _returnDelay = 1f;

    private YieldInstruction _returnYieldInstruction;

    private void Awake() {
        _returnYieldInstruction = new WaitForSeconds(_returnDelay);
    }

    public override void OnSpawn() {
        base.OnSpawn();

        StartCoroutine(WaitAndDestroy());
    }

    private IEnumerator WaitAndDestroy() {
        yield return _returnYieldInstruction;
        Return();
    }
}