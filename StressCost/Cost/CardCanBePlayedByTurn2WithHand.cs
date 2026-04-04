using DiskCardGame;
using HarmonyLib;
using InscryptionAPI.Card;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StressCost.Cost
{
    internal class FairHandStress
    {
        public static bool CanBePlayed(int amount, CardInfo card, List<CardInfo> hand)
        {
            // Fair hand kicks in for cards whose Stress cost is 3 or less
            return amount <= StressPlugin.configFairHandCostStress.Value;
        }
    }

    internal class FairHandValor
    {
        public static bool CanBePlayed(int amount, CardInfo card, List<CardInfo> hand)
        {
            // Fair hand kicks in for cards whose Valor cost is 1 or less
            return amount <= StressPlugin.configFairHandCostValor.Value;
        }
    }

    //Fair hand value is the same as valor since Ideally you want a card that takes only 1 Alchemy Die
    internal class FairHandFlesh
    {
        public static bool CanBePlayed(int amount, CardInfo card, List<CardInfo> hand)
        {
            return amount <= StressPlugin.configFairHandCostValor.Value;
        }
    }

    internal class FairHandMetal
    {
        public static bool CanBePlayed(int amount, CardInfo card, List<CardInfo> hand)
        {
            return amount <= StressPlugin.configFairHandCostValor.Value;
        }
    }

    internal class FairHandElixir
    {
        public static bool CanBePlayed(int amount, CardInfo card, List<CardInfo> hand)
        {
            return amount <= StressPlugin.configFairHandCostValor.Value;
        }
    }

    internal class FairHandStardust
    {
        public static bool CanBePlayed(int amount, CardInfo card, List<CardInfo> hand)
        {
            // Fair hand kicks in for cards whose Stardust cost is 2 or less, assuming no other card is eligible for fair hand
            return amount <= StressPlugin.configFairHandCostStardust.Value && hand.FindAll(card => card.EnergyCost <= 1 ||
            card.GetCustomCost("StressCost") <= StressPlugin.configFairHandCostStress.Value || card.GetCustomCost("FleshCost") <= StressPlugin.configFairHandCostValor.Value ||
            card.GetCustomCost("MetalCost") <= StressPlugin.configFairHandCostValor.Value || card.GetCustomCost("ElixirCost") <= StressPlugin.configFairHandCostValor.Value).Count == 0;
        }
    }
}
