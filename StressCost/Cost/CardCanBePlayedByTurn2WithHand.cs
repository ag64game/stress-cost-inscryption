using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StressCost.Cost
{
    internal class CardCanBePlayedByTurn2WithHand
    {
        public static bool CanBePlayed(int amount, CardInfo card, List<CardInfo> hand)
        {
            // Fair hand kicks in for cards whose Stress cost is 3 or less
            return card.GetExtendedPropertyAsInt("StressCost") <= StressPlugin.configFairHandCost.Value;
        }
    }
}
