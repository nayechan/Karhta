using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InGame.Item;
using InGame.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCraftController : MonoBehaviour, IPlayerListener
{
    [Serializable]
    public struct ItemIngredient
    {
        public Item item;
        public int count;
    }

    [Serializable]
    public struct ItemCraftData
    {
        public List<ItemIngredient> itemIngredient;
        public Item result;
    }

    [SerializeField] private Player player;
    [SerializeField] private List<ItemCraftData> itemCraftDataset;
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text descText;

    private List<ItemCraftData> filteredCraftDataset = null;
    private int currentIndex = -1;
    private Item currentItem = null;

    private bool IsIngredientSufficient(Player _player, ItemCraftData itemCraftData)
    {
        foreach (var itemIngredient in itemCraftData.itemIngredient)
        {
            var itemId = AddressableHelper.Instance.FindItemId(itemIngredient.item);
            if (_player.PlayerItemData.GetCount(itemId) < itemIngredient.count)
            {
                return false;
            }
        }

        return true;
    }

    public void OnPlayerRefresh(Player _player)
    {
        filteredCraftDataset = itemCraftDataset
            .Where(itemCraftData => IsIngredientSufficient(_player, itemCraftData))
            .ToList();

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (filteredCraftDataset == null || filteredCraftDataset.Count <= 0)
        {
            currentIndex = -1;
            descText.text = "No Available Crafts";
            image.color = Color.clear;
            return;
        }

        if (currentIndex < 0 ||
            filteredCraftDataset.Count < currentIndex)
        {
            currentIndex = 0;
        }
        
        currentItem = filteredCraftDataset[currentIndex].result;

        descText.text = $"[ {currentIndex + 1} / {filteredCraftDataset.Count} ]\n\n";
        descText.text += $"{currentItem.name}\n\n";
        descText.text += "Require :\n";

        foreach (var ingredient in filteredCraftDataset[currentIndex].itemIngredient)
        {
            descText.text += $"{ingredient.count} {ingredient.item.name}\n";
        }
        
        image.color = Color.white;
        image.sprite = currentItem.GetItemPreview();
    }

    public void Craft()
    {
        if (currentIndex < 0) return;

        var itemCraftData = filteredCraftDataset[currentIndex];
        
        foreach (var ingredient in itemCraftData.itemIngredient)
        {
            var itemId = AddressableHelper.Instance.FindItemId(ingredient.item);
            player.PlayerItemData.ReduceCount(itemId, ingredient.count);
        }

        var resultItemId = AddressableHelper.Instance.FindItemId(itemCraftData.result);
        player.PlayerItemData.AddCount(resultItemId, 1);
        
        player.Refresh();
    }

    public void PrevPage()
    {
        if (filteredCraftDataset.Count <= 0) return;
        
        if (currentIndex - 1 >= 0)
            --currentIndex;
        
        RefreshUI();
    }

    public void NextPage()
    {
        if (filteredCraftDataset.Count <= 0) return;
        
        if (currentIndex + 1 < filteredCraftDataset.Count)
            ++currentIndex;
        
        RefreshUI();
    }
}
