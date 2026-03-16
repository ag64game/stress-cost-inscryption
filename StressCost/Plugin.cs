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
using Pixelplacement.TweenSystem;
using Steamworks;
using StressCost.Cost;
using StressCost.Sigils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using UnityEngine.UI;
using static InscryptionAPI.CardCosts.CardCostManager;


namespace StressCost
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class StressPlugin : BaseUnityPlugin
    {
        public const string GUID = "aga.stress";
        public const string NAME = "StressCost";
        private const string VERSION = "0.0.6.7";

        public static string Directory;
        internal static ManualLogSource Log;

        Harmony harmony = new Harmony(GUID);

        internal static ConfigEntry<bool> configFairHandActive;
        internal static ConfigEntry<int> configFairHandCost;

        public static PixelNumeral disStressCounter;

        private void Awake()
        {


            Log = base.Logger;
            Directory = base.Info.Location.Replace("StressCost.dll", "");
            harmony.PatchAll();
            configFairHandActive = base.Config.Bind<bool>("Fair Hand", "Active", true, "Should this mod post-fix patch fair hand to include the Money, Life, and Hybrid Costs?");
            configFairHandCost = base.Config.Bind<int>("Fair Hand", "Stress Cost", 3, "The value in which the card should not show up in fair hand.");

            AddCost();
            AddSigils();

            harmony.PatchAll(typeof(StressPlugin));
        }

        public static void AddCost()
        {
            FullCardCost stressCost = Register(GUID, "StressCost", typeof(Cost.StressCost), Cost.StressCost.Texture_3D, Cost.StressCost.Texture_Pixel);
            stressCost.SetCostTier(Cost.CostTier.CostTierS);
            stressCost.ResourceType = (ResourceType)42;

            if (configFairHandActive.Value)
            {
                stressCost.SetCanBePlayedByTurn2WithHand(Cost.CardCanBePlayedByTurn2WithHand.CanBePlayed);
            }
        }

        public static void AddSigils()
        {
            AbilRelaxant.AddRelaxant();
            AbilAffection.AddAffection();
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.ResolveCardOnBoard))]
        [HarmonyPostfix]
        public static IEnumerator PayStressCost(IEnumerator enumerator, BoardManager __instance, PlayableCard card, CardSlot slot)
        {
            if (slot.IsPlayerSlot) Cost.StressCost.stressCounter += Convert.ToInt32(card.Info.GetExtendedProperty("StressCost"));

            return enumerator;
        }

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoCombatPhase))]
        [HarmonyPostfix]
        public static IEnumerator ApplyStress(IEnumerator enumerator, TurnManager __instance, bool playerIsAttacker)
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
            } finally { }
            yield return enumerator;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        public static IEnumerator ResetStress(IEnumerator enumerator, TurnManager __instance, EncounterData encounterData)
        {
            Cost.StressCost.stressCounter = 0;

            try
            {
                GameObject stress = Instantiate<GameObject>(PixelResourcesManager.Instance.gameObject.transform.Find("Bones").gameObject);
                stress.SetActive(true);
                stress.transform.SetParent(PixelResourcesManager.Instance.gameObject.transform);
                stress.layer = 31;
                stress.name = "Stress";

                GameObject stressDis = stress.gameObject.FindChild("BoneIcon");
                stressDis.name = "StressCounter";

                Texture2D texture = TextureHelper.GetImageAsTexture($"displaycost_stress.png", typeof(StressPlugin).Assembly);

                stressDis.GetComponent<SpriteRenderer>().sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(13.7f, -6f)
                    );

                GameObject numsBorder = stressDis.gameObject.FindChild("PixelBorderNumeral");
                disStressCounter = numsBorder.GetComponent<PixelNumeral>();
                numsBorder.transform.position = new Vector3(-2.08f, 0.87f, 0);
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return enumerator;
        }

        [HarmonyPatch(typeof(Card), nameof(Card.RenderCard))]
        [HarmonyPostfix]
        public static void AddRareDecals(Card __instance)
        {
            try
            {
                Texture2D texture = TextureHelper.GetImageAsTexture($"pixel_rare_frame_stress.png", typeof(StressPlugin).Assembly);
                Sprite rareDecal = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.487f));

                if (__instance.Info.GetModPrefix().Contains("Stress") && __instance.Info.metaCategories.Contains(CardMetaCategory.Rare))
                    __instance.gameObject.FindChild("Base").FindChild("PixelSnap").FindChild("CardElements").FindChild("RareCardDetail").GetComponent<SpriteRenderer>().sprite = rareDecal;
            }
            catch { }
        }
        

        [HarmonyPatch(typeof(CollectionUI), nameof(CollectionUI.Start))]
        public class AddStressTab
        {
            public static void Postfix(ref CollectionUI __instance)
            {
                AddTab(__instance);
            }
            public static void AddTab(CollectionUI instance)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(instance.gameObject.transform.Find("MainPanel").Find("Tabs").Find("Tab_4").gameObject);
                gameObject.name = "Tab_Stress";

                gameObject.transform.parent = instance.gameObject.transform.Find("MainPanel").Find("Tabs");
                gameObject.transform.localPosition = new Vector3(-0.718f, 0.175f, 0);

                instance.tabButtons.Add(gameObject.GetComponent<GenericUIButton>());
                gameObject.GetComponent<GenericUIButton>().inputKey = KeyCode.Alpha5;
                gameObject.GetComponent<GenericUIButton>().OnButtonDown = (Action<GenericUIButton>)instance.gameObject.transform.Find("MainPanel").Find("Tabs").Find("Tab_4").gameObject.GetComponent<GenericUIButton>().OnButtonDown;
                gameObject.GetComponent<BoxCollider2D>().size = new Vector2(0.55f, 0.44f);
                
                Texture2D image = TextureHelper.GetImageAsTexture($"temple_stress.png", typeof(StressPlugin).Assembly);
                gameObject.gameObject.transform.Find("Icon").gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(image, new Rect(0f, 0f, (float)image.width, (float)image.height), new Vector2(0.5f, 0.5f));
            }
        }


        [HarmonyPatch(typeof(CollectionUI), "CreatePages")]
        [HarmonyPostfix]
        public static void SortStressCards(ref CollectionUI __instance, ref List<List<CardInfo>> __result, ref List<CardInfo> cards)
        {
            //Take the amount of buttons for array
            int[] pageTrackers;
            if(!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("mrfantastik.inscryption.infact2")) 
                pageTrackers = new int[__instance.gameObject.transform.Find("MainPanel").childCount];
            else
                pageTrackers = new int[__instance.gameObject.transform.Find("MainPanel").childCount + 1];
            Console.WriteLine(pageTrackers.Length);

            List<List<CardInfo>> res = new List<List<CardInfo>>();
            int index = 0;

            foreach (CardTemple temple in Enum.GetValues(typeof(CardTemple))) if (temple != CardTemple.NUM_TEMPLES)
            {
                //Get all the cards of the temple who aren't Stress cards. Inject them to __results
                List<CardInfo> list = cards.FindAll(info => info.temple == temple && info.GetModPrefix() != "Stress");
                InjectToPixelMenu(ref res, list);

                pageTrackers[(int)temple] = index;

                //Variable used for page tracking. Increments based on added pages
                if (list.Count != 0) index += Convert.ToInt32(Mathf.Ceil(list.Count / 8f)); else index++;
            }
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("mrfantastik.inscryption.infact2"))
            {
                res.Add(new List<CardInfo>());

                pageTrackers[(int)CardTemple.NUM_TEMPLES] = index;
                index++;
            }

            //Injects Stress cards
            List<CardInfo> stressCards = cards.FindAll(info => info.GetModPrefix() == "Stress");
            stressCards = stressCards.OrderBy(info =>  (info.metaCategories.Contains(CardMetaCategory.Rare) ? 1 : 100))
                .ThenBy(info => (info.GetExtendedPropertyAsInt("StressCost")))
                .ThenBy(info => (info.DisplayedNameEnglish))
                .ToList();

            InjectToPixelMenu(ref res, stressCards);
            pageTrackers[pageTrackers.Length - 1] = index;

            //Injects all variables proper. Dynamic injection would cause NullPointerExceptions
            __result = res;
            __instance.tabPageIndices = pageTrackers;
        }

        //Need to inject into __result in chunks of 8. Each CardInfo List inside the large List represents a single page in the menu
        private static void InjectToPixelMenu(ref List<List<CardInfo>> __result, List<CardInfo> cards)
        {
            List<CardInfo> page = new List<CardInfo>();

            if (cards.Count != 0)
            {
                for (int i = 1; i <= cards.Count; i++)
                {
                    page.Add(cards[i - 1]);
                    if (i % 8 == 0 || i == cards.Count)
                    {
                        if (page.Count > 0) __result.Add(page);
                        page = new List<CardInfo>();
                    }
                }
            }
            else __result.Add(new List<CardInfo>());
        }
    }

}