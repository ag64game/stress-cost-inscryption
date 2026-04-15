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
    internal class CostGraphicPatches
    {
        public static PixelNumeral disStressCounter;
        public static PixelNumeral disValorCounter;
        public static PixelNumeral disStardustCounter;
        public static AlchemyCounter disAlchemyCounter;

        public static void Update()
        {
            if (SaveManager.SaveFile.IsPart2)
            {
                foreach (GameObject card in Array.FindAll(CostmaniaPlugin.FindObjectsOfType<GameObject>(), obj => obj.name.Contains("Card (")))
                    try { UpdateValorRank(card.gameObject); }
                    catch { }

                //try { UpdateTerrainDesc(); } catch { }
            }
        }

        public static void UpdateValorRank(GameObject card)
        {
            if (SaveManager.SaveFile.IsPart2)
            {
                GameObject statsSect = card.FindChild("Base").FindChild("PixelSnap").FindChild("CardElements").FindChild("PixelCardStats");
                GameObject rankText = statsSect.FindChild("ValorRank");
                PixelText statText = rankText.GetComponent<PixelText>();
                CardInfo info;
                int valorRank = 0;

                if (card.GetComponent<PixelSelectableCard>() != null)
                {
                    PixelSelectableCard selectableCardGBC = card.GetComponent<PixelSelectableCard>();
                    valorRank = selectableCardGBC.ValorRank();
                }
                else if (card.GetComponent<PixelPlayableCard>() != null)
                {
                    PlayableCard playableCard = card.GetComponent<PixelPlayableCard>();
                    valorRank = playableCard.ValorRank();
                }

                if (valorRank > 0) statText.SetText(Convert.ToString(valorRank));
                else statText.SetText("");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Card), nameof(Card.RenderCard))]
        public static void RenderValorRank(Card __instance)
        {
            if (CostmaniaPlugin.config3DValor.Value || SaveManager.SaveFile.IsPart2)
            {
                GameObject statsSect = __instance.gameObject.FindChild("Base").FindChild("PixelSnap").FindChild("CardElements").FindChild("PixelCardStats");
                GameObject rankText = statsSect.FindChild("ValorRank");
                CardInfo info;

                try { info = __instance.gameObject.GetComponent<PixelSelectableCard>().Info; }
                catch { info = __instance.gameObject.GetComponent<PixelPlayableCard>().Info; }

                int baseVal = info.ValorRank();

                if (rankText == null)
                {
                    GameObject attack = statsSect.FindChild("Attack");
                    rankText = MonoBehaviour.Instantiate(attack);
                    rankText.name = "ValorRank";
                    rankText.transform.position = new Vector3(attack.transform.position.x + 0.166f, attack.transform.position.y + 0.005f, attack.transform.position.z);
                    rankText.transform.SetParent(statsSect.transform);

                    PixelText stat = rankText.GetComponent<PixelText>();
                    stat.SetColor(Color.gray);

                    if (baseVal > 0) stat.SetText(Convert.ToString(baseVal));
                    else stat.SetText("");
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(TurnManager), nameof(TurnManager.SetupPhase))]
        public static IEnumerator SetupCostDisplays(IEnumerator enumerator, TurnManager __instance, EncounterData encounterData)
        {
            if (CostmaniaPlugin.config3DStress.Value || SaveManager.SaveFile.IsPart2) Cost.StressCost.stressCounter = 0;
            VariablestatDeathToll.killCount = 0;
            if (SaveManager.SaveFile.IsPart2)
            {
                try
                {
                    RenderStressCounter();
                    RenderValorCounter();
                    RenderStardustCounter();
                    RenderAlchemyCollection();
                }
                catch { }

                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("julianperge.inscryption.act2.increaseCardSlots"))
                {
                    PixelResourcesManager.Instance.gameObject.transform.Find("Bones").gameObject.transform.position = new Vector3(-1.945f, 0.96f, 0);
                    PixelResourcesManager.Instance.gameObject.transform.Find("Gems").gameObject.transform.position = new Vector3(-1.9135f, 0.82f, 0);
                }
            }

            return enumerator;
        }

        public static void RenderStressCounter()
        {
            try
            {
                GameObject stress = MonoBehaviour.Instantiate<GameObject>(PixelResourcesManager.Instance.gameObject.transform.Find("Bones").gameObject);
                stress.SetActive(true);
                stress.transform.SetParent(PixelResourcesManager.Instance.gameObject.transform);
                stress.layer = 31;
                stress.name = "Stress";

                GameObject stressDis = stress.gameObject.FindChild("BoneIcon");
                stressDis.name = "StressCounter";

                Texture2D texture = TextureHelper.GetImageAsTexture($"displaycost_stress.png", typeof(CostmaniaPlugin).Assembly);

                if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("julianperge.inscryption.act2.increaseCardSlots")) stressDis.GetComponent<SpriteRenderer>().sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(13.7f, -6.5f)
                    );
                else stressDis.GetComponent<SpriteRenderer>().sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(7.24f, -7.8f)
                    );

                GameObject numsBorder = stressDis.gameObject.FindChild("PixelBorderNumeral");
                disStressCounter = numsBorder.GetComponent<PixelNumeral>();
                if(!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("julianperge.inscryption.act2.increaseCardSlots")) numsBorder.transform.position = new Vector3(-2.08f, 0.94f, 0);
                else numsBorder.transform.position = new Vector3(-1.44f, 1.132f, 0);
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
                GameObject valor = MonoBehaviour.Instantiate<GameObject>(PixelResourcesManager.Instance.gameObject.transform.Find("Bones").gameObject);
                valor.SetActive(true);
                valor.transform.SetParent(PixelResourcesManager.Instance.gameObject.transform);
                valor.layer = 31;
                valor.name = "Valor";

                GameObject valorDis = valor.gameObject.FindChild("BoneIcon");
                valorDis.name = "MaxValorRank";

                Texture2D texture = TextureHelper.GetImageAsTexture($"displaycost_valor.png", typeof(CostmaniaPlugin).Assembly);

                if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("julianperge.inscryption.act2.increaseCardSlots")) valorDis.GetComponent<SpriteRenderer>().sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(13.7f, -5.5f)
                    );
                else valorDis.GetComponent<SpriteRenderer>().sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(14.2f, -6.42f)
                    );

                GameObject numsBorder = valorDis.gameObject.FindChild("PixelBorderNumeral");
                disValorCounter = numsBorder.GetComponent<PixelNumeral>();
                if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("julianperge.inscryption.act2.increaseCardSlots")) numsBorder.transform.position = new Vector3(-2.08f, 0.80f, 0);
                else numsBorder.transform.position = new Vector3(-2.13f, 0.932f, 0);
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
                GameObject stardust = MonoBehaviour.Instantiate<GameObject>(PixelResourcesManager.Instance.gameObject.transform.Find("Bones").gameObject);
                stardust.SetActive(true);
                stardust.transform.SetParent(PixelResourcesManager.Instance.gameObject.transform);
                stardust.layer = 31;
                stardust.name = "Stardust";

                GameObject stardustDis = stardust.gameObject.FindChild("BoneIcon");
                stardustDis.name = "StardustCounter";

                Texture2D texture = TextureHelper.GetImageAsTexture($"displaycost_stardust.png", typeof(CostmaniaPlugin).Assembly);

                if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("julianperge.inscryption.act2.increaseCardSlots")) stardustDis.GetComponent<SpriteRenderer>().sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(13.7f, -6.4f)
                    );
                else stardustDis.GetComponent<SpriteRenderer>().sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(14.2f, -7.55f)
                    );

                GameObject numsBorder = stardustDis.gameObject.FindChild("PixelBorderNumeral");
                disStardustCounter = numsBorder.GetComponent<PixelNumeral>();
                if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("julianperge.inscryption.act2.increaseCardSlots"))  numsBorder.transform.position = new Vector3(-2.08f, 0.67f, 0);
                else numsBorder.transform.position = new Vector3(-2.13f, 0.792f, 0);
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


        private static string last = "";
        private static void UpdateTerrainDesc()
        {
            var scrollArea = Singleton<PixelScrollArea>.Instance;
            string desc = "";
            foreach (string line in scrollArea.fullLines) desc += line;

            if (last == null || last != desc)
            {
                scrollArea.SetText(desc.Replace("CAN'T BE SACRIFICED.", "CAN'T BE SACRIFICED OR PROMOTED."));
                last = desc;
            }

        }
    }
}
