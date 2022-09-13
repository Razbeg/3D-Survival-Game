using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingRecipeUI : MonoBehaviour
{
    public CraftingRecipe recipe;
    public Image backgroundImage;
    public Image icon;
    public TextMeshProUGUI itemName;
    public Image[] resourceCosts;

    public Color canCraftColor;
    public Color cannotCraftColor;
    private bool _canCraft;

    private void OnEnable ()
    {
        UpdateCanCraft();
    }

    public void UpdateCanCraft ()
    {
        _canCraft = true;

        for(int i = 0; i < recipe.cost.Length; i++)
        {
            if(!Inventory.instance.HasItems(recipe.cost[i].item, recipe.cost[i].quantity))
            {
                _canCraft = false;
                break;
            }
        }

        backgroundImage.color = _canCraft ? canCraftColor : cannotCraftColor;
    }

    private void Start ()
    {
        icon.sprite = recipe.itemToCraft.icon;
        itemName.text = recipe.itemToCraft.displayName;

        for(int i = 0; i < resourceCosts.Length; i++)
        {
            if(i < recipe.cost.Length)
            {
                resourceCosts[i].gameObject.SetActive(true);
                resourceCosts[i].sprite = recipe.cost[i].item.icon;
                resourceCosts[i].transform.GetComponentInChildren<TextMeshProUGUI>().text = recipe.cost[i].quantity.ToString();
            }
            else
                resourceCosts[i].gameObject.SetActive(false);
        }
    }

    public void OnClickButton ()
    {
        if(_canCraft)
        {
            CraftingWindow.instance.Craft(recipe);
        }
    }
}