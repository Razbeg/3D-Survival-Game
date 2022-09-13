using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    public Button button;
    public Image icon;
    public TextMeshProUGUI quantityText;
    private ItemSlot _curSlot;
    private Outline _outline;

    public int index;
    public bool equipped;

    private void Awake ()
    {
        _outline = GetComponent<Outline>();
    }

    private void OnEnable ()
    {
        _outline.enabled = equipped;
    }

    public void Set (ItemSlot slot)
    {
        _curSlot = slot;

        icon.gameObject.SetActive(true);
        icon.sprite = slot.item.icon;
        quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : string.Empty;

        if(_outline != null)
            _outline.enabled = equipped;
    }

    public void Clear ()
    {
        _curSlot = null;

        icon.gameObject.SetActive(false);
        quantityText.text = string.Empty;
    }

    public void OnButtonClick ()
    {
        Inventory.instance.SelectItem(index);
    }
}