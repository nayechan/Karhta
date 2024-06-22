using System;
using System.Collections.Generic;

namespace InGame.Player
{
    [Serializable]
    public class ItemData
    {
        private Dictionary<string, int> itemCount;

        public ItemData()
        {
            itemCount = new Dictionary<string, int>();
        }

        public void SetCount(string itemId, int count)
        {
            itemCount[itemId] = count;
        }

        public void AddCount(string itemId, int amount)
        {
            var maxStack = AddressableHelper.Instance.GetItem(itemId).MaxStack;
            
            if (!itemCount.TryAdd(itemId, amount))
            {
                itemCount[itemId] += amount;
            }

            if (itemCount[itemId] > maxStack)
                itemCount[itemId] = maxStack;
        }

        public void ReduceCount(string itemId, int amount)
        {
            if (!itemCount.ContainsKey(itemId)) return;
            
            var result = itemCount[itemId] - amount;

            if (result <= 0)
            {
                itemCount.Remove(itemId);
            }
            else
            {
                itemCount[itemId] = result;
            }
        }

        public int GetCount(string itemId)
        {
            return itemCount.GetValueOrDefault(itemId, 0);
        }
        
        public Dictionary<string, int> GetAllItemCount(){return itemCount;}
    }
}