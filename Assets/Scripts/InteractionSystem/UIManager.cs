using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _interactionCrosshair;
    [SerializeField] private GameObject _interactionPanel;
    [SerializeField] private GameObject _feedbackPanel;
    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private GameObject _inventorySlotsContainer;
    [SerializeField] private GameObject _inventoryIconsContainer;
    [SerializeField] private int        _defaultCrosshairScale;
    [SerializeField] private int        _interactionCrosshairScale;
    [SerializeField] private Color      _unselectedSlotColor;
    [SerializeField] private Color      _selectedSlotColor;
    
    private TextMeshProUGUI _interactionMessage;
    private TextMeshProUGUI _feedbackMessage;
    private Image[]         _inventorySlots;
    private Image[]         _inventoryIcons;
    private int             _selectedInventorySlot;
    private float           _feedbackPanelDisplayTimer;

    public int  inventorySlotCount      => _inventorySlots.Length;
    public bool isInventoryPanelVisible => _inventoryPanel.activeSelf;

    public void Init()
    {
        _interactionMessage         = _interactionPanel.GetComponentInChildren<TextMeshProUGUI>();
        _feedbackMessage            = _feedbackPanel.GetComponentInChildren<TextMeshProUGUI>();
        _inventorySlots             = _inventorySlotsContainer.GetComponentsInChildren<Image>();
        _inventoryIcons             = _inventoryIconsContainer.GetComponentsInChildren<Image>();
        _selectedInventorySlot      = -1;
        _feedbackPanelDisplayTimer  = 0f;

        HideCursor();
        ShowDefaultCrosshair();
        HideInteractionPanel();
        HideFeedbackPanel();
        ResetInventoryIcons();

        for (int i = 0; i < _inventorySlots.Length; ++i)
            UnselectInventorySlot(i);
    }

    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ShowDefaultCrosshair()
    {
        _interactionCrosshair.transform.localScale = new Vector3(_defaultCrosshairScale, _defaultCrosshairScale, _defaultCrosshairScale);
    }
    
    public void ShowInteractionCrosshair()
    {
        _interactionCrosshair.transform.localScale = new Vector3(_interactionCrosshairScale, _interactionCrosshairScale, _interactionCrosshairScale);
    }

    public void ShowInteractionPanel(string message)
    {
        _interactionMessage.text = message;
        _interactionPanel.SetActive(true);
    }

    public void HideInteractionPanel()
    {
        _interactionPanel.SetActive(false);
    }

    public void ShowFeedbackPanel(string message, float displayTime)
    {
        _feedbackMessage.text = message;
        _feedbackPanel.SetActive(true);

        _feedbackPanelDisplayTimer = displayTime;
    }

    public void HideFeedbackPanel()
    {
        _feedbackPanel.SetActive(false);
    }

    public void ShowInventoryPanel()
    {
        _inventoryPanel.SetActive(true);
    }

    public void HideInventoryPanel()
    {
        _inventoryPanel.SetActive(false);
    }

    public void SetInventoryIcon(int slot, Sprite icon)
    {
        _inventoryIcons[slot].sprite   = icon;
        _inventoryIcons[slot].enabled  = true;
    }

    public void ResetInventoryIcons()
    {
        foreach (Image image in _inventoryIcons)
            image.enabled = false;
    }

    public void SelectInventorySlot(int slot)
    {
        if (_selectedInventorySlot != -1)
            UnselectInventorySlot(_selectedInventorySlot);

        _selectedInventorySlot = slot;

        if (_selectedInventorySlot != -1)
            _inventorySlots[_selectedInventorySlot].color = _selectedSlotColor;
    }

    public void UnselectInventorySlot(int slot)
    {
        _inventorySlots[slot].color = _unselectedSlotColor;
    }

    public void ToggleInventory()
    {
        if (_inventoryPanel.activeSelf)
            HideInventoryPanel();
        else
            ShowInventoryPanel();
    }

    void Update()
    {
        UpdateFeedbackPanelDisplayTimer();
    }

    private void UpdateFeedbackPanelDisplayTimer()
    {
        if (_feedbackPanelDisplayTimer >= 0)
        {
            _feedbackPanelDisplayTimer -= Time.deltaTime;

            if (_feedbackPanelDisplayTimer <= 0)
                HideFeedbackPanel();
        }
    }

}
