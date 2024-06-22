using UnityEngine;

namespace InGame.Item.Consumables
{
    [CreateAssetMenu(fileName = "New HP Potion", menuName = "Item/Potion/HP Potion")]
    public class HpPotion : Potion
    {
        public override void UseItem(Player.Player player)
        {
            var itemId = AddressableHelper.Instance.FindItemId(this);
            if (player.PlayerItemData.GetCount(itemId) > 0)
            {
                player.PlayerItemData.ReduceCount(itemId, 1);
                player.GainHP(amount);
            }
        }
        
        public override string GetDescription()
        {
            return $"HP +{amount:N0}";
        }
    }
}