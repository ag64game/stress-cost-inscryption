using DiskCardGame;
using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.CardCosts;
using InscryptionAPI.Helpers;
using InscryptionCommunityPatch.Card;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using static InscryptionAPI.CardCosts.CardCostManager;

namespace StressCost.Cost
{
    internal class StressCost : CustomCardCost
    {
        private static int __prev = 0;
        private static int __counter = 0;
        public static int stressCounter
        {
            get
            {
                return __counter;
            }

            set
            {
                __counter = value;
                try { Patches.CostGraphicPatches.disStressCounter.DisplayValue(Cost.StressCost.stressCounter); } catch { }
            }
        }

        public static IEnumerator IncrementCounter(int amount)
        {
            float time = 0.2f / amount;

            for (int i = 0; i < amount; i++)
            {
                stressCounter++;

                AudioController.Instance.PlaySound2D("plainBlip6", volume: 0.6f);
                yield return new WaitForSeconds(time);
                time += 0.025f;
            }
        }

        private static int secondPlayer = 0;

        public override string CostName => "StressCost";

        public override bool CostSatisfied(int cardCost, PlayableCard card)
        {
            return true;
        }

        public static void SwitchPlayer() => (secondPlayer, stressCounter) = (stressCounter, secondPlayer);
        public static void ResetPlayerTwo() => secondPlayer = 0;

        public static Texture2D Texture_3D(int cardCost, CardInfo info, PlayableCard card)
        {
            return TextureHelper.GetImageAsTexture($"StressCost_{cardCost}.png", typeof(CostmaniaPlugin).Assembly);
        }

        public static Texture2D Texture_Pixel(int cardCost, CardInfo info, PlayableCard card)
        {
            // if you want the API to handle adding stack numbers, you can instead provide a 7x8 texture like so:
            return Part2CardCostRender.CombineIconAndCount(cardCost, TextureHelper.GetImageAsTexture("pixelcost_stress.png", typeof(CostmaniaPlugin).Assembly));
        }
    }

    public static class CardStressExpansion
    {
        public static int StressCost(this CardInfo card)
        {
            int? baseVal = card.GetExtendedPropertyAsInt("StressCost");
            if (baseVal == null) baseVal = 0;

            return baseVal.Value;
        }
    }

    public class UIShake : MonoBehaviour
    {
        public IEnumerator Shake(float duration, float magnitude)
        {
            Vector3 originalPos = transform.localPosition;
            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                float movePercent = 1 - (elapsed / duration);
                float x = UnityEngine.Random.Range(-movePercent, movePercent) * magnitude;
                float y = UnityEngine.Random.Range(-movePercent, movePercent) * magnitude;

                transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = originalPos;
        }
    }
}
