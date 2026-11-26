using UnityEngine;

public class PuzzleClickTarget : MonoBehaviour
{
    [SerializeField] private RotatingPiece _piece;

    public RotatingPiece Piece => _piece;

}