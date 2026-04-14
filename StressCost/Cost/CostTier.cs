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
        public static int CostTierS(int amount) => Convert.ToInt32(Mathf.Floor(amount / 1.75f));
        public static int CostTierV(int amount) => Convert.ToInt32(Mathf.Floor(amount / 1.65f));
        public static int CostTierA(int amount) => Convert.ToInt32(Mathf.Floor(amount / 1.55f));
        public static int CostTierF(int amount) => Convert.ToInt32(Mathf.Floor(amount / 1.95f));
    }
}
