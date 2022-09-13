using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    public ItemSlotUI[] uiSlots;
    public ItemSlot[] slots;

    public GameObject inventoryWindow;
    public Transform dropPosition;

    [Header("Selected Item")]
    private ItemSlot _selectedItem;
    private int _selectedItemIndex;
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDescription;
    public TextMeshProUGUI selectedItemStatNames;
    public TextMeshProUGUI selectedItemStatValues;
    public GameObject useButton;
    public GameObject equipButton;
    public GameObject unEquipButton;
    public GameObject dropButton;

    private int _curEquipIndex;

    private PlayerController _controller;
    private PlayerNeeds _needs;

    [Header("Events")]
    public UnityEvent onOpenInventory;
    public UnityEvent onCloseInventory;

    public static Inventory instance;

    private void Awake ()
    {
        instance = this;
        _controller = GetComponent<PlayerController>();
        _needs = GetComponent<PlayerNeeds>();
    }

    private void Start ()
    {
        inventoryWindow.SetActive(false);
        slots = new ItemSlot[uiSlots.Length];

        // initialize the slots
        for(int x = 0; x < slots.Length; x++)
        {
            slots[x] = new ItemSlot();
            uiSlots[x].index = x;
            uiSlots[x].Clear();
        }

        ClearSelectedItemWindow();
    }

    public void OnInventoryButton (InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started)
        {
            Toggle();
        }
    }

    public void Toggle ()
    {
        if(inventoryWindow.activeInHierarchy)
        {
            inventoryWindow.SetActive(false);
            onCloseInventory.Invoke();
            _controller.ToggleCursor(false);
        }
        else
        {
            inventoryWindow.SetActive(true);
            onOpenInventory.Invoke();
            ClearSelectedItemWindow();
            _controller.ToggleCursor(true);
        }    
    }

    public bool IsOpen ()
    {
        return inventoryWindow.activeInHierarchy;
    }

    public void AddItem (ItemData item)
    {
        if(item.canStack)
        {
            ItemSlot slotToStackTo = GetItemStack(item);

            if(slotToStackTo != null)
            {
                slotToStackTo.quantity++;
                UpdateUI();
                return;
            }
        }

        ItemSlot emptySlot = GetEmptySlot();

        if(emptySlot != null)
        {
            emptySlot.item = item;
            emptySlot.quantity = 1;
            UpdateUI();
            return;
        }

        ThrowItem(item);
    }

    private void ThrowItem (ItemData item)
    {
        Instantiate(item.dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360.0f));
    }

    private void UpdateUI ()
    {
        for(int x = 0; x < slots.Length; x++)
        {
            if(slots[x].item != null)
                uiSlots[x].Set(slots[x]);
            else
                uiSlots[x].Clear();
        }
    }

    private ItemSlot GetItemStack (ItemData item)
    {
        for(int x = 0; x < slots.Length; x++)
        {
            if(slots[x].item == item && slots[x].quantity < item.maxStackAmount)
                return slots[x];
        }

        return null;
    }

    private ItemSlot GetEmptySlot ()
    {
        for(int x = 0; x < slots.Length; x++)
        {
            if(slots[x].item == null)
                return slots[x];
        }

        return null;
    }

    public void SelectItem (int index)
    {
        if(slots[index].item == null)
            return;

        _selectedItem = slots[index];
        _selectedItemIndex = index;

        selectedItemName.text = _selectedItem.item.displayName;
        selectedItemDescription.text = _selectedItem.item.description;

        selectedItemStatNames.text = string.Empty;
        selectedItemStatValues.text = string.Empty;

        for(int x = 0; x < _selectedItem.item.consumables.Length; x++)
        {
            selectedItemStatNames.text += _selectedItem.item.consumables[x].type.ToString() + "\n";
            selectedItemStatValues.text += _selectedItem.item.consumables[x].value.ToString() + "\n";
        }

        useButton.SetActive(_selectedItem.item.type == ItemType.Consumable);
        equipButton.SetActive(_selectedItem.item.type == ItemType.Equipable && !uiSlots[index].equipped);
        unEquipButton.SetActive(_selectedItem.item.type == ItemType.Equipable && uiSlots[index].equipped);
        dropButton.SetActive(true);
    }

    private void ClearSelectedItemWindow ()
    {
        _selectedItem = null;
        selectedItemName.text = string.Empty;
        selectedItemDescription.text = string.Empty;
        selectedItemStatNames.text = string.Empty;
        selectedItemStatValues.text = string.Empty;

        useButton.SetActive(false);
        equipButton.SetActive(false);
        unEquipButton.SetActive(false);
        dropButton.SetActive(false);
    }

    public void OnUseButton ()
    {
        if(_selectedItem.item.type == ItemType.Consumable)
        {
            for(int x = 0; x < _selectedItem.item.consumables.Length; x++)
            {
                switch(_selectedItem.item.consumables[x].type)
                {
                    case ConsumableType.Health: _needs.Heal(_selectedItem.item.consumables[x].value); break;
                    case ConsumableType.Hunger: _needs.Eat(_selectedItem.item.consumables[x].value); break;
                    case ConsumableType.Thirst: _needs.Drink(_selectedItem.item.consumables[x].value); break;
                    case ConsumableType.Sleep: _needs.Sleep(_selectedItem.item.consumables[x].value); break;
                }
            }
        }

        RemoveSelectedItem();
    }

    public void OnEquipButton ()
    {
        if(uiSlots[_curEquipIndex].equipped)
            UnEquip(_curEquipIndex);

        uiSlots[_selectedItemIndex].equipped = true;
        _curEquipIndex = _selectedItemIndex;
        EquipManager.instance.EquipNew(_selectedItem.item);
        UpdateUI();

        SelectItem(_selectedItemIndex);
    }

    private void UnEquip (int index)
    {
        uiSlots[index].equipped = false;
        EquipManager.instance.UnEquip();
        UpdateUI();

        if(_selectedItemIndex == index)
            SelectItem(index);
    }    

    public void OnUnEquipButton ()
    {
        UnEquip(_selectedItemIndex);
    }

    public void OnDropButton ()
    {
        ThrowItem(_selectedItem.item);
        RemoveSelectedItem();
    }

    private void RemoveSelectedItem ()
    {
        _selectedItem.quantity--;

        if(_selectedItem.quantity == 0)
        {
            if(uiSlots[_selectedItemIndex].equipped == true)
                UnEquip(_selectedItemIndex);

            _selectedItem.item = null;
            ClearSelectedItemWindow();
        }

        UpdateUI();
    }

    public void RemoveItem (ItemData item)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if(slots[i].item == item)
            {
                slots[i].quantity--;

                if(slots[i].quantity == 0)
                {
                    if(uiSlots[i].equipped == true)
                        UnEquip(i);

                    slots[i].item = null;
                    ClearSelectedItemWindow();
                }

                UpdateUI();
                return;
            }
        }
    }

    public bool HasItems (ItemData item, int quantity)
    {
        int amount = 0;

        for(int i = 0; i < slots.Length; i++)
        {
            if(slots[i].item == item)
                amount += slots[i].quantity;

            if(amount >= quantity)
                return true;
        }

        return false;
    }
}

public class ItemSlot
{
    public ItemData item;
    public int quantity;
}