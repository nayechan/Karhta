using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace InGame.Item
{
    [Serializable]
    public abstract class Item : ScriptableObject
    {
        [field : SerializeField] public int MaxStack { get; private set; }
        public abstract Sprite GetItemPreview();
        public abstract void UseItem(Player.Player player);
        public abstract string GetItemType();
        public abstract bool IsItemCountShown();
        public abstract string GetDescription();

        public virtual void InstantiateDropItem(Vector3 pos)
        {
            Addressables.InstantiateAsync("Dropitem").Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    var dropItem = op.Result;
                    dropItem.transform.position = pos;
                    dropItem.GetComponent<Dropitem>().Init(this);
                }
            };
        }

        public virtual bool IsEquippable()
        {
            return true;
        }
    }
}