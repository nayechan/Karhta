using System;
using UnityEngine;

namespace InGame.Item
{
    [Serializable]
    public abstract class Consumable : Item
    {
        public abstract void Consume(Player.Player player);
    }
}