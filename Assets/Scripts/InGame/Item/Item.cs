using System;
using UnityEngine;

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
    }
}