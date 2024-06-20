using UnityEngine;

namespace InGame.Item.Consumables
{
    [CreateAssetMenu(fileName = "New DEF Potion", menuName = "Item/Potion/DEF Potion")]
    public class DefPotion : Potion
    {
        public override void UseItem(Player.Player player)
        {
        }
        
        public override string GetDescription()
        {
            return $"DEF +{amount:N0}";
        }
    }
}