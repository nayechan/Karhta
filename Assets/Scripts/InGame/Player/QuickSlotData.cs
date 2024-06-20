using System;
using System.Linq;
using UnityEngine;

namespace InGame.Player
{
    [Serializable]
    public class QuickSlotData
    {
        [SerializeField] private string[] slots;

        public QuickSlotData()
        {
            slots = new string[8];
        }

        public void SetSlot(int index, string itemId)
        {
            slots[index] = itemId;
        }

        public string GetSlot(int index)
        {
            return slots[index];
        }

        public int FindItemIndex(string itemId)
        {
            for(int index = 0; index < slots.Count(); ++index)
            {
                var slot = slots[index];
                if (slot == itemId)
                {
                    return index;
                }
            }

            return -1; // Not found
        }

    }
}