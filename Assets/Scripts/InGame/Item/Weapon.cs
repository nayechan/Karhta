using System;
using UnityEngine;

namespace InGame.Item
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Item/Weapon")]
    public class Weapon : Item
    {
        [SerializeField] private long damagePoint;
        [SerializeField] private GameObject weaponPrefab;

        public long GetDamagePoint()
        {
            return damagePoint;
        }

        public GameObject InstantiateWeapon()
        {
            return Instantiate(weaponPrefab);
        }
    }
}