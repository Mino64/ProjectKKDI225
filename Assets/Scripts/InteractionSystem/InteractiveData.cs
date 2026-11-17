using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "InteractiveData", menuName = "Scriptable Objects/InteractiveData")]
public class InteractiveData : ScriptableObject
{
    public enum Type { Pickable, Direct, Indirect };

    public bool isEnabled = true;
    
    [Space(15)]
    
    public Type type;

    [ShowIf(nameof(type), Type.Pickable)]
    public string inventoryName;

    [ShowIf(nameof(type), Type.Pickable)]
    public Sprite inventoryIcon;

    [ShowIf(nameof(type), Type.Pickable)]
    public string pickFeedback;

    [ShowIf(nameof(type), Type.Pickable)]
    public string wrongTargetFeedback;

    [ShowIf(nameof(CanBePickAfterUse))]
    public bool keepAfterUse;

    [ShowIf(nameof(CanBeSingleInteraction))]
    public bool singleInteraction;

    [ShowIf(nameof(type), Type.Direct)]
    public bool autoInteract;

    [Space(15)]

    [HideIf(nameof(type), Type.Pickable)]
    public InteractiveData[] requirements;

    [HideIf(nameof(type), Type.Pickable)]
    public InteractiveData[] wrongRequirements;

    [Space(15)]

    public string[] interactionMessages;

    [ShowIf(nameof(HasRequirements))]
    public string requirementsFeedback;

    [HideIf(nameof(type), Type.Indirect)]
    public string interactionFeedback;

    private bool CanBePickAfterUse => type == Type.Pickable && !singleInteraction;

    //private bool CanBeSingleInteraction => (type == Type.Pickable && !keepAfterUse) || (type == Type.Direct && !autoInteract);
    private bool CanBeSingleInteraction => (type == Type.Pickable && !keepAfterUse) || type == Type.Direct;

    private bool HasRequirements => requirements != null && requirements.Length != 0;
}
