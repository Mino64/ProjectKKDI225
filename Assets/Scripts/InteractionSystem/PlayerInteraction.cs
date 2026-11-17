using UnityEngine;
using System;

public class PlayerInteraction : MonoBehaviour
{
    private Transform   _cameraTransform;
    private float       _maxInteractionDistance;
    private Interactive _focusedInteractive;
    private bool        _refreshFocusedInteractive;
    private bool        _interact;
    
    public event Action<Interactive> OnFocusedInteractiveChanged;

    public void Init(float maxInteractionDistance)
    {
        _cameraTransform            = GetComponentInChildren<Camera>().transform;
        _maxInteractionDistance     = maxInteractionDistance;
        _focusedInteractive         = null;
        _refreshFocusedInteractive  = false;
        _interact                   = false;
    }

    public void SetInteractionInput(bool interact)
    {
        _interact = interact;
    }

    void Update()
    {
        UpdateCurrentInteractive();
        CheckForPlayerInteraction();
    }

    private void UpdateCurrentInteractive()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hitInfo, _maxInteractionDistance))
            CheckObjectForInteraction(hitInfo.collider);
        else if (_focusedInteractive != null)
            ClearFocusedInteractive();
    }

    private void CheckObjectForInteraction(Collider collider)
    {
        Interactive interactive = collider.GetComponent<Interactive>();

        if (interactive == null || !interactive.isEnabled || !interactive.HasInteractionMessage())
        {
            if (_focusedInteractive != null)
                ClearFocusedInteractive();
        }
        else if (interactive != _focusedInteractive || _refreshFocusedInteractive)
            SetFocusedInteractive(interactive);
    }

    private void ClearFocusedInteractive()
    {
        _focusedInteractive = null;

        OnFocusedInteractiveChanged?.Invoke(null);
    }

    private void SetFocusedInteractive(Interactive interactive)
    {
        _focusedInteractive         = interactive;
        _refreshFocusedInteractive  = false;

        OnFocusedInteractiveChanged?.Invoke(_focusedInteractive);
    }

    private void CheckForPlayerInteraction()
    {
        if (_interact && _focusedInteractive != null)
        {
            _focusedInteractive.Interact(true);
            _refreshFocusedInteractive = true;
        }
    }

    public void RefreshFocusedInteractive()
    {
        _refreshFocusedInteractive = true;
    }
}
