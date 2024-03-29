﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EquipManager : MonoBehaviour
{
    public Equip curEquip;
    public Transform equipParent;

    private PlayerController _controller;

    // singleton
    public static EquipManager instance;

    private void Awake()
    {
        instance = this;
        _controller = GetComponent<PlayerController>();
    }

    public void OnAttackInput (InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed && curEquip != null && _controller.canLook == true)
        {
            curEquip.OnAttackInput();
        }
    }

    public void OnAltAttackInput (InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed && curEquip != null && _controller.canLook == true)
        {
            curEquip.OnAltAttackInput();
        }
    }

    public void EquipNew (ItemData item)
    {
        UnEquip();
        curEquip = Instantiate(item.equipPrefab, equipParent).GetComponent<Equip>();
    }

    public void UnEquip ()
    {
        if(curEquip != null)
        {
            Destroy(curEquip.gameObject);
            curEquip = null;
        }
    }
}