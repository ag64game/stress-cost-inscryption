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


namespace StressCost
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class StressPlugin : BaseUnityPlugin
    {
        public const string GUID = "aga.costmania";
        public const string NAME = "CostMania";
        private const string VERSION = "0.0.6.7";

        public static string Directory;
        internal static ManualLogSource Log;

        Harmony harmony = new Harmony(GUID);

        internal static ConfigEntry<bool> configFairHandActive;
        internal static ConfigEntry<int> configFairHandCostStress;
        internal static ConfigEntry<int> configFairHandCostValor;
        internal static ConfigEntry<int> configFairHandCostStardust;

        public static PixelNumeral disStressCounter;
        public static PixelNumeral disValorCounter;
        public static PixelNumeral disStardustCounter;
        public static AlchemyCounter disAlchemyCounter;

        public static readonly string[] NEW_TEMPLES = ["Alchemy", "Stress", "Space", "Valor"];

        private static string packDisplay = "Og";
        private static CardTemple packTemple = CardTemple.NUM_TEMPLES;
        private static SpriteRenderer packImg, packRippedRight, packRippedLeft;
        private static List<PixelSelectableCard> packCards, preset;
        private static bool openPack;

        private void Awake()
        {
            Log = base.Logger;
            Directory = base.Info.Location.Replace("StressCost.dll", "");
            harmony.PatchAll();
            configFairHandActive = base.Config.Bind<bool>("Fair Hand", "Active", true, "Should this mod post-fix patch fair hand to include the new costs");
            configFairHandCostStress = base.Config.Bind<int>("Fair Hand", "Stress Cost", 3, "The value in which the card should not show up in fair hand.");
            configFairHandCostValor = base.Config.Bind<int>("Fair Hand", "Valor Cost", 1, "The value in which the card should not show up in fair hand.");
            configFairHandCostStardust = base.Config.Bind<int>("Fair Hand", "Stardust Cost", 1, "The value in which the card should not show up in fair hand.");

            AddCost();
            AddSigils();

            harmony.PatchAll(typeof(StressPlugin));
        }
        private void Update()
        {
            Cost.ValorCost.SetMaxRank();
        }

        private void OnGUI()
        {
            try { foreach (GameObject card in Array.FindAll(FindObjectsOfType<GameObject>(), obj => obj.name.Contains("Card ("))) UpdateValorRank(card); } catch { }


            if (openPack)
            {
                AddCustomTemples();
                GeneratePack();
                packCards = preset;

                if (packDisplay != "Og" && packImg.isVisible)
                {
                    Texture2D texture = TextureHelper.GetImageAsTexture($"card_pack_{packDisplay.ToLower()}.png", typeof(StressPlugin).Assembly);
                    packImg.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector3(0.5f, 0.5f, 0.1f));

                    Texture2D rippedLeft = TextureHelper.GetImageAsTexture($"ripped_card_left_{packDisplay.ToLower()}.png", typeof(StressPlugin).Assembly);
                    packRippedLeft.sprite = Sprite.Create(rippedLeft, new Rect(0, 0, rippedLeft.width, rippedLeft.height), new Vector3(0.5f, 0.5f, 0.1f));

                    Texture2D rippedRight = TextureHelper.GetImageAsTexture($"ripped_card_right_{packDisplay.ToLower()}.png", typeof(StressPlugin).Assembly);
                    packRippedRight.sprite = Sprite.Create(rippedRight, new Rect(0, 0, rippedRight.width, rippedRight.height), new Vector3(0.5f, 0.5f, 0.1f));
                }


                openPack = false;
            }

            try
            {
                if (packDisplay != "Og" && packImg.isVisible)
                {
                    foreach (GameObject text in Array.FindAll(FindObjectsOfType<GameObject>(), obj => obj.name == "DialogueHandler"))
                        if (packDisplay[0] == 'A' || packDisplay[0] == 'I' || packDisplay[0] == 'E' || packDisplay[0] == 'O') text.FindChild("TextBox").FindChild("Box").FindChild("PixelText").GetComponent<PixelText>().SetText($"You recieved an {packDisplay} card pack!");
                        else if (packDisplay == "Stress") text.FindChild("TextBox").FindChild("Box").FindChild("PixelText").GetComponent<PixelText>().SetText($"You recieved a Paranoia card pack!");
                        else text.FindChild("TextBox").FindChild("Box").FindChild("PixelText").GetComponent<PixelText>().SetText($"You recieved a {packDisplay} card pack!");
                }
            }
            catch { }
        }
        private static void UpdateValorRank(GameObject card)
        {
            GameObject statsSect = card.FindChild("Base").FindChild("PixelSnap").FindChild("CardElements").FindChild("PixelCardStats");
            GameObject rankText = statsSect.FindChild("ValorRank");
            CardInfo info;

            try { info = card.GetComponent<PixelSelectableCard>().Info; }
            catch { info = card.GetComponent<PixelPlayableCard>().Info; }

            int? baseVal = info.GetExtendedPropertyAsInt("ValorRank");
            if (baseVal == null) baseVal = 0;

            PixelText statText = rankText.GetComponent<PixelText>();
            try
            {
                int? modVal = info.GetPlayableCard().temporaryMods.Sum(mod => mod.GetExtendedPropertyAsInt("ValorRank"));
                if (modVal == null) modVal = 0;

                if (baseVal + modVal > 0) statText.SetText(Convert.ToString(baseVal + modVal));
                else statText.SetText("");
            }
            catch
            {
                if (baseVal > 0) statText.SetText(Convert.ToString(baseVal));
                else statText.SetText("");
            }
        }

        public static void AddCost()
        {
            FullCardCost stressCost = Register(GUID, "StressCost", typeof(Cost.StressCost), Cost.StressCost.Texture_3D, Cost.StressCost.Texture_Pixel);
            stressCost.SetCostTier(Cost.CostTier.CostTierS);
            stressCost.ResourceType = (ResourceType)42;

            FullCardCost valorCost = Register(GUID, "ValorCost", typeof(Cost.ValorCost), Cost.ValorCost.Texture_3D, Cost.ValorCost.Texture_Pixel);
            valorCost.SetCostTier(Cost.CostTier.CostTierV);
            valorCost.ResourceType = (ResourceType)42;

            FullCardCost fleshCost = Register(GUID, "FleshCost", typeof(Cost.FleshCost), Cost.FleshCost.Texture_3D, Cost.FleshCost.Texture_Pixel);
            fleshCost.SetCostTier(Cost.CostTier.CostTierA);
            fleshCost.ResourceType = (ResourceType)42;

            FullCardCost metalCost = Register(GUID, "MetalCost", typeof(Cost.MetalCost), Cost.MetalCost.Texture_3D, Cost.MetalCost.Texture_Pixel);
            metalCost.SetCostTier(Cost.CostTier.CostTierA);
            metalCost.ResourceType = (ResourceType)42;

            FullCardCost elixirCost = Register(GUID, "ElixirCost", typeof(Cost.ElixirCost), Cost.ElixirCost.Texture_3D, Cost.ElixirCost.Texture_Pixel);
            elixirCost.SetCostTier(Cost.CostTier.CostTierA);
            elixirCost.ResourceType = (ResourceType)42;

            FullCardCost stardustCost = Register(GUID, "StardustCost", typeof(Cost.StardustCost), Cost.StardustCost.Texture_3D, Cost.StardustCost.Texture_Pixel);
            stardustCost.SetCostTier(Cost.CostTier.CostTierS);
            stardustCost.ResourceType = (ResourceType)42;

            if (configFairHandActive.Value)
            {
                stressCost.SetCanBePlayedByTurn2WithHand(Cost.FairHandStress.CanBePlayed);
                valorCost.SetCanBePlayedByTurn2WithHand(Cost.FairHandValor.CanBePlayed);
                fleshCost.SetCanBePlayedByTurn2WithHand(Cost.FairHandFlesh.CanBePlayed);
                metalCost.SetCanBePlayedByTurn2WithHand(Cost.FairHandMetal.CanBePlayed);
                elixirCost.SetCanBePlayedByTurn2WithHand(Cost.FairHandElixir.CanBePlayed);
                stardustCost.SetCanBePlayedByTurn2WithHand(Cost.FairHandStardust.CanBePlayed);
            }
        }

        public static void AddSigils()
        {
            AbilRelaxant.AddRelaxant();
            AbilAffection.AddAffection();
            AbilEnrage.AddEnrage();
            AbilLiftoff.AddLiftoff();
            AbilGemAbsorber.AddGemAbsorber();
            AbilFrontliner.AddFrontliner();
            AbilBloodGuzzler.AddBloodGuzzler();
            AbilIronclad.AddIronclad();
            AbilFearmonger.AddFearmonger();
            AbilFirstStrike.AddFirstStrike();
            AbilWatchman.AddWatchman();
            AbilEldritchPower.AddEldritchPower();
            AbilSigilEater.AddSigilEater();
            AbilStarbringer.AddStrbringer();
            AbilHealingAura.AddHealingAura();
            AbilWarper.AddWarper();
            AbilShatteringStardust.AddShatteringStardust();
            AbilArmyBuilder.AddArmyBuilder();
            AbilRandomAbility.AddRandomAbility();
            AbilAfterimage.AddAfterimage();
            AbilRentrance.AddRentrance();

            VariablestatMightierPen.AddMightierPen();
            VariablestatDeathToll.AddDeathToll();
        }

        [HarmonyPatch(typeof(BoardManager), nameof(BoardManager.ResolveCardOnBoard))]
        [HarmonyPostfix]
        public static IEnumerator PayCosts(IEnumerator enumerator, BoardManager __instance, PlayableCard card, CardSlot slot)
        {
            if (slot.IsPlayerSlot)
            {
                if (card.Info.GetExtendedPropertyAsInt("StressCost") > 0)
                {
                    Cost.StressCost.stressCounter += card.Info.GetExtendedPropertyAsInt("StressCost").Value;
                    foreach (CardSlot fearSlot in __instance.AllSlots.FindAll(slot => slot.Card != null && !slot.Card.LacksAllAbilities())) yield return OnStressCounterChange(fearSlot.Card, enumerator);
                }

                if (card.Info.GetExtendedPropertyAsInt("FleshCost") > 0) disAlchemyCounter.PayIfPossible(AlchemyValue.Flesh, card.Info.GetExtendedPropertyAsInt("FleshCost").Value);
                if (card.Info.GetExtendedPropertyAsInt("MetalCost") > 0) disAlchemyCounter.PayIfPossible(AlchemyValue.Metal, card.Info.GetExtendedPropertyAsInt("MetalCost").Value);
                if (card.Info.GetExtendedPropertyAsInt("ElixirCost") > 0) disAlchemyCounter.PayIfPossible(AlchemyValue.Elixir, card.Info.GetExtendedPropertyAsInt("ElixirCost").Value);

                StardustCost.stardustCounter++;
            }

            yield return enumerator;
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

        [HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        public class PromotionPhase
        {
            public static IEnumerator Postfix(IEnumerator enumerator, TurnManager __instance, bool playerUpkeep)
            {
                if (playerUpkeep && __instance.TurnNumber > 1) yield return DoPromotionPhase();
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

                UpdateValorRank(slot.Card.gameObject);
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

        [HarmonyPostfix, HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        public static IEnumerator IncrementAlchemy(IEnumerator enumerator, TurnManager __instance, bool playerUpkeep)
        {
            if (playerUpkeep)
            {
                disAlchemyCounter.AddDies();


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

                    if (available.Count > 0) disAlchemyCounter.RollDies(specific: available[UnityEngine.Random.Range(0, available.Count)]);
                    else disAlchemyCounter.RollDies();
                } else disAlchemyCounter.RollDies();
            }
            yield return enumerator;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TurnManager), nameof(TurnManager.DoUpkeepPhase))]
        public static IEnumerator ResetStardust(IEnumerator enumerator, TurnManager __instance, bool playerUpkeep)
        {
            StardustCost.stardustCounter = 0;
            yield return enumerator;
        }


        [HarmonyPostfix, HarmonyPatch(typeof(Card), nameof(Card.RenderCard))]
        public static void RenderValorRank(Card __instance)
        {
            GameObject statsSect = __instance.gameObject.FindChild("Base").FindChild("PixelSnap").FindChild("CardElements").FindChild("PixelCardStats");
            GameObject rankText = statsSect.FindChild("ValorRank");
            CardInfo info;

            try { info = __instance.gameObject.GetComponent<PixelSelectableCard>().Info; }
            catch { info = __instance.gameObject.GetComponent<PixelPlayableCard>().Info; }

            int? baseVal = info.GetExtendedPropertyAsInt("ValorRank");
            if (baseVal == null) baseVal = 0;

            if (rankText == null)
            {
                GameObject attack = statsSect.FindChild("Attack");
                rankText = Instantiate(attack);
                rankText.name = "ValorRank";
                rankText.transform.position = new Vector3(attack.transform.position.x + 0.166f, attack.transform.position.y + 0.005f, attack.transform.position.z);
                rankText.transform.SetParent(statsSect.transform);

                PixelText stat = rankText.GetComponent<PixelText>();
                stat.SetColor(Color.gray);

                if (baseVal > 0) stat.SetText(Convert.ToString(baseVal));
                else stat.SetText("");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        public static IEnumerator SetupCosts(IEnumerator enumerator, TurnManager __instance, EncounterData encounterData)
        {
            Cost.StressCost.stressCounter = 0;
            VariablestatDeathToll.killCount = 0;
            try
            {
                RenderStressCounter();
                RenderValorCounter();
                RenderStardustCounter();
                RenderAlchemyCollection();
            } catch { }

            return enumerator;
        }

        public static void RenderStressCounter()
        {
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
                    new Vector2(13.7f, -6.5f)
                    );

                GameObject numsBorder = stressDis.gameObject.FindChild("PixelBorderNumeral");
                disStressCounter = numsBorder.GetComponent<PixelNumeral>();
                numsBorder.transform.position = new Vector3(-2.08f, 0.94f, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void RenderValorCounter()
        {
            try
            {
                GameObject valor = Instantiate<GameObject>(PixelResourcesManager.Instance.gameObject.transform.Find("Bones").gameObject);
                valor.SetActive(true);
                valor.transform.SetParent(PixelResourcesManager.Instance.gameObject.transform);
                valor.layer = 31;
                valor.name = "Valor";

                GameObject valorDis = valor.gameObject.FindChild("BoneIcon");
                valorDis.name = "MaxValorRank";

                Texture2D texture = TextureHelper.GetImageAsTexture($"displaycost_valor.png", typeof(StressPlugin).Assembly);

                valorDis.GetComponent<SpriteRenderer>().sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(13.7f, -5.5f)
                    );

                GameObject numsBorder = valorDis.gameObject.FindChild("PixelBorderNumeral");
                disValorCounter = numsBorder.GetComponent<PixelNumeral>();
                numsBorder.transform.position = new Vector3(-2.08f, 0.80f, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void RenderStardustCounter()
        {
            try
            {
                GameObject stardust = Instantiate<GameObject>(PixelResourcesManager.Instance.gameObject.transform.Find("Bones").gameObject);
                stardust.SetActive(true);
                stardust.transform.SetParent(PixelResourcesManager.Instance.gameObject.transform);
                stardust.layer = 31;
                stardust.name = "Stardust";

                GameObject stardustDis = stardust.gameObject.FindChild("BoneIcon");
                stardustDis.name = "StardustCounter";

                Texture2D texture = TextureHelper.GetImageAsTexture($"displaycost_stardust.png", typeof(StressPlugin).Assembly);

                stardustDis.GetComponent<SpriteRenderer>().sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(13.7f, -6.4f)
                    );

                GameObject numsBorder = stardustDis.gameObject.FindChild("PixelBorderNumeral");
                disStardustCounter = numsBorder.GetComponent<PixelNumeral>();
                numsBorder.transform.position = new Vector3(-2.08f, 0.67f, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void RenderAlchemyCollection()
        {
            GameObject aga = new GameObject("Alchemy");
            disAlchemyCounter = aga.AddComponent<AlchemyCounter>();

            aga.transform.SetParent(PixelResourcesManager.Instance.gameObject.transform);
            aga.SetActive(true);
        }







        [HarmonyPatch(typeof(Card), nameof(Card.RenderCard))]
        [HarmonyPostfix]
        public static void AddRareDecals(Card __instance)
        {
            try
            {
                Texture2D textureStress = TextureHelper.GetImageAsTexture($"pixel_rare_frame_stress.png", typeof(StressPlugin).Assembly);
                Sprite rareDecalStress = Sprite.Create(textureStress, new Rect(0, 0, textureStress.width, textureStress.height), new Vector2(0.5f, 0.487f));

                Texture2D textureValor = TextureHelper.GetImageAsTexture($"pixel_rare_frame_valor.png", typeof(StressPlugin).Assembly);
                Sprite rareDecalValor = Sprite.Create(textureValor, new Rect(0, 0, textureValor.width, textureValor.height), new Vector2(0.5f, 0.487f));

                Texture2D textureAlchemy = TextureHelper.GetImageAsTexture($"pixel_rare_frame_alchemy.png", typeof(StressPlugin).Assembly);
                Sprite rareDecalAlchemy = Sprite.Create(textureAlchemy, new Rect(0, 0, textureAlchemy.width, textureAlchemy.height), new Vector2(0.5f, 0.487f));

                Texture2D textureSpace = TextureHelper.GetImageAsTexture($"pixel_rare_frame_space.png", typeof(StressPlugin).Assembly);
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


        [HarmonyPatch(typeof(CollectionUI), nameof(CollectionUI.Start))]
        public class AddStressTab
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
                
                Texture2D image = TextureHelper.GetImageAsTexture($"temple_{name.ToLower()}.png", typeof(StressPlugin).Assembly);
                gameObject.gameObject.transform.Find("Icon").gameObject.GetComponent<SpriteRenderer>().sprite = Sprite.Create(image, new Rect(0f, 0f, (float)image.width, (float)image.height), new Vector2(0.5f, 0.5f));
            }
        }

        [HarmonyPatch(typeof(CollectionUI), "CreatePages")]
        [HarmonyPostfix]
        public static void SortCards(ref CollectionUI __instance, ref List<List<CardInfo>> __result, ref List<CardInfo> cards)
        {
            //Take the amount of buttons for array
            int[] pageTrackers;
            if(BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("mrfantastik.inscryption.infact2")) 
                pageTrackers = new int[9];
            else
                pageTrackers = new int[8];

            Console.WriteLine(pageTrackers.Length);

            List<List<CardInfo>> res = new List<List<CardInfo>>();
            int index = 0;

            foreach (CardTemple temple in Enum.GetValues(typeof(CardTemple))) if (temple != CardTemple.NUM_TEMPLES)
            {
                //Get all the cards of the temple who aren't from the new Temples. Inject them to __results
                List<CardInfo> list = cards.FindAll(info => info.temple == temple && (info.GetModPrefix() == null || !NEW_TEMPLES.Any(newTemple => info.GetModPrefix().Contains(newTemple))));
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
            stressCards = stressCards.OrderBy(info =>  (info.metaCategories.Contains(CardMetaCategory.Rare) ? 1 : 100))
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
                res.Add(new List<CardInfo>());

                pageTrackers[8] = index;
                index++;
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

        [HarmonyPatch(typeof(ActivatedAbilityBehaviour), nameof(ActivatedAbilityBehaviour.OnActivatedAbility))]
        [HarmonyPostfix]
        public static IEnumerator ActivatedAddStress(IEnumerator enumerator, ActivatedAbilityBehaviour __instance)
        {
            if (__instance is StressActivatedAbility)
            {
                Cost.StressCost.stressCounter += (__instance as StressActivatedAbility).StressCost;
                if((__instance as StressActivatedAbility).StressCost > 0) foreach (CardSlot slot in Singleton<BoardManager>.Instance.AllSlots.FindAll(slot => slot.Card != null)) OnStressCounterChange(slot.Card, enumerator);
            }

            yield return enumerator;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.TakeDamage))]
        [HarmonyPrefix]
        public static void SetupDefenceAbilities(ref PlayableCard __instance, ref int damage, ref PlayableCard attacker) 
        { 
            if (__instance.HasAbility(AbilIronclad.ability) && damage > 0)
            {
                damage--;
                __instance.Anim.StrongNegationEffect();
            }
        }

        public static IEnumerator OnStressCounterChange(PlayableCard card, IEnumerator enumerator)
        {
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
           
            yield return enumerator;
        }

        [HarmonyPatch(typeof(PlayableCard), nameof(PlayableCard.Die))]
        [HarmonyPostfix]
        public static IEnumerator ThanatoKillCount(IEnumerator enumerator, PlayableCard __instance, bool wasSacrifice)
        {
            if (__instance.OpponentCard) VariablestatDeathToll.killCount++;
            yield return enumerator;
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
                good = all.FindAll(info => ((info.GetModPrefix() == null || !NEW_TEMPLES.Any(newTemple => info.GetModPrefix().Contains(newTemple)))) && info.temple == packTemple);
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
                        packDisplay = NEW_TEMPLES[0];
                        break;
                    case (CardTemple.Undead):
                        packDisplay = NEW_TEMPLES[1];
                        break;
                    case (CardTemple.Tech):
                        packDisplay = NEW_TEMPLES[2];
                        break;
                    case (CardTemple.Wizard):
                        packDisplay = NEW_TEMPLES[3];
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