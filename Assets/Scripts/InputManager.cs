using System;
using UnityEngine;

public enum SwipeDirection {
    Left,
    Right,
    Up,
    Down,
    None
}

public readonly struct SwipeArgs {
    public readonly SwipeDirection swipeDirection;
    public readonly Vector2 startPos;
    public readonly Vector2 endPos;

    public SwipeArgs(SwipeDirection swipeDirection, Vector2 startPos, Vector2 endPos) {
        this.swipeDirection = swipeDirection;
        this.startPos = startPos;
        this.endPos = endPos;
    }
}

public class InputManager : MonoBehaviour {
    [SerializeField] private Camera _camera;
    [SerializeField] private SC_GameVariables _gameVariables;

    public event Action<SwipeArgs> Swiped;

    private Vector3 _mouseDownPos;
    private bool _isDragging;

    private void Awake() {
        if (_camera == null) {
            _camera = Camera.main;
        }

        if (_camera == null) {
            throw new NullReferenceException($"[{nameof(InputManager)}]Camera missing");
        }

        if (_gameVariables == null) {
            _gameVariables = FindObjectOfType<SC_GameVariables>();
        }

        if (_gameVariables == null) {
            throw new NullReferenceException($"[{nameof(InputManager)}]GameVariables missing");
        }
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            _mouseDownPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            _isDragging = true;
        }

        if (_isDragging && Input.GetMouseButtonUp(0)) {
            _isDragging = false;
            var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            DetectAndFireSwipe(_mouseDownPos, mousePos);
        }
    }

    private void DetectAndFireSwipe(Vector2 startPos, Vector2 endPos) {
        var delta = endPos - startPos;
        //could use sqr magnitude but this will not happen each frame so performance impact is negligible
        if (delta.magnitude < _gameVariables.minSwipeDistance) {
            return;
        }

        var direction = SwipeDirection.None;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) {
            direction = delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
        } else {
            direction = delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
        }

        Swiped?.Invoke(new SwipeArgs(direction, startPos, endPos));
    }
}