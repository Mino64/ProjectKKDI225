using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class RotatingPiece : MonoBehaviour
{
    [SerializeField] private Vector3    _localAxis = Vector3.up;
    [SerializeField] private float      _stepAngle = 45f;
    [SerializeField] private float      _rotateDuration = 0.25f;
    [SerializeField] private int        _solutionStep;

    private Quaternion  _baseRotation;
    private Quaternion  _targetRotation;
    private int         _stepsPerTurn;
    private int         _step;
    private int         _initialStep;
    private int         _direction = 1;

    public bool IsAvailable { get; private set; } = true;
    public bool IsRotating  { get; private set; }
    public bool IsCorrect   => IsAvailable && _step == _solutionStep;

    public event Action OnRotated;

    void Awake()
    {
        _stepsPerTurn   = Mathf.RoundToInt(360f / _stepAngle);
        _solutionStep   = WrapStep(_solutionStep);
        _baseRotation   = transform.localRotation;

        ApplyStep(0);
    }

    public void Hide()
    {
        IsAvailable = false;

        gameObject.SetActive(false);
    }

    public void Reveal()
    {
        gameObject.SetActive(true);

        Shuffle();

        IsAvailable = true;
    }

    public void SetDirection(bool clockwise)
    {
        _direction = clockwise ? 1 : -1;
    }

    public void Shuffle()
    {
        do
            _initialStep = Random.Range(0, _stepsPerTurn);
        while (_initialStep == _solutionStep);

        ApplyStep(_initialStep);
    }

    public void Rotate()
    {
        if (!IsAvailable || IsRotating)
            return;

        _step = WrapStep(_step + _direction);
        _targetRotation *= Quaternion.AngleAxis(_stepAngle * _direction, _localAxis);

        StartCoroutine(RotateRoutine());
    }

    public void ResetRotation()
    {
        if (!IsAvailable)
            return;

        StopAllCoroutines();
        IsRotating = false;

        ApplyStep(_initialStep);
    }

    private void ApplyStep(int step)
    {
        _step                   = step;
        _targetRotation         = _baseRotation * Quaternion.AngleAxis(step * _stepAngle, _localAxis);
        transform.localRotation = _targetRotation;
    }

    private int WrapStep(int step)
    {
        return ((step % _stepsPerTurn) + _stepsPerTurn) % _stepsPerTurn;
    }

    private IEnumerator RotateRoutine()
    {
        IsRotating = true;

        Quaternion start = transform.localRotation;

        for (float t = 0f; t < 1f; t += Time.deltaTime / _rotateDuration)
        {
            transform.localRotation = Quaternion.Slerp(start, _targetRotation, Mathf.SmoothStep(0f, 1f, t));

            yield return null;
        }

        transform.localRotation = _targetRotation;
        IsRotating = false;

        OnRotated?.Invoke();
    }

}