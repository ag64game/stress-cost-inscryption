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
    internal class CollectionUIPatches
    {
        [HarmonyPatch(typeof(Card), nameof(Card.RenderCard))]
        [HarmonyPostfix]
        public static void AddRareDecals(Card __instance)
        {
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("nevernamed.inscryption.noraredecals"))
            {
                try
                {
                    Texture2D textureStress = TextureHelper.GetImageAsTexture($"pixel_rare_frame_stress.png", typeof(CostmaniaPlugin).Assembly);
                    Sprite rareDecalStress = Sprite.Create(textureStress, new Rect(0, 0, textureStress.width, textureStress.height), new Vector2(0.5f, 0.487f));

                    Texture2D textureValor = TextureHelper.GetImageAsTexture($"pixel_rare_frame_valor.png", typeof(CostmaniaPlugin).Assembly);
                    Sprite rareDecalValor = Sprite.Create(textureValor, new Rect(0, 0, textureValor.width, textureValor.height), new Vector2(0.5f, 0.487f));

                    Texture2D textureAlchemy = TextureHelper.GetImageAsTexture($"pixel_rare_frame_alchemy.png", typeof(CostmaniaPlugin).Assembly);
                    Sprite rareDecalAlchemy = Sprite.Create(textureAlchemy, new Rect(0, 0, textureAlchemy.width, textureAlchemy.height), new Vector2(0.5f, 0.487f));

                    Texture2D textureSpace = TextureHelper.GetImageAsTexture($"pixel_rare_frame_space.png", typeof(CostmaniaPlugin).Assembly);
                    Sprite rareDecalSpace = Sprite.Create(textureSpace, new Rect(0, 0, textureSpace.width, textureSpace.height), new Vector2(0.5f, 0.487f));

                    SpriteRenderer decalRenderer = __instance.gameObject.FindChild("Base").FindChild("PixelSnap").FindChild("CardElements").FindChild("RareCardDetail").GetComponent<SpriteRenderer>();

                    if (__instance.Info.GetModPrefix() != null && __instance.Info.metaCategories.Contains(CardMetaCategory.Rare))
                    {
                        if (__instance.Info.GetModPrefix().Contains("Stress")) decalRenderer.sprite = rareDecalStress;
                        else if (__instance.Info.GetModPrefix().Contains("Valor")) decalRenderer.sprite = rareDecalValor;
                        else if (__instance.Info.GetModPrefix().Contains("Alchemy")) decalRenderer.sprite = rareDecalAlchemy;
                        else if (__instance.Info.GetModPrefix().Contains("Space")) decalRenderer.sprite = rareDecalSpace;
                    }

                }
                catch { }
            }

        }

        [HarmonyPatch(typeof(CollectionUI), nameof(CollectionUI.Start))]
        public class AddCostsTab
        {
            public static void Prefix(ref CollectionUI __instance)
            {
                AddTab(__instance, "Alchemy", new Vector3(-0.718f, 0.175f, 0));
                AddTab(__instance, "Stress", new Vector3(-0.242f, 0.175f, 0));
                AddTab(__instance, "Space", new Vector3(0.234f, 0.175f, 0));
                AddTab(__instance, "Valor", new Vector3(0.71f, 0.175f, 0));
            }
            public static void AddTab(CollectionUI instance, string name, Vector3 position)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(instance.gameObject.transform.Find("MainPanel").Find("Tabs").Find("Tab_4").gameObject);
                gameObject.name = $"Tab_{name}";

                gameObject.transform.parent = instance.gameObject.transform.Find("MainPanel").Find("Tabs");
                gameObject.transform.localPosition = position;

                instance.tabButtons.Add(gameObject.GetComponent<GenericUIButton>());
                gameObject.GetComponent<GenericUIButton>().inputKey = KeyCode.Alpha5;
                gameObject.GetComponent<GenericUIButton>().OnButtonDown = (Action<GenericUIButton>)instance.gameObject.transform.Find("MainPanel").Find("Tabs").Find("Tab_4").gameObject.GetComponent<GenericUIButton>().OnButtonDown;
                gameObject.GetComponent<BoxCollider2D>().size = new Vector2(0.55f, 0.38f);

                Texture2D image = TextureHelper.GetImageAsTexture($"temple_{name.ToLower()}.png", typeof(CostmaniaPlugin).Assembly);
                gameObject.gameObject.transform.Find("Icon").gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(image, new Rect(0f, 0f, (float)image.width, (float)image.height), new Vector2(0.5f, 0.5f));
            }
        }

        [HarmonyPatch(typeof(CollectionUI), "CreatePages")]
        [HarmonyPostfix]
        public static void SortCards(ref CollectionUI __instance, ref List<List<CardInfo>> __result, ref List<CardInfo> cards)
        {
            //Take the amount of buttons for array
            int[] pageTrackers;
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("mrfantastik.inscryption.infact2"))
                pageTrackers = new int[9];
            else
                pageTrackers = new int[8];

            Console.WriteLine(pageTrackers.Length);

            List<List<CardInfo>> res = new List<List<CardInfo>>();
            int index = 0;

            foreach (CardTemple temple in Enum.GetValues(typeof(CardTemple))) if (temple != CardTemple.NUM_TEMPLES)
                {
                    //Get all the cards of the temple who aren't from the new Temples. Inject them to __results
                    List<CardInfo> list = cards.FindAll(info => info.temple == temple && (info.GetModPrefix() == null || !CostmaniaPlugin.NEW_TEMPLES.Any(newTemple => info.GetModPrefix().Contains(newTemple))));
                    InjectToPixelMenu(ref res, list);

                    pageTrackers[(int)temple] = index;

                    //Variable used for page tracking. Increments based on added pages
                    if (list.Count != 0) index += Convert.ToInt32(Mathf.Ceil(list.Count / 8f)); else index++;
                }

            //Injects Alchemy cards
            List<CardInfo> alchemyCards = cards.FindAll(info => info.GetModPrefix() != null && info.GetModPrefix().Contains("Alchemy"));
            alchemyCards = alchemyCards.OrderBy(info => (info.metaCategories.Contains(CardMetaCategory.Rare) ? 1 : 100))
                .ThenBy(info => (info.GetExtendedPropertyAsInt("ElixirCost")))
                .ThenBy(info => (info.GetExtendedPropertyAsInt("MetalCost")))
                .ThenBy(info => (info.GetExtendedPropertyAsInt("FleshCost")))
                .ThenBy(info => (info.GetExtendedPropertyAsInt("FleshCost") + info.GetExtendedPropertyAsInt("MetalCost") + info.GetExtendedPropertyAsInt("ElixirCost")))
                .ThenBy(info => (info.DisplayedNameEnglish))
                .ToList();

            InjectToPixelMenu(ref res, alchemyCards);
            pageTrackers[4] = index;
            if (alchemyCards.Count != 0) index += Convert.ToInt32(Mathf.Ceil(alchemyCards.Count / 8f)); else index++;

            //Injects Stress cards
            List<CardInfo> stressCards = cards.FindAll(info => info.GetModPrefix() != null && info.GetModPrefix().Contains("Stress"));
            stressCards = stressCards.OrderBy(info => (info.metaCategories.Contains(CardMetaCategory.Rare) ? 1 : 100))
                .ThenBy(info => (info.GetExtendedPropertyAsInt("StressCost")))
                .ThenBy(info => (info.DisplayedNameEnglish))
                .ToList();

            InjectToPixelMenu(ref res, stressCards);
            pageTrackers[5] = index;
            if (stressCards.Count != 0) index += Convert.ToInt32(Mathf.Ceil(stressCards.Count / 8f)); else index++;

            //Injects Space cards
            List<CardInfo> spaceCards = cards.FindAll(info => info.GetModPrefix() != null && info.GetModPrefix().Contains("Space"));
            spaceCards = spaceCards.OrderBy(info => (info.metaCategories.Contains(CardMetaCategory.Rare) ? 1 : 100))
                .ThenBy(info => (info.GetExtendedPropertyAsInt("StardustCost")))
                .ThenBy(info => (info.DisplayedNameEnglish))
                .ToList();

            InjectToPixelMenu(ref res, spaceCards);
            pageTrackers[6] = index;
            if (spaceCards.Count != 0) index += Convert.ToInt32(Mathf.Ceil(spaceCards.Count / 8f)); else index++;

            //Injects Valor cards
            List<CardInfo> valorCards = cards.FindAll(info => info.GetModPrefix() != null && info.GetModPrefix().Contains("Valor"));
            valorCards = valorCards.OrderBy(info => (info.metaCategories.Contains(CardMetaCategory.Rare) ? 1 : 100))
                .ThenBy(info => (info.GetExtendedPropertyAsInt("ValorCost")))
                .ThenBy(info => (info.DisplayedNameEnglish))
                .ToList();

            InjectToPixelMenu(ref res, valorCards);
            pageTrackers[7] = index;
            if (valorCards.Count != 0) index += Convert.ToInt32(Mathf.Ceil(valorCards.Count / 8f)); else index++;

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("mrfantastik.inscryption.infact2"))
            {
                //List<CardInfo> boons = cards.FindAll(info => info.metaCategories.Contains(infact2.Plugin.BoonsPool));
                List<CardInfo> boons = new List<CardInfo>();
                InjectToPixelMenu(ref res, boons);

                pageTrackers[8] = index;
                if (boons.Count != 0) index += Convert.ToInt32(Mathf.Ceil(boons.Count / 8f)); else index++;
            }

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
