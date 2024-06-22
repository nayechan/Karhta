using System;
using System.Collections.Generic;
using InGame.Battle;
using UnityEngine;

namespace InGame.Item
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Item/Weapon")]
    public class Weapon : Item
    {
        [SerializeField] private List<IDamageable.DamageableType> damageableList;
        [SerializeField] private long damagePoint;
        [SerializeField] private GameObject weaponPrefab;
        [SerializeField] private AnimationClip animationClip;
        
        [field: SerializeField] public bool MultiTargeting { get; private set; }

        public long GetDamagePoint()
        {
            return damagePoint;
        }

        public bool IsDamageable(IDamageable.DamageableType damageableType)
        {
            return damageableList.Contains(damageableType);
        }

        public GameObject InstantiateWeapon(Transform transform)
        {
            return Instantiate(weaponPrefab, transform);
        }

        public AnimationClip GetAnimationClip()
        {
            return animationClip;
        }

        public override Sprite GetItemPreview()
        {
            var texture = RuntimePreviewGenerator.GenerateModelPreview(weaponPrefab.transform, shouldCloneModel: true);
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), Vector2.one / 2.0f);
        }

        public override void UseItem(Player.Player player)
        {
            player.SetCurrentWeapon(this);
        }

        public override string GetItemType() => "Weapon";
        public override bool IsItemCountShown() => false;

        public override string GetDescription()
        {
            return $"Attack +{damagePoint:N0}";
        }
    }
}