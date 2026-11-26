using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PuzzleFocus : MonoBehaviour
{
    private enum State { Idle, Entering, Focused, Exiting }

    [SerializeField] private Interactive            _trigger;
    [SerializeField] private Transform              _focusPoint;
    [SerializeField] private Button                 _exitButton;
    [SerializeField] private PlayerMovement         _movement;
    [SerializeField] private Camera                 _camera;
    [SerializeField] private InputActionReference   _exitAction;
    [SerializeField] private float                  _transitionDuration = 0.6f;

    private Transform   _camTransform;
    private Vector3     _savedLocalPosition;
    private Quaternion  _savedLocalRotation;
    private State       _state;

    public Camera Cam       => _camera;
    public bool   IsFocused => _state == State.Focused;

    public event Action OnFocusEntered;
    public event Action OnFocusExited;

    void Start()
    {
        _camTransform   = _camera.transform;
        _state          = State.Idle;

        _exitButton.gameObject.SetActive(false);
        _trigger.OnInteract += Enter;
    }

    void OnDestroy()
    {
        if (_trigger != null)
            _trigger.OnInteract -= Enter;
    }

    void Update()
    {
        if (_state == State.Focused && _exitAction.action.WasPressedThisFrame())
            Exit();
    }

    private void Enter()
    {
        if (_state == State.Idle)
            StartCoroutine(EnterRoutine());
    }

    public void Exit()
    {
        if (_state == State.Focused)
            StartCoroutine(ExitRoutine());
    }

    private IEnumerator EnterRoutine()
    {
        _state = State.Entering;

        _movement.enabled = false;
        _trigger.Disable();

        _savedLocalPosition = _camTransform.localPosition;
        _savedLocalRotation = _camTransform.localRotation;

        yield return MoveCamera(_focusPoint.position, _focusPoint.rotation);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        _exitButton.onClick.AddListener(Exit);
        _exitButton.gameObject.SetActive(true);

        _state = State.Focused;
        OnFocusEntered?.Invoke();
    }

    private IEnumerator ExitRoutine()
    {
        _state = State.Exiting;

        _exitButton.onClick.RemoveListener(Exit);
        _exitButton.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;

        OnFocusExited?.Invoke();

        Transform   parent      = _camTransform.parent;
        Vector3     targetPos   = parent.TransformPoint(_savedLocalPosition);
        Quaternion  targetRot   = parent.rotation * _savedLocalRotation;

        yield return MoveCamera(targetPos, targetRot);

        _movement.enabled = true;
        _trigger.Enable();

        _state = State.Idle;
    }

    private IEnumerator MoveCamera(Vector3 toPos, Quaternion toRot)
    {
        Vector3     fromPos = _camTransform.position;
        Quaternion  fromRot = _camTransform.rotation;

        for (float t = 0f; t < 1f; t += Time.deltaTime / _transitionDuration)
        {
            float s = Mathf.SmoothStep(0f, 1f, t);

            _camTransform.SetPositionAndRotation(Vector3.Lerp(fromPos, toPos, s), Quaternion.Slerp(fromRot, toRot, s));

            yield return null;
        }

        _camTransform.SetPositionAndRotation(toPos, toRot);
    }

}