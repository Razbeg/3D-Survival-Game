using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingTable : Building, IInteractable
{
    private CraftingWindow _craftingWindow;
    private PlayerController _player;

    private void Start()
    {
        _craftingWindow = FindObjectOfType<CraftingWindow>(true);
        _player = FindObjectOfType<PlayerController>();
    }

    public string GetInteractPrompt()
    {
        return "Craft";
    }

    public void OnInteract()
    {
        _craftingWindow.gameObject.SetActive(true);
        _player.ToggleCursor(true);
    }
}
