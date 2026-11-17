using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    private static InteractionManager _instance;

    public static InteractionManager instance
    {
        get
        {
            if (_instance == null)
                FindFirstObjectByType<InteractionManager>().Init();

            return _instance;
        }
    }

    [SerializeField] private float  _maxInteractionDistance;
    [SerializeField] private float  _feedbackDisplayTime;
    [Space(15)]
    [SerializeField] private string _interactMessagePrefix;
    [SerializeField] private string _pickMessagePrefix;
    [SerializeField] private string _inventoryFullFeedback;
    [Space(15)]
    [SerializeField] private bool   _hideInventoryWhenEmpty;
    [SerializeField] private bool   _showInventoryOnAdd;
    [SerializeField] private bool   _showInventoryOnRemove;
    [SerializeField] private bool   _showInventoryOnSelect;
    [field: Space(15)]
    [field: SerializeField] public string attemptAnimationTrigger   { get; private set; }
    [field: SerializeField] public string readyAnimationTrigger     { get; private set; }
    [field: SerializeField] public string pickAnimationTrigger      { get; private set; }
    [field: SerializeField] public string interactAnimationTrigger  { get; private set; }

    private UIManager           _uiManager;
    private PlayerInteraction   _playerInteraction;
    private List<Interactive>   _interactives;

    public PlayerInventory inventory { get; private set; }

    void Awake()
    {
        if (_instance == null)
            Init();
        else if (_instance != this)
            Destroy(gameObject);
    }
    
    private void Init()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitUI();
        InitInventory();
        InitInteraction();
        InitInputManager();
    }

    private void InitUI()
    {
        _uiManager = FindAnyObjectByType<UIManager>();
        _uiManager.Init();

        if (_hideInventoryWhenEmpty)
            _uiManager.HideInventoryPanel();
        else
            _uiManager.ShowInventoryPanel();
    }

    private void InitInventory()
    {
        inventory = FindAnyObjectByType<PlayerInventory>();
        inventory.Init(_uiManager.inventorySlotCount);

        inventory.OnAdded       += ProcessInventoryAdd;
        inventory.OnRemoved     += ProcessInventoryRemove;
        inventory.OnSelected    += ProcessInventorySelect;
    }

    private void ProcessInventoryAdd(int slot, Interactive item)
    {
        _uiManager.SetInventoryIcon(slot, item.interactiveData.inventoryIcon);

        if (_showInventoryOnAdd)
            _uiManager.ShowInventoryPanel();
    }

    private void ProcessInventoryRemove()
    {
        _uiManager.ResetInventoryIcons();

        for (int i = 0; i < inventory.count; ++i)
            _uiManager.SetInventoryIcon(i, inventory.GetItem(i).interactiveData.inventoryIcon);
        
        if (_showInventoryOnRemove)
            _uiManager.ShowInventoryPanel();
        else if (inventory.count == 0 && _hideInventoryWhenEmpty)
            _uiManager.HideInventoryPanel();
    }

    private void ProcessInventorySelect(int slot, bool forced)
    {
        _uiManager.SelectInventorySlot(slot);

        if (!forced && _showInventoryOnSelect)
            _uiManager.ShowInventoryPanel();

        _playerInteraction.RefreshFocusedInteractive();
    }

    private void InitInteraction()
    {
        _interactives = new List<Interactive>();

        _playerInteraction = FindAnyObjectByType<PlayerInteraction>();
        _playerInteraction.Init(_maxInteractionDistance);

        _playerInteraction.OnFocusedInteractiveChanged += ProcessFocusedInteractiveChanged;
    }

    private void ProcessFocusedInteractiveChanged(Interactive interactive)
    {
        if (interactive != null)
        {
            _uiManager.ShowInteractionCrosshair();
            _uiManager.ShowInteractionPanel(interactive.GetInteractionMessage());
        }
        else
        {
            _uiManager.ShowDefaultCrosshair();
            _uiManager.HideInteractionPanel();                
        }
    }

    private void InitInputManager()
    {
        PlayerMovement playerMovement = FindAnyObjectByType<PlayerMovement>();

        FindAnyObjectByType<InputManager>().Init(playerMovement, _playerInteraction, inventory, _uiManager);
    }

    public void RegisterInteractive(Interactive interactive)
    {
        _interactives.Add(interactive);
        interactive.OnInteractionFeedback += ProcessInteractionFeedback;
    }

    private void ProcessInteractionFeedback(string feedback)
    {
        _uiManager.ShowFeedbackPanel(feedback, _feedbackDisplayTime);
    }

    public void UnregisterInteractive(Interactive interactive)
    {
        _interactives.Remove(interactive);
        interactive.OnInteractionFeedback -= ProcessInteractionFeedback;
    }

    public string CreatePickMessage(string objectName)
    {
        return _interactMessagePrefix + " " + _pickMessagePrefix + " " + objectName;
    }

    public string CreateInteractionMessage(string message)
    {
        return _interactMessagePrefix + " " + message;
    }

    public void NotifyInventoryFull()
    {
        _uiManager.ShowFeedbackPanel(_inventoryFullFeedback, _feedbackDisplayTime);   
    }

    public List<Interactive> FindDependents(InteractiveData interactiveData)
    {
        List<Interactive> dependents = new List<Interactive>();

        foreach (Interactive interactive in _interactives)
        {
            foreach (InteractiveData requirementInteractiveData in interactive.interactiveData.requirements)
            {
                if (requirementInteractiveData == interactiveData)
                {
                    dependents.Add(interactive);
                    break;
                }
            }
        }

        return dependents;      
    }

}
