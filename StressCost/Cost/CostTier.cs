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
        public static int CostTierS(int amount) => Convert.ToInt32(Mathf.Floor(amount));
    }
}
