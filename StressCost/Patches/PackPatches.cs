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
    internal class PackPatches
    {
        private static string packDisplay = "Og";
        private static CardTemple packTemple = CardTemple.NUM_TEMPLES;
        private static SpriteRenderer packImg, packRippedRight, packRippedLeft;
        private static List<PixelSelectableCard> packCards, preset;
        private static bool openPack;

        private static bool introStarted = false;
        private static List<PickupCardPileVolume> stones;

        private static bool addedToDeck = false;
        public static void Update()
        {
            if (openPack && SaveManager.SaveFile.IsPart2)
            {
                AddCustomTemples();
                GeneratePack();
                packCards = preset;

                if (packDisplay != "Og" && packImg.isVisible)
                {
                    Texture2D texture = TextureHelper.GetImageAsTexture($"card_pack_{packDisplay.ToLower()}.png", typeof(CostmaniaPlugin).Assembly);
                    packImg.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector3(0.5f, 0.5f, 0.1f));

                    Texture2D rippedLeft = TextureHelper.GetImageAsTexture($"ripped_card_left_{packDisplay.ToLower()}.png", typeof(CostmaniaPlugin).Assembly);
                    packRippedLeft.sprite = Sprite.Create(rippedLeft, new Rect(0, 0, rippedLeft.width, rippedLeft.height), new Vector3(0.5f, 0.5f, 0.1f));

                    Texture2D rippedRight = TextureHelper.GetImageAsTexture($"ripped_card_right_{packDisplay.ToLower()}.png", typeof(CostmaniaPlugin).Assembly);
                    packRippedRight.sprite = Sprite.Create(rippedRight, new Rect(0, 0, rippedRight.width, rippedRight.height), new Vector3(0.5f, 0.5f, 0.1f));
                }


                openPack = false;
            }

            try
            {
                if (packDisplay != "Og" && packImg.isVisible)
                {
                    foreach (GameObject text in Array.FindAll(CostmaniaPlugin.FindObjectsOfType<GameObject>(), obj => obj.name == "DialogueHandler"))
                        if (packDisplay[0] == 'A' || packDisplay[0] == 'I' || packDisplay[0] == 'E' || packDisplay[0] == 'O') text.FindChild("TextBox").FindChild("Box").FindChild("PixelText").GetComponent<PixelText>().SetText($"You recieved an {packDisplay} card pack!");
                        else if (packDisplay == "Stress") text.FindChild("TextBox").FindChild("Box").FindChild("PixelText").GetComponent<PixelText>().SetText($"You recieved a Paranoia card pack!");
                        else text.FindChild("TextBox").FindChild("Box").FindChild("PixelText").GetComponent<PixelText>().SetText($"You recieved a {packDisplay} card pack!");
                }
            }
            catch { }

            if (!addedToDeck && BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("mrfantastik.inscryption.infact2"))
            {
                SetupSideDeckCards();
                addedToDeck = true;
            }
        }

        public static void SetupStarterDecks()
        {
            string[] alchemyDeck = {"Alchemy_Pylon", "Alchemy_Pylon" , "Alchemy_Pylon" , "Alchemy_Pylon" , "Alchemy_Pylon" , "Alchemy_Pylon" , "Alchemy_Homonculus", "Alchemy_Homonculus",
            "Alchemy_Zeppeloid", "Alchemy_Zeppeloid", "Alchemy_Spite", "Alchemy_Spite", "Alchemy_Biosynth", "Alchemy_Biosynth", "Alchemy_CarnivorousPylop", "Alchemy_Biborg", "Alchemy_Biborg", 
            "Alchemy_CarnivorousPylop", "Alchemy_CarnivorousPylop", "Alchemy_Biborg"};
            GBC.StarterDecks.NATURE_STARTER.AddRange(alchemyDeck);

            string[] stressDeck = { "Stress_Pills", "Stress_Pills", "Stress_Pills", "Stress_Pills", "Stress_Pills", "Stress_Myso", "Stress_Myso", "Stress_Myso", "Stress_Ekrixi",
            "Stress_Ekrixi", "Stress_Venustra", "Stress_Venustra", "Stress_Obeso", "Stress_Obeso", "Stress_Venustra", "Stress_Myso", "Stress_Aero", "Stress_Aero", "Stress_Ommeta", "Stress_Ommeta"};
            GBC.StarterDecks.UNDEAD_STARTER.AddRange(stressDeck);

            string[] spaceDeck = { "Space_Telescope", "Space_Telescope", "Space_Telescope", "Space_Telescope", "Space_Telescope", "Space_Alien", "Space_Alien", "Space_Alien",
                "Space_ShootingStar", "Space_ShootingStar", "Space_ShootingStar", "Space_Asteroid", "Space_Asteroid", "Space_Stargazer",
                "Space_Stargazer", "Space_Asteroid", "Space_Alien", "Space_Exomorph", "Space_Exomorph", "Space_Alien", };
            GBC.StarterDecks.TECH_STARTER.AddRange(spaceDeck);

            string[] valorDeck = { "Valor_WarBanner", "Valor_WarBanner", "Valor_WarBanner", "Valor_WarBanner", "Valor_WarBanner", "Valor_InfantryKnight", "Valor_InfantryKnight", "Valor_InfantryKnight",
                "Valor_Flagbearer", "Valor_Flagbearer", "Valor_Flagbearer", "Valor_Longbowman", "Valor_Longbowman", "Valor_Longbowman", "Valor_Longbowman", "Valor_CorsairPirate", "Valor_CorsairPirate",
                "Valor_Commandant", "Valor_Commandant", "Valor_Flagbearer"};
            GBC.StarterDecks.WIZARD_STARTER.AddRange(valorDeck);
        }
        public static void SetupSideDeckCards()
        {
            string[] sideDecks = { "Alchemy_Pylon", "Alchemy_Pylon", "Stress_Pills", "Stress_Pills", "Space_Telescope", "Space_Telescope", "Valor_WarBanner", "Valor_WarBanner" };
            GBC.StarterDecks.NATURE_STARTER.AddRange(sideDecks);
            GBC.StarterDecks.UNDEAD_STARTER.AddRange(sideDecks);
            GBC.StarterDecks.TECH_STARTER.AddRange(sideDecks);
            GBC.StarterDecks.WIZARD_STARTER.AddRange(sideDecks);
        }

        private static List<CardInfo> ArrayToInfos(string[] names)
        {
            List<CardInfo> ret = new List<CardInfo>();
            foreach (string name in names) ret.Add(CardLoader.GetCardByName(name));
            return ret;
        }

        [HarmonyPatch(typeof(PackOpeningUI), nameof(PackOpeningUI.OpenPack))]
        [HarmonyPostfix]
        public static IEnumerator SetPackImage(IEnumerator enumerator, PackOpeningUI __instance, CardTemple packType)
        {
            packImg = __instance.mainPack;
            packCards = __instance.cards;
            packRippedLeft = __instance.packLeftRipped;
            packRippedRight = __instance.packRightRipped;
            PixelSelectableCard[] presetPre = new PixelSelectableCard[5];
            packCards.CopyTo(presetPre, 0);
            preset = presetPre.ToList();

            openPack = true;
            yield return enumerator;
        }

        public static void GeneratePack()
        {
            List<CardInfo> all = CardManager.AllCardsCopy.FindAll(info => info.metaCategories.Contains(CardMetaCategory.GBCPack) && info.metaCategories.Contains(CardMetaCategory.GBCPlayable));
            List<CardInfo> good, bad;

            if (packTemple != CardTemple.NUM_TEMPLES && packDisplay == "Og")
            {
                good = all.FindAll(info => ((info.GetModPrefix() == null || !CostmaniaPlugin.NEW_TEMPLES.Any(newTemple => info.GetModPrefix().Contains(newTemple)))) && info.temple == packTemple);
            }
            else if (packTemple == CardTemple.NUM_TEMPLES)
            {
                good = all.FindAll(info => info.GetModPrefix() != null && info.GetModPrefix().Contains(packDisplay));
            }
            else throw new ArgumentException("Cannot add both a base temple and new temple");


            bad = all.FindAll(info => !good.Contains(info) && !info.metaCategories.Contains(CardMetaCategory.Rare));

            var goodCommon = good.Where(info => !info.metaCategories.Contains(CardMetaCategory.Rare)).ToList();
            var goodRare = good.Where(info => info.metaCategories.Contains(CardMetaCategory.Rare)).ToList();

            preset[0].SetInfo(goodCommon[UnityEngine.Random.Range(0, goodCommon.Count - 1)]);
            preset[1].SetInfo(goodRare[UnityEngine.Random.Range(0, goodRare.Count - 1)]);
            preset[2].SetInfo(goodCommon[UnityEngine.Random.Range(0, goodCommon.Count - 1)]);
            preset[3].SetInfo(bad[UnityEngine.Random.Range(0, bad.Count - 1)]);
            preset[3].SetInfo(bad[UnityEngine.Random.Range(0, bad.Count - 1)]);

            if (packCards[3].isActiveAndEnabled) openPack = false;
        }

        private static void AddCustomTemples()
        {
            if (UnityEngine.Random.Range(0, 2) == 1)
            {
                packTemple = CardTemple.NUM_TEMPLES;
                switch (Singleton<PackOpeningUI>.Instance.currentPackType)
                {
                    case (CardTemple.Nature):
                        packDisplay = CostmaniaPlugin.NEW_TEMPLES[0];
                        break;
                    case (CardTemple.Undead):
                        packDisplay = CostmaniaPlugin.NEW_TEMPLES[1];
                        break;
                    case (CardTemple.Tech):
                        packDisplay = CostmaniaPlugin.NEW_TEMPLES[2];
                        break;
                    case (CardTemple.Wizard):
                        packDisplay = CostmaniaPlugin.NEW_TEMPLES[3];
                        break;
                    default:
                        packDisplay = "Og";
                        break;
                }
            }
            else
            {
                packDisplay = "Og";
                switch (Singleton<PackOpeningUI>.Instance.currentPackType)
                {
                    case (CardTemple.Nature):
                        packTemple = CardTemple.Nature;
                        break;
                    case (CardTemple.Undead):
                        packTemple = CardTemple.Undead;
                        break;
                    case (CardTemple.Tech):
                        packTemple = CardTemple.Tech;
                        break;
                    case (CardTemple.Wizard):
                        packTemple = CardTemple.Wizard;
                        break;
                }
            }
        }
    }
}
