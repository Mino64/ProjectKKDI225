using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactive : MonoBehaviour
{
    [SerializeField] private InteractiveData _interactiveData;

    private InteractionManager  _interactionManager;
    private PlayerInventory     _inventory;
    private Collider            _collider;
    private Animator            _animator;
    private bool[]              _requirementStates;
    private bool                _isReady;
    private int                 _interactionCount;

    public InteractiveData interactiveData => _interactiveData;
    
    public bool isEnabled { get; private set; }

    public event Action<string> OnInteractionFeedback;

    public event Action OnPick;
    public event Action OnReady;
    public event Action OnAttempt;
    public event Action OnInteract;

    void Start()
    {
        _interactionManager = InteractionManager.instance;
        _inventory          = _interactionManager.inventory;
        _collider           = GetComponent<Collider>();
        _animator           = GetComponent<Animator>();
        _isReady            = interactiveData.requirements.Length == 0;
        _interactionCount   = 0;
        isEnabled           = interactiveData.isEnabled;

        if (_animator != null)
            _animator.keepAnimatorStateOnDisable = true;

        InitRequirementStates();

        if (isEnabled)
            Enable();
        else
            Disable();
    }

    private void InitRequirementStates()
    {
        if (!_isReady)
        {
            _requirementStates = new bool[interactiveData.requirements.Length];

            for (int i = 0; i < _requirementStates.Length; ++i)
                _requirementStates[i] = false;
        }
    }

    public void Enable()
    {
        isEnabled = true;
        _collider.enabled = true;
        _interactionManager.RegisterInteractive(this);
    }

    public void Disable()
    {
        isEnabled = false;
        _collider.enabled = false;
        _interactionManager.UnregisterInteractive(this);
    }

    public bool HasInteractionMessage()
    {
        return  (IsType(InteractiveData.Type.Pickable) && !_inventory.Contains(this)) ||
                (!_isReady && (InventoryHasRequirementSelected() || InventoryHasWrongRequirementSelected())) ||
                IsType(InteractiveData.Type.Direct) ||
                (IsType(InteractiveData.Type.Indirect) && !_isReady && interactiveData.interactionMessages.Length > 0);
    }

    public string GetInteractionMessage()
    {
        if (IsType(InteractiveData.Type.Pickable) && !_inventory.Contains(this))
            return _interactionManager.CreatePickMessage(interactiveData.inventoryName);
        else if (!_isReady && (InventoryHasRequirementSelected() || InventoryHasWrongRequirementSelected()))
            return _interactionManager.CreateInteractionMessage(_inventory.GetSelectedItem().interactiveData.interactionMessages[0]);
        else if (IsType(InteractiveData.Type.Direct) || (IsType(InteractiveData.Type.Indirect) && !_isReady && interactiveData.interactionMessages.Length > 0))
            return _interactionManager.CreateInteractionMessage(interactiveData.interactionMessages[_interactionCount % interactiveData.interactionMessages.Length]);
        else
            return null;
    }

    private bool IsType(InteractiveData.Type type)
    {
        return interactiveData.type == type;
    }

    private bool InventoryHasRequirementSelected()
    {
        foreach (InteractiveData requirementData in interactiveData.requirements)
        {
            Interactive selected = _inventory.GetSelectedItem();

            if (selected != null && selected.interactiveData == requirementData)
                return true;
        }

        return false;
    }

    private bool InventoryHasWrongRequirementSelected()
    {
        foreach (InteractiveData decoyRequirementData in interactiveData.wrongRequirements)
        {
            Interactive selected = _inventory.GetSelectedItem();

            if (selected != null && selected.interactiveData == decoyRequirementData)
                return true;
        }

        return false;
    }

    public void Interact(bool direct)
    {
        if (IsType(InteractiveData.Type.Pickable))
            DoPickableInteraction();
        else if (IsType(InteractiveData.Type.Direct))
            DoDirectInteraction();
        else
            DoIndirectInteraction(direct);
    }

    private void DoPickableInteraction()
    {
        if (_inventory.IsFull())
            _interactionManager.NotifyInventoryFull();
        else
            AddToInventory();
    }

    private void AddToInventory()
    {
        TriggerAnimation(_interactionManager.pickAnimationTrigger, OnPick, true);

        _inventory.Add(this);

        if (interactiveData.pickFeedback.Length > 0)
            OnInteractionFeedback?.Invoke(interactiveData.pickFeedback);
    }

    private void TriggerAnimation(string trigger, Action eventToDispatch, bool disableObject = false, bool disableCollider = false)
    {
        if (_animator != null && AnimationExists(trigger))
        {
            gameObject.SetActive(true);
            _collider.enabled = false;

            _animator.SetTrigger(trigger);
            StartCoroutine(PostAnimationProcessing(eventToDispatch, disableObject, disableCollider));
        }
        else
            DoPostAnimationActions(eventToDispatch, disableObject, disableCollider);
    }

    private bool AnimationExists(string animation)
    {
        foreach (var parameter in _animator.parameters)
        {
            if (parameter.name == animation)
                return true;
        }

        return false;
    }

    private IEnumerator PostAnimationProcessing(Action eventToDispatch, bool disableObject, bool disableCollider)
    {
        AnimatorStateInfo prevInfo = _animator.GetCurrentAnimatorStateInfo(0);

        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo curInfo = _animator.GetCurrentAnimatorStateInfo(0);

            return curInfo.shortNameHash != prevInfo.shortNameHash || curInfo.normalizedTime < prevInfo.normalizedTime;
        });

        prevInfo = _animator.GetCurrentAnimatorStateInfo(0);

        while (true)
        {
            AnimatorStateInfo curInfo = _animator.GetCurrentAnimatorStateInfo(0);

            if (curInfo.shortNameHash != prevInfo.shortNameHash || curInfo.normalizedTime >= 1f)
                break;

            yield return null;
        }

        DoPostAnimationActions(eventToDispatch, disableObject, disableCollider);
    }

    private void DoPostAnimationActions(Action eventToDispatch, bool disableObject, bool disableCollider)
    {
        if (disableObject)
            gameObject.SetActive(false);
        else if (!disableCollider)
            _collider.enabled = true;

        eventToDispatch?.Invoke();
    }    

    private void DoDirectInteraction()
    {
        if (!_isReady)
        {
            Debug.Log("Fail");
            if (InventoryHasRequirementSelected())
                UseRequirementFromInventory();
            else if (InventoryHasWrongRequirementSelected())
                UseWrongRequirementFromInventory();
            else
                AttemptInteraction();
        }
        else
        {
            Debug.Log("Success");
            ++_interactionCount;

            if (interactiveData.singleInteraction)
                Disable();

            DoInteractionSteps();
        }
    }

    private void UseRequirementFromInventory()
    {
        Interactive requirement = _inventory.GetSelectedItem();

        if (!requirement.interactiveData.keepAfterUse)
            _inventory.Remove(requirement);

        requirement.transform.SetParent(transform);
        requirement.TriggerAnimation(_interactionManager.interactAnimationTrigger, requirement.OnInteract, requirement.interactiveData.keepAfterUse, requirement.interactiveData.singleInteraction);
        
        if (requirement.interactiveData.interactionFeedback.Length > 0)
            OnInteractionFeedback?.Invoke(requirement.interactiveData.interactionFeedback);

        _requirementStates[GetUnmetRequirementIndex(requirement.interactiveData)] = true;

        CheckIfReady();
    }

    private void UseWrongRequirementFromInventory()
    {
        Interactive wrongRequirement = _inventory.GetSelectedItem();

        wrongRequirement.transform.SetParent(transform);
        wrongRequirement.TriggerAnimation(_interactionManager.interactAnimationTrigger, wrongRequirement.OnInteract, true);

        if (wrongRequirement.interactiveData.wrongTargetFeedback.Length > 0)
            OnInteractionFeedback?.Invoke(wrongRequirement.interactiveData.wrongTargetFeedback);
    }

    private int GetUnmetRequirementIndex(InteractiveData requirementData)
    {
        for (int i = 0; i < interactiveData.requirements.Length; ++i)
        {
            if (interactiveData.requirements[i] == requirementData && !_requirementStates[i])
                return i;
        }

        return -1;
    }

    private void CheckIfReady()
    {
        _isReady = true;

        for (int i = 0; i < _requirementStates.Length; ++i)
        {
            if (!_requirementStates[i])
                _isReady = false;
        }

        if (_isReady)
        {
            if (IsType(InteractiveData.Type.Direct))
            {
                if (interactiveData.autoInteract)
                    DoInteractionSteps();
                else
                    TriggerAnimation(_interactionManager.readyAnimationTrigger, OnReady);
            }
            else if (IsType(InteractiveData.Type.Indirect))
                DoInteractionSteps();
        }
    }

    private void DoInteractionSteps()
    {
        if (interactiveData.singleInteraction)
            Disable();

        TriggerAnimation(_interactionManager.interactAnimationTrigger, OnInteract, false, interactiveData.singleInteraction);

        if (interactiveData.interactionFeedback.Length > 0)
            OnInteractionFeedback?.Invoke(interactiveData.interactionFeedback);

        PropagateInteraction();
    }

    private void PropagateInteraction()
    {
        List<Interactive> dependents = _interactionManager.FindDependents(interactiveData);

        foreach (Interactive dependent in dependents)
        {
            int unmetRequirementIndex = dependent.GetUnmetRequirementIndex(interactiveData);

            if (unmetRequirementIndex != -1)
                dependent._requirementStates[unmetRequirementIndex] = true;

            dependent.CheckIfReady();
        }
    }

    private void AttemptInteraction()
    {
        TriggerAnimation(_interactionManager.attemptAnimationTrigger, OnAttempt);

        OnInteractionFeedback?.Invoke(interactiveData.requirementsFeedback);
    }

    private void DoIndirectInteraction(bool direct)
    {
        if (direct)
            AttemptInteraction();
        else
            DoInteractionSteps();
    }

}