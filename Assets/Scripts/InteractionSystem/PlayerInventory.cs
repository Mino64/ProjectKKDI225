using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int                 _maxItems;
    private List<Interactive>   _inventory;

    public int count            => _inventory.Count;
    public int selectedSlot     { private set; get; }

    public event Action<int, Interactive>   OnAdded;
    public event Action                     OnRemoved;
    public event Action<int, bool>          OnSelected;

    public void Init(int maxItems)
    {
        _maxItems       = maxItems;
        _inventory      = new List<Interactive>();
        selectedSlot    = -1;
    }

    public void Add(Interactive item)
    {
        _inventory.Add(item);

        if (_inventory.Count == 1)
            SetSelectedSlot(0, true);

        OnAdded?.Invoke(_inventory.Count - 1, item);
    }

    private void SetSelectedSlot(int slot, bool forced)
    {
        selectedSlot = slot;

        OnSelected?.Invoke(slot, forced);
    }

    public void Remove(Interactive item)
    {
        _inventory.Remove(item);

        if (selectedSlot == _inventory.Count)
            SetSelectedSlot(selectedSlot - 1, true);

        OnRemoved?.Invoke();
    }

    public Interactive GetItem(int slot)
    {
        return _inventory[slot];
    }

    public bool Contains(Interactive item)
    {
        return _inventory.Contains(item);
    }

    public bool IsFull()
    {
        return _inventory.Count == _maxItems;
    }

    public void SelectSlot(int slot)
    {
        SetSelectedSlot(slot, false);
    }

    public Interactive GetSelectedItem()
    {
        return selectedSlot >= 0 && selectedSlot < _inventory.Count ? _inventory[selectedSlot] : null;
    }

}
