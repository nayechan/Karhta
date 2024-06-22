using System;
using UnityEngine;

namespace InGame.Item
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Material", menuName = "Item/Material")]
    public class Material : Item
    {
        [SerializeField] private Sprite itemImage;
        public override Sprite GetItemPreview()
        {
            return itemImage;
        }

        public override void UseItem(Player.Player player)
        {
            
        }

        public override string GetItemType()
        {
            return "Material";
        }

        public override bool IsItemCountShown()
        {
            return true;
        }

        public override string GetDescription()
        {
            return "Can not use";
        }

        public bool IsEquippable()
        {
            return false;
        }
    }
}