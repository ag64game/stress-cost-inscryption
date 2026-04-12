using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StressCost.Cost
{
    internal class CostTier
    {
        public static int CostTierS(int amount) => Convert.ToInt32(Mathf.Floor(amount / 4.5f));
        public static int CostTierV(int amount) => Convert.ToInt32(Mathf.Floor(amount / 5f));
        public static int CostTierA(int amount) => Convert.ToInt32(Mathf.Floor(amount / 3.5f));
        public static int CostTierF(int amount) => Convert.ToInt32(Mathf.Floor(amount / 4.6f));
    }
}
