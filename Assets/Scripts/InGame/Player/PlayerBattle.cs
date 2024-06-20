using InGame.Battle;
using InGame.Item;
using UnityEngine;

namespace InGame.Player
{
    public partial class Player : IAttackable, IDamageable
    {
        private void Attack()
        {
            animator.SetTrigger(AttackTrigger);
            foreach (var damageable in damageablesInArea)
            {
                Damage(damageable);
            }
        }

        public void Damage(IDamageable target)
        {
            long damageAmount = PlayerStatData.StatAtk + 1;

            if (currentWeapon != null)
            {
                damageAmount += currentWeapon.GetDamagePoint();
            }
            
            target.TakeDamage(this, damageAmount);
        }

        public void TakeDamage(IAttackable from, long amount)
        {
            long damageTaken = amount;
            var defStat = PlayerStatData.StatDef;
            
            if(defStat > 0)
                damageTaken -= Random.Range(defStat / 2, defStat);
            
            CurrentHp -= damageTaken;

            if (CurrentHp <= 0)
            {
                Death();
            }
            
            Refresh();
        }

        public void Death()
        {
            
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

            if (currentWeapon == null) return;
            
            var instantiatedWeapon = weapon.InstantiateWeapon(weaponTransform);
        }

        public void GainHP(long amount)
        {
            CurrentHp += amount;
            if (CurrentHp > MaxHp) CurrentHp = MaxHp;
            Refresh();
        }

        public void GainXP(long amount)
        {
            CurrentXP += amount;

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