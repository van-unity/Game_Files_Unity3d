using System;
using System.Collections;
using UnityEngine;

public class SC_Gem : MonoBehaviour {
    [SerializeField] private InputManager _inputManager;
    [HideInInspector] public Vector2Int posIndex;

    public GlobalEnums.GemType type;
    public bool isMatch = false;
    public GameObject destroyEffect;
    public int scoreValue = 10;
    public int blastSize = 1;
    
    private Vector2Int previousPos;
    private SC_GameLogic scGameLogic;
    private SC_Gem otherGem;

    private void Update() {
        if (Vector2.Distance(transform.position, posIndex) > 0.01f)
            transform.position = Vector2.Lerp(transform.position, posIndex,
                SC_GameVariables.Instance.gemSpeed * Time.deltaTime);
        else {
            transform.position = new Vector3(posIndex.x, posIndex.y, 0);
            scGameLogic.SetGem(posIndex.x, posIndex.y, this);
        }
    }

    public void SetupGem(SC_GameLogic _ScGameLogic, Vector2Int _Position) {
        posIndex = _Position;
        scGameLogic = _ScGameLogic;
    }

    private void MovePieces(SwipeDirection direction) {
        previousPos = posIndex;

        if (direction is SwipeDirection.Right && posIndex.x < SC_GameVariables.Instance.rowsSize - 1) {
            otherGem = scGameLogic.GetGem(posIndex.x + 1, posIndex.y);
            otherGem.posIndex.x--;
            posIndex.x++;
        } else if (direction is SwipeDirection.Up && posIndex.y < SC_GameVariables.Instance.colsSize - 1) {
            otherGem = scGameLogic.GetGem(posIndex.x, posIndex.y + 1);
            otherGem.posIndex.y--;
            posIndex.y++;
        } else if (direction is SwipeDirection.Down && posIndex.y > 0) {
            otherGem = scGameLogic.GetGem(posIndex.x, posIndex.y - 1);
            otherGem.posIndex.y++;
            posIndex.y--;
        } else if (direction is SwipeDirection.Left && posIndex.x > 0) {
            otherGem = scGameLogic.GetGem(posIndex.x - 1, posIndex.y);
            otherGem.posIndex.x++;
            posIndex.x--;
        }

        scGameLogic.SetGem(posIndex.x, posIndex.y, this);
        scGameLogic.SetGem(otherGem.posIndex.x, otherGem.posIndex.y, otherGem);
        StartCoroutine(CheckMoveCo());
    }

    public IEnumerator CheckMoveCo() {
        scGameLogic.SetState(GlobalEnums.GameState.wait);

        yield return new WaitForSeconds(.5f);
        scGameLogic.FindAllMatches();

        if (otherGem != null) {
            if (isMatch == false && otherGem.isMatch == false) {
                otherGem.posIndex = posIndex;
                posIndex = previousPos;

                scGameLogic.SetGem(posIndex.x, posIndex.y, this);
                scGameLogic.SetGem(otherGem.posIndex.x, otherGem.posIndex.y, otherGem);

                yield return new WaitForSeconds(.5f);
                scGameLogic.SetState(GlobalEnums.GameState.move);
            } else {
                scGameLogic.DestroyMatches();
            }
        }
    }

    private void Start() {
        if (_inputManager == null) {
            _inputManager = FindObjectOfType<InputManager>();
        }

        if (_inputManager == null) {
            throw new NullReferenceException($"[{nameof(SC_Gem)}] Input Manager is missing!");
        }

        _inputManager.Swiped += OnSwiped;
    }

    private void OnSwiped(SwipeArgs args) {
        if (scGameLogic.CurrentState != GlobalEnums.GameState.move) {
            return;
        }

        var pos = new Vector2Int(Mathf.RoundToInt(args.startPos.x), Mathf.RoundToInt(args.startPos.y));
        if (pos != posIndex) {
            return;
        }

        MovePieces(args.swipeDirection);
    }

    private void OnDestroy() {
        if (_inputManager) {
            _inputManager.Swiped -= OnSwiped;
        }
    }
}