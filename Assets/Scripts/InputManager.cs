using System;
using UnityEngine;
using Zenject;

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
    [SerializeField] private float _minSwipeDistance = 0.5f;

    public event Action<SwipeArgs> Swiped;

    private Vector3 _mouseDownPos;
    private bool _isDragging;

    [Inject]
    public void Bind(Camera c) {
        _camera = c;
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
        if (delta.magnitude < _minSwipeDistance) {
            return;
        }

        SwipeDirection direction;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) {
            direction = delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
        } else {
            direction = delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
        }

        Swiped?.Invoke(new SwipeArgs(direction, startPos, endPos));
    }
}