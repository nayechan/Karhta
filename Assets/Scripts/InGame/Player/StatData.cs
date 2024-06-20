using System;
using UnityEngine;

namespace InGame.Player
{
    [Serializable]
    public class StatData
    {
        public int Ap { get; private set;}
        public int StatAtk { get; private set; }
        public int StatDef { get; private set; }
        public int StatHp { get; private set; }

        public StatData()
        {
            Ap = 0;
            StatAtk = 0;
            StatDef = 0;
            StatHp = 0;
        }

        public void AddAp(int amount)
        {
            Ap += amount;
        }

        public void AddStat(string statType)
        {
            if (Ap > 0)
            {
                --Ap;
                switch (statType)
                {
                    case "atk":
                        ++StatAtk;
                        break;
                    case "def":
                        ++StatDef;
                        break;
                    case "hp":
                        StatHp += 5;
                        break;
                }
            }
        }

        public float GetStat(string statType)
        {
            switch (statType)
            {
                case "ap":
                    return Ap;
                case "atk":
                    return StatAtk;
                case "def":
                    return StatDef;
                case "hp":
                    return StatHp;
            }

            return 0;
        }
    }
}