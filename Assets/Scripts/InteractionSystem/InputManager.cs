using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [field: SerializeField] public string lookActionName                { get; private set; }
    [field: SerializeField] public string moveActionName                { get; private set; }
    [field: SerializeField] public string jumpActionName                { get; private set; }
    [field: SerializeField] public string interactActionName            { get; private set; }
    [field: SerializeField] public string inventoryActionName           { get; private set; }
    [field: SerializeField] public string selectSlotActionNamePrefix    { get; private set; }

    private PlayerMovement      _movement;
    private PlayerInteraction   _interaction;
    private PlayerInventory     _inventory;
    private UIManager           _uiManager;
    private InputAction         _lookAction;
    private InputAction         _moveAction;
    private InputAction         _jumpAction;
    private InputAction         _interactAction;
    private InputAction         _toggleInventoryAction;
    private InputAction[]       _inventorySlotSelectActions;

    public string interactActionBinding => _interactAction.GetBindingDisplayString();

    public void Init(PlayerMovement movement, PlayerInteraction interaction, PlayerInventory inventory, UIManager uiManager)
    {
        _movement       = movement;
        _interaction    = interaction;
        _inventory      = inventory;
        _uiManager      = uiManager;

        _inventorySlotSelectActions = new InputAction[_uiManager.inventorySlotCount];

        _lookAction             = InputSystem.actions.FindAction(lookActionName);
        _moveAction             = InputSystem.actions.FindAction(moveActionName);
        _jumpAction             = InputSystem.actions.FindAction(jumpActionName);        
        _interactAction         = InputSystem.actions.FindAction(interactActionName);
        _toggleInventoryAction  = InputSystem.actions.FindAction(inventoryActionName);

        for (int i = 0; i < _uiManager.inventorySlotCount; ++i)
            _inventorySlotSelectActions[i] = InputSystem.actions.FindAction(selectSlotActionNamePrefix + (i + 1));
    }

    void Update()
    {
        CheckForLookInput();
        CheckForMoveInput();
        CheckForJumpInput();
        CheckForInteractionInput();
        CheckForInventoryToggle();
        CheckForInventorySlotSelection();
    }

    private void CheckForLookInput()
    {
        _movement.SetLookInput(_lookAction.ReadValue<Vector2>());
    }

    private void CheckForMoveInput()
    {
        _movement.SetMoveInput(_moveAction.ReadValue<Vector2>());
    }

    private void CheckForInteractionInput()
    {
        _interaction.SetInteractionInput(_interactAction.WasPressedThisFrame());
    }

    private void CheckForJumpInput()
    {
        _movement.SetJumpInput(_jumpAction.WasPressedThisFrame());
    }

    private void CheckForInventoryToggle()
    {
        if (_toggleInventoryAction.WasPressedThisFrame())
            _uiManager.ToggleInventory();
    }

    private void CheckForInventorySlotSelection()
    {
        for (int i = 0; i < _uiManager.inventorySlotCount; ++i)
        {
            if (_inventorySlotSelectActions[i].WasPressedThisFrame() && i < _inventory.count && i != _inventory.selectedSlot)
            {
                _uiManager.UnselectInventorySlot(_inventory.selectedSlot);
                _inventory.SelectSlot(i);

                return;
            }
        }
    }    

}
