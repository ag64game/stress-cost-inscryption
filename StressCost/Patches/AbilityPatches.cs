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
    internal class AbilityPatches
    {
        [HarmonyPatch(typeof(ActivatedAbilityBehaviour), nameof(ActivatedAbilityBehaviour.OnActivatedAbility))]
        [HarmonyPostfix]
        public static IEnumerator ActivatedAddCosts(IEnumerator enumerator, ActivatedAbilityBehaviour __instance)
        {
            if (__instance is StressActivatedAbility)
            {
                Cost.StressCost.stressCounter += (__instance as StressActivatedAbility).StressCost;
                if ((__instance as StressActivatedAbility).StressCost > 0 && enumerator != null) foreach (CardSlot slot in Singleton<BoardManager>.Instance.AllSlots.FindAll(slot => slot.Card != null)) OnStressCounterChange(slot.Card, enumerator);
            }

            yield return enumerator;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TakeDamage))]
        [HarmonyPrefix]
        public static void SetupDefenceAbilities(out int __state, PlayableCard __instance, ref int damage, PlayableCard attacker)
        {
            __state = damage;
            if (__instance)
            {
                if (__instance.HasAbility((Ability)5121) && damage > 0)
                {
                    __state--;
                    damage--;
                    __instance.Anim.StrongNegationEffect();
                }
            }
        }

        public static IEnumerator OnStressCounterChange(PlayableCard card, IEnumerator enumerator)
        {
            Console.WriteLine(card == null);
            if (card.LacksAllAbilities()) yield return enumerator;

            if (card.HasAbility(AbilFearmonger.ability))
            {
                CardSlot opponent = card.Slot.opposingSlot;

                card.Anim.PlayAttackAnimation(false, opponent);
                yield return new WaitForSeconds(0.175f);

                if (opponent.Card != null && !opponent.Card.FaceDown) yield return opponent.Card.TakeDamage(card.Attack, card);
                else
                {
                    yield return new WaitForSeconds(0.175f);
                    yield return Singleton<LifeManager>.Instance.ShowDamageSequence(card.Attack, card.Attack, card.OpponentCard, 0.3f, null, 0.15f, true);
                }
            }

            Console.WriteLine(enumerator == null);

            try { yield return enumerator; } finally { }
            yield return true;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.Die))]
        [HarmonyPostfix]
        public static IEnumerator ThanatoKillCount(IEnumerator enumerator, PlayableCard __instance, bool wasSacrifice)
        {
            if (__instance.OpponentCard) VariablestatDeathToll.killCount++;

            yield return enumerator;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        [HarmonyPostfix]
        public static IEnumerator WatchmanPlaceCards(IEnumerator enumerator, TurnManager __instance, bool playerUpkeep)
        {
            if (AbilWatchman.spiedCountEnemy > 0)
            {
                List<CardSlot> slots = Singleton<BoardManager>.Instance.opponentSlots.Where(slot => Singleton<BoardManager>.Instance.GetCardQueuedForSlot(slot) == null).ToList();
                List<PlayableCard> cards = Singleton<BoardManager>.Instance.GetOpponentCards();

                for (int i = 0; i < AbilWatchman.spiedCountEnemy / 2; i++)
                {
                    CardInfo newCard = CardLoader.GetCardByName(cards[UnityEngine.Random.Range(0, cards.Count)].Info.name);


                    if (slots.Count > 0 && AbilWatchman.spiedCountEnemy > 1)
                    {
                        PlayableCard playableCard = CardSpawner.SpawnPlayableCard(newCard);
                        playableCard.SetIsOpponentCard(true);
                        Singleton<TurnManager>.Instance.Opponent.ModifyQueuedCard(playableCard);

                        Singleton<BoardManager>.Instance.QueueCardForSlot(playableCard, slots[UnityEngine.Random.Range(0, slots.Count)]);
                        Singleton<TurnManager>.Instance.Opponent.Queue.Add(playableCard);
                        AbilWatchman.spiedCountEnemy /= 2;
                    }
                }
            }

            if (AbilWatchman.spiedCountPlayer > 0)
            {
                for (int i = 0; i < AbilWatchman.spiedCountPlayer; i++) yield return Singleton<CardDrawPiles>.Instance.DrawCardFromDeck(null, null);
                AbilWatchman.spiedCountPlayer = 0;
            }

            yield return enumerator;
        }
    }
}
