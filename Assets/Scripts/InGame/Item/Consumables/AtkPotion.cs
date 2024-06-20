using UnityEngine;

namespace InGame.Item.Consumables
{
    [CreateAssetMenu(fileName = "New ATK Potion", menuName = "Item/Potion/ATK Potion")]
    public class AtkPotion : Potion
    {
        public override void UseItem(Player.Player player)
        {
        }
        
        public override string GetDescription()
        {
            return $"ATK +{amount:N0}";
        }
    }
}