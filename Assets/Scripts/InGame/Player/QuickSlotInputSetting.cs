using System;
using Unity.Collections;
using UnityEngine;

namespace InGame.Player
{
    [Serializable]
    public struct QuickSlotInputSetting
    {
        [field : SerializeField]
        public KeyCode[] KeyBind { get; private set; }
    }
}