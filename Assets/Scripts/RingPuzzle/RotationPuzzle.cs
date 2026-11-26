using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class RotationPuzzle : MonoBehaviour
{
    [SerializeField] private PuzzleFocus            _focus;
    [SerializeField] private RotatingPiece[]        _pieces;
    [SerializeField] private InputActionReference   _clickAction;
    [SerializeField] private InputActionReference   _pointAction;
    [SerializeField] private LayerMask              _clickMask = ~0;
    [SerializeField] private float                  _maxClickDistance = 10f;
    [Space(15)]
    [SerializeField] private bool                   _alternateDirections;
    [SerializeField] private bool                   _shuffleOnStart;
    [SerializeField] private bool                   _resetOnFocusExit;

    public bool IsSolved { get; private set; }

    public event Action OnSolved;

    void Start()
    {
        for (int i = 0; i < _pieces.Length; ++i)
        {
            _pieces[i].SetDirection(!_alternateDirections || i % 2 == 0);
            _pieces[i].OnRotated += HandlePieceRotated;
        }

        if (_resetOnFocusExit)
            _focus.OnFocusExited += ResetPieces;

        if (_shuffleOnStart)
        {
            foreach (RotatingPiece piece in _pieces)
                piece.Shuffle();
        }
    }

    void OnDestroy()
    {
        foreach (RotatingPiece piece in _pieces)
        {
            if (piece != null)
                piece.OnRotated -= HandlePieceRotated;
        }

        if (_focus != null)
            _focus.OnFocusExited -= ResetPieces;
    }

    void Update()
    {
        if (CanRotate() && _clickAction.action.WasPressedThisFrame())
            TryRotateClickedPiece();
    }

    public void ResetPieces()
    {
        if (IsSolved)
            return;

        foreach (RotatingPiece piece in _pieces)
            piece.ResetRotation();
    }

    private bool CanRotate()
    {
        return _focus.IsFocused && !IsSolved && !IsAnyPieceRotating();
    }

    private void TryRotateClickedPiece()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = _focus.Cam.ScreenPointToRay(_pointAction.action.ReadValue<Vector2>());

        if (Physics.Raycast(ray, out RaycastHit hit, _maxClickDistance, _clickMask, QueryTriggerInteraction.Ignore))
        {
            PuzzleClickTarget target = hit.collider.GetComponent<PuzzleClickTarget>();

            if (target != null)
                target.Piece.Rotate();
        }
    }

    private bool IsAnyPieceRotating()
    {
        foreach (RotatingPiece piece in _pieces)
        {
            if (piece.IsRotating)
                return true;
        }

        return false;
    }

    private bool IsSolutionReached()
    {
        foreach (RotatingPiece piece in _pieces)
        {
            if (!piece.IsCorrect)
                return false;
        }

        return true;
    }

    private void HandlePieceRotated()
    {
        if (IsSolved || !IsSolutionReached())
            return;

        IsSolved = true;

        OnSolved?.Invoke();
    }

}