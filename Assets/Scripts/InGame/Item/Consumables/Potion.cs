using UnityEngine;

namespace InGame.Item.Consumables
{
    public abstract class Potion : Item
    {
        [SerializeField] protected Sprite itemImage;
        [SerializeField] protected long amount;
        
        public override Sprite GetItemPreview()
        {
            return itemImage;
        }
        public override string GetItemType() => "Potion";
        public override bool IsItemCountShown() => true;
    }
}