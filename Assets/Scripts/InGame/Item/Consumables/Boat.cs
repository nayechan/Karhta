using System;
using UnityEngine;

namespace InGame.Item.Consumables
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Boat", menuName = "Item/Boat")]
    public class Boat : Consumable
    {
        [SerializeField] private GameObject boatPrefab;
        

        public override Sprite GetItemPreview()
        {
            var texture = RuntimePreviewGenerator.GenerateModelPreview(boatPrefab.transform, shouldCloneModel: true);
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one / 2.0f);
        }

        public override void UseItem(Player.Player player)
        {
            var itemId = AddressableHelper.Instance.FindItemId(this);
            if (player.PlayerItemData.GetCount(itemId) > 0)
            {
                player.PlayerItemData.ReduceCount(itemId, 1);
                var boat = Instantiate(
                    boatPrefab, 
                    player.transform.position + Vector3.up * 2 + player.transform.forward * 3,
                    player.transform.rotation);
            }
        }

        public override string GetItemType() => "Boat";

        public override bool IsItemCountShown() => false;

        public override string GetDescription() => "Used to spawn boat";

        public override void Consume(Player.Player player)
        {
            throw new System.NotImplementedException();
        }
    }
}