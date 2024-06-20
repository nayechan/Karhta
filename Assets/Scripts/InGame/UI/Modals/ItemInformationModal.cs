using InGame.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI
{
    public class ItemInformationModal : Modal
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text informationText;
        [SerializeField] private Player.Player player;
        [SerializeField] private Transform selectSlotTransform, clearSlotTransform;
        
        public override void Refresh()
        {
            if (!TryGetValue<string>("itemId", out var itemId)) return;
            
            var item = AddressableHelper.Instance.GetItem(itemId);
            image.sprite = item.GetItemPreview();

            var itemName = item.name;
            var itemType = item.GetItemType();
            var itemDesc = item.GetDescription();

            informationText.text = $"{itemName}\n{itemType}\n\n{itemDesc}";

            var isInQuickSlot = player.PlayerQuickSlotData.FindItemIndex(itemId) != -1;
                
            selectSlotTransform.gameObject.SetActive(!isInQuickSlot);
            clearSlotTransform.gameObject.SetActive(isInQuickSlot);
        }

        public void SelectQuickSlot(int index)
        {
            if (!TryGetValue<string>("itemId", out var itemId)) return;
            
            var quickSlotData = player.PlayerQuickSlotData;
            ClearQuickSlot();
            quickSlotData.SetSlot(index, itemId);
            
            player.Refresh();
            Refresh();
        }

        public void ClearQuickSlot()
        {
            if (!TryGetValue<string>("itemId", out var itemId)) return;
            
            var quickSlotData = player.PlayerQuickSlotData;
            var prevSlot = quickSlotData.FindItemIndex(itemId);
            if(prevSlot != -1)
                quickSlotData.SetSlot(prevSlot, "");
            
            player.Refresh();
            Refresh();
        }
    }
}