using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI
{
    public class QuickslotIndicatorButton : QuickslotIndicator
    {
        [SerializeField] private ItemInformationModal modal;
        private void Awake()
        {
            var button = GetComponent<Button>();
            
            if (button != null)
                button.onClick.AddListener(OpenSlot);
        }
        
        public void OpenSlot()
        {
            if (string.IsNullOrEmpty(itemId))
                return;
            
            if (!AddressableHelper.Instance.GetItem(itemId).IsEquippable())
                return;
            
            modal.SetValue("itemId", itemId);
            modal.gameObject.SetActive(true);
        }
    }
}