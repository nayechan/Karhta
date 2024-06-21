using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace InGame.UI
{
    public class InventoryItem : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text text;

        private string itemId = "";
        private int itemCount = 0;
    
        public IObjectPool<GameObject> InventoryItemPool { private get; set; }
    
        public void SetData(string _itemId, int _itemCount)
        {
            itemId = _itemId;
            itemCount = _itemCount;
        
            var item = AddressableHelper.Instance.GetItem(itemId);
            image.sprite = item.GetItemPreview();
            text.text = itemCount.ToString();
        }

        public string GetItemId()
        {
            return itemId;
        }

        public int GetItemCount()
        {
            return itemCount;
        }
    }
}
