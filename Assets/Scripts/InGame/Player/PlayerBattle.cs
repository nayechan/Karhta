using System.Collections.Generic;
using System.Linq;
using InGame.Battle;
using InGame.Item;
using UnityEngine;

namespace InGame.Player
{
    public partial class Player : IAttackable, IDamageable
    {
        public bool IsAttackable()
        {
            return true;
        }
        
        public bool IsDamageable()
        {
            return true;
        }

        public IDamageable.DamageableType GetDamageableType()
        {
            return IDamageable.DamageableType.Player;
        }
        
        private void Attack()
        {
            animator.SetTrigger(AttackTrigger);
            
            
            List<IDamageable> filteredDamageables = damageablesInArea;

            if (currentWeapon != null)
            {
                filteredDamageables = filteredDamageables
                    .Where(damageable => currentWeapon.IsDamageable(damageable.GetDamageableType()))
                    .ToList();
            }

            if (currentWeapon == null || !currentWeapon.MultiTargeting)
            {
                if(filteredDamageables.Count > 0)
                    Damage(filteredDamageables[0]);
            }
            else
            {
                foreach (var damageable in filteredDamageables)
                {
                    Damage(damageable);
                }
            }
            
        }

        public void Damage(IDamageable target)
        {
            long damageAmount = Mathf.RoundToInt(PlayerStatData.StatAtk * allStatMultiplier + 1);

            if (currentWeapon != null)
            {
                damageAmount += currentWeapon.GetDamagePoint();
            }
            
            if(IsAttackable() && target.IsDamageable())
                target.TakeDamage(this, damageAmount);
        }

        public void TakeDamage(IAttackable from, long amount)
        {
            long damageTaken = amount;
            var defStat = Mathf.RoundToInt(PlayerStatData.StatDef * allStatMultiplier);
            
            if(defStat > 0)
                damageTaken -= Random.Range(defStat / 2, defStat);

            if (damageTaken < 0)
                damageTaken = 0;
            
            CurrentHp -= damageTaken;

            if (CurrentHp <= 0)
            {
                Death(from);
            }
            
            Refresh();
        }

        public void Death(IAttackable from)
        {
            from?.OnTargetDead(this);
            gameOverController.gameObject.SetActive(true);
        }

        public void OnTargetDead(IDamageable target)
        {
            damageablesInArea.Remove(target);
        }

        public void SetCurrentWeapon(Weapon weapon)
        {
            
            foreach (Transform _transform in weaponTransform)
            {
                Destroy(_transform.gameObject);
            }

            if (currentWeapon == weapon)
                weapon = null;
            
            currentWeapon = weapon;

            if (currentWeapon == null)
            {
                attackAnimationController.ChangeAttackAnimation(defaultAttackAnimation);
                return;
            }
            
            var attackAnimationClip = currentWeapon.GetAnimationClip();
            if(attackAnimationClip != null)
                attackAnimationController.ChangeAttackAnimation(attackAnimationClip);
            
            var instantiatedWeapon = currentWeapon.InstantiateWeapon(weaponTransform);
        }

        public Weapon GetCurrentWeapon()
        {
            return currentWeapon;
        }
        

        public void GainHP(long amount)
        {
            CurrentHp += amount;
            if (CurrentHp > MaxHp) CurrentHp = MaxHp;
            Refresh();
        }

        public void GainXP(long amount)
        {
            CurrentXP += Mathf.RoundToInt(amount * xpMultiplier);

            if (CurrentXP >= MaxXp)
            {
                CurrentXP -= MaxXp;
                ++currentLevel;
                PlayerStatData.AddAp(5);
            }
            
            Refresh();
        }

        public void QuickSlotAction()
        {
            for(var index = 0; index < inputSetting.KeyBind.Length; ++index)
            {
                var input = inputSetting.KeyBind[index];
                if (Input.GetKeyDown(input))
                {
                    var itemId = PlayerQuickSlotData.GetSlot(index);
                    if(!string.IsNullOrEmpty(itemId))
                        AddressableHelper.Instance.GetItem(itemId).UseItem(this);
                }
            }
        }
    }
}