using System;
using Cysharp.Threading.Tasks;
using InGame.Player;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace InGame.UI
{
    public class QuickslotIndicator : MonoBehaviour, IPlayerListener
    {
        [SerializeField] protected Image image;
        [SerializeField] protected int index;

        [SerializeField] protected string itemId = null;

        [SerializeField] protected TMP_Text keyHint, countText;

        public virtual void OnPlayerRefresh(Player.Player _player)
        {
            image.color = Color.clear;
            if (countText != null)
            {
                countText.text = "";
            }
            
            if (keyHint != null)
            {
                keyHint.text = _player.GetKeyBind(index).Replace("Alpha","");
            }
            
            itemId = _player.PlayerQuickSlotData.GetSlot(index);
            
            if (string.IsNullOrEmpty(itemId))
            {
                return;
            }

            var item = AddressableHelper.Instance.GetItem(itemId);

            if (item == null)
            {
                return;
            }
            
            image.color = Color.white;
            image.sprite = item.GetItemPreview();
            if (countText != null)
            {
                countText.gameObject.SetActive(item.IsItemCountShown());
                countText.text = _player.PlayerItemData.GetCount(itemId).ToString("N0");
            }
        }
    }
}