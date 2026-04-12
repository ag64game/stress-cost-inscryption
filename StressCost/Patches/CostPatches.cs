using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DiskCardGame;
using EasyFeedback.APIs;
using GBC;
using HarmonyLib;
using InscryptionAPI.Ascension;
using InscryptionAPI.Boons;
using InscryptionAPI.Card;
using InscryptionAPI.Encounters;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Nodes;
using InscryptionAPI.PixelCard;
using InscryptionAPI.Regions;
using InscryptionAPI.Sound;
using InscryptionAPI.Triggers;
using InscryptionCommunityPatch.Card;
using InscryptionCommunityPatch.PixelTutor;
using Pixelplacement;
using Pixelplacement.TweenSystem;
using Sirenix.Serialization.Utilities;
using Steamworks;
using StressCost.Cost;
using StressCost.Sigils;
using StressCost.Sigils.VariableStats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using UnityEngine.UI;
using static InscryptionAPI.CardCosts.CardCostManager;
using static System.Net.Mime.MediaTypeNames;

namespace StressCost.Patches
{
    internal class CostPatches
    {
        public static bool dontPay = false;
        public static bool isPlayerTwo = false;


        public static void Update()
        {
            Cost.ValorCost.SetMaxRank();
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.ResolveCardOnBoard))]
        [HarmonyPostfix]
        public static IEnumerator PayCosts(IEnumerator enumerator, BoardManager __instance, PlayableCard card, CardSlot slot)
        {
            if (slot.IsPlayerSlot)
            {
                if (!dontPay)
                {
                    if (card.Info.GetExtendedPropertyAsInt("StressCost") > 0 && (CostmaniaPlugin.config3DStress.Value || SaveManager.SaveFile.IsPart2))
                    {
                        Cost.StressCost.stressCounter += card.Info.GetExtendedPropertyAsInt("StressCost").Value;
                        if (enumerator != null) foreach (CardSlot fearSlot in __instance.AllSlots.FindAll(slot => slot.Card != null && slot.Card.Info.abilities.Count != 0)) yield return Patches.AbilityPatches.OnStressCounterChange(fearSlot.Card, enumerator);
                    }

                    if (CostmaniaPlugin.config3DAlchemy.Value || SaveManager.SaveFile.IsPart2)
                    {
                        if (card.Info.GetExtendedPropertyAsInt("FleshCost") > 0) Patches.CostGraphicPatches.disAlchemyCounter.PayIfPossible(AlchemyValue.Flesh, card.Info.GetExtendedPropertyAsInt("FleshCost").Value);
                        if (card.Info.GetExtendedPropertyAsInt("MetalCost") > 0) Patches.CostGraphicPatches.disAlchemyCounter.PayIfPossible(AlchemyValue.Metal, card.Info.GetExtendedPropertyAsInt("MetalCost").Value);
                        if (card.Info.GetExtendedPropertyAsInt("ElixirCost") > 0) Patches.CostGraphicPatches.disAlchemyCounter.PayIfPossible(AlchemyValue.Elixir, card.Info.GetExtendedPropertyAsInt("ElixirCost").Value);
                    }
                }

                if (CostmaniaPlugin.config3DStardust.Value || SaveManager.SaveFile.IsPart2) StardustCost.stardustCounter++;
            }

            yield return enumerator;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoCombatPhase))]
        [HarmonyPostfix]
        public static IEnumerator ApplyStress(IEnumerator enumerator, TurnManager __instance, bool playerIsAttacker)
        {
            if (CostmaniaPlugin.config3DStress.Value || SaveManager.SaveFile.IsPart2)
            {
                try
                {
                    if (Cost.StressCost.stressCounter > 0)
                    {
                        if (playerIsAttacker)
                        {
                            int damage = Convert.ToInt32(Mathf.Floor(Cost.StressCost.stressCounter / 2f));

                            if (damage > 0) yield return Singleton<LifeManager>.Instance.ShowDamageSequence(damage, damage, true, 0.3f, null, 0.15f, true);

                            if (Cost.StressCost.stressCounter < 2) Cost.StressCost.stressCounter = 0; else Cost.StressCost.stressCounter -= 2;
                        }

                        if (Singleton<LifeManager>.Instance.PlayerDamage - Singleton<LifeManager>.Instance.OpponentDamage < 5) yield return enumerator; else yield break;
                    }
                    else yield return enumerator;
                }
                finally { }
            }
            yield return enumerator;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        public class PromotionPhase
        {
            public static IEnumerator Postfix(IEnumerator enumerator, TurnManager __instance, bool playerUpkeep)
            {
                if (playerUpkeep && __instance.TurnNumber > 1 && (CostmaniaPlugin.config3DValor.Value || SaveManager.SaveFile.IsPart2)) yield return DoPromotionPhase();
                yield return enumerator;
            }

            private static IEnumerator DoPromotionPhase()
            {
                var board = Singleton<BoardManager>.Instance;

                List<CardSlot> all = board.playerSlots;
                List<CardSlot> available = all.FindAll(slot => slot.Card != null && !slot.Card.HasTrait(Trait.Terrain));

                if (available.Count > 0) yield return board.ChooseTarget(all, available, PromotionSuccess, PromotionFailed, CursorEnteredSlot, () => false, CursorType.Sacrifice);

                yield return true;
            }

            private static void PromotionSuccess(CardSlot slot)
            {
                var mod = new CardModificationInfo(0, 0);
                mod.SetExtendedProperty("ValorRank", 1);

                slot.Card.AddTemporaryMod(mod);

                Cost.ValorCost.SetMaxRank();
            }

            private static void PromotionFailed(CardSlot slot)
            {
                if (slot.Card != null && slot.Card.Info.HasTrait(Trait.Terrain))
                {
                    slot.Card.Anim.StrongNegationEffect();
                    slot.PlaySound();
                }
            }

            private static void CursorEnteredSlot(CardSlot slot)
            {

            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.DestroyWhenStackIsClear))]
        public static IEnumerator ResetMaxValor(IEnumerator enumerator, PlayableCard __instance)
        {
            if (SaveManager.SaveFile.IsPart2) Cost.ValorCost.SetMaxRank();
            yield return enumerator;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        public static IEnumerator IncrementAlchemy(IEnumerator enumerator, TurnManager __instance, bool playerUpkeep)
        {
            if (playerUpkeep && SaveManager.SaveFile.IsPart2)
            {
                Patches.CostGraphicPatches.disAlchemyCounter.AddDies();


                if (__instance.TurnNumber < 2)
                {
                    List<AlchemyValue> available = new List<AlchemyValue>();
                    foreach (PlayableCard card in Singleton<PlayerHand>.Instance.CardsInHand)
                    {
                        if (card.Info.GetExtendedProperty("FleshCost") != null && card.Info.GetExtendedProperty("MetalCost") == null && card.Info.GetExtendedProperty("ElixirCost") == null)
                            available.Add(AlchemyValue.Flesh);

                        if (card.Info.GetExtendedProperty("MetalCost") != null && card.Info.GetExtendedProperty("FleshCost") == null && card.Info.GetExtendedProperty("ElixirCost") == null)
                            available.Add(AlchemyValue.Metal);

                        if (card.Info.GetExtendedProperty("ElixirCost") != null && card.Info.GetExtendedProperty("MetalCost") == null && card.Info.GetExtendedProperty("FleshCost") == null)
                            available.Add(AlchemyValue.Elixir);
                    }

                    if (available.Count > 0) Patches.CostGraphicPatches.disAlchemyCounter.RollDies(specific: available[UnityEngine.Random.Range(0, available.Count)]);
                    else Patches.CostGraphicPatches.disAlchemyCounter.RollDies();
                }
                else Patches.CostGraphicPatches.disAlchemyCounter.RollDies();
            }
            yield return enumerator;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        public static IEnumerator ResetStardust(IEnumerator enumerator, TurnManager __instance, bool playerUpkeep)
        {
            StardustCost.stardustCounter = 0;
            yield return enumerator;
        }


        [HarmonyPatch(typeof(AbilityBehaviour), nameof(AbilityBehaviour.PreSuccessfulTriggerSequence))]
        [HarmonyPrefix]
        public static void MakeAbilNotPay(AbilityBehaviour __instance)
        {
            dontPay = true;
        }

        [HarmonyPatch(typeof(AbilityBehaviour), nameof(AbilityBehaviour.LearnAbility))]
        [HarmonyPrefix]
        public static void EndAbilPay(AbilityBehaviour __instance)
        {
            dontPay = false;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.Die))]
        [HarmonyPrefix]
        public static void AbilDiePlay(PlayableCard __instance, bool wasSacrifice)
        {
            dontPay = false;
        }


        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        [HarmonyPostfix]
        public static IEnumerator SwitchPlayers(IEnumerator enumerator, TurnManager __instance, bool playerUpkeep)
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("tvflabs.inscryption.MultiplayerMod"))
            {
                if (__instance.TurnNumber > 1)
                {
                    Patches.CostGraphicPatches.disAlchemyCounter.SwitchPlayer();
                    Cost.StressCost.SwitchPlayer();
                }
            }

            yield return enumerator;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.CleanupPhase))]
        [HarmonyPrefix]
        public static void ResetPlayerTwo(TurnManager __instance)
        {
            Patches.CostGraphicPatches.disAlchemyCounter.ResetPlayerTwo();
            Cost.StressCost.ResetPlayerTwo();
        }
    }
}
