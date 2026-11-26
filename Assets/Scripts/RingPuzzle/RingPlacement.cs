using System;
using UnityEngine;

[RequireComponent(typeof(Interactive))]
public class RingPlacement : MonoBehaviour
{
    [SerializeField] private Interactive    _pod;
    [SerializeField] private RotatingPiece  _revealedPiece;

    private Interactive _interactive;

    public bool IsPlaced { get; private set; }

    public event Action OnPlaced;

    void Awake()
    {
        _interactive = GetComponent<Interactive>();
    }

    void Start()
    {
        _revealedPiece.Hide();

        _interactive.OnInteract += HandleInteract;
    }

    void OnDestroy()
    {
        _interactive.OnInteract -= HandleInteract;
    }

    private void HandleInteract()
    {
        if (IsPlaced || transform.parent != _pod.transform)
            return;

        IsPlaced = true;

        _interactive.Disable();
        gameObject.SetActive(false);

        _revealedPiece.Reveal();

        OnPlaced?.Invoke();
    }

}