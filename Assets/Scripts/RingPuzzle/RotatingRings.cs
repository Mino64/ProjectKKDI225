using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class RotatingRings : MonoBehaviour
{
    [SerializeField] private float stepAngle = 45f;
    [SerializeField] private float rotateDuration = 0.25f;
    [SerializeField] private Vector3 localAxis = Vector3.up;
    [SerializeField] private bool clockwise = true;
    [SerializeField] private Camera cam;
    private Collider ringCollider;
    private Quaternion targetRotation;
    private bool isRotating;

    private void Awake()
    {

        ringCollider = GetComponent<Collider>();
        targetRotation = transform.localRotation;
    }

    private void Update()
    {
        if (isRotating) return;
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (ringCollider.Raycast(ray, out _, 100f))
            Rotate();
    }

    private void Rotate()
    {
        float angle = clockwise ? stepAngle : -stepAngle;
        targetRotation *= Quaternion.AngleAxis(angle, localAxis);
        StartCoroutine(RotateRoutine());
    }

    private IEnumerator RotateRoutine()
    {
        isRotating = true;
        Quaternion start = transform.localRotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / rotateDuration;
            transform.localRotation = Quaternion.Slerp(start, targetRotation, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.localRotation = targetRotation;
        isRotating = false;
    }
}
