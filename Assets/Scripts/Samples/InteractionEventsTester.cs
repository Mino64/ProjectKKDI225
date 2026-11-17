using UnityEngine;

public class InteractionEventsTester : MonoBehaviour
{
    [SerializeField] private Interactive _interactiveToTest;

    void Start()
    {
        if (_interactiveToTest != null)
        {
            _interactiveToTest.OnPick       += OnPickTest;
            _interactiveToTest.OnReady      += OnReadyTest;
            _interactiveToTest.OnAttempt    += OnAttemptTest;
            _interactiveToTest.OnInteract   += OnInteractTest;
        }
    }

    private void OnPickTest()
    {
        print("Pick event.");
    }

    private void OnReadyTest()
    {
        print("Ready event.");
    }

    private void OnAttemptTest()
    {
        print("Attempt event.");
    }

    private void OnInteractTest()
    {
        print("Interact event.");
    }

}
