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
                //__prev = __counter;
                __counter = value;
                try { StressPlugin.disStressCounter.DisplayValue(Cost.StressCost.stressCounter); } catch { }
                //DisplaySequentially();
            }
        }
        private static IEnumerator DisplaySequentially()
        {
            int dif = __prev / __counter;
            float waitTime = 0.25f / dif;
            int factor = __prev > __counter ? -1 : 1;
            int curVal = factor;

            try
            {
                for (int i = 1; i < dif; i++)
                {
                    StressPlugin.disStressCounter.DisplayValue(__prev + curVal);
                    curVal+= factor;

                    yield return new WaitForSeconds(waitTime);
                }
            }
            finally { }


            yield return true;
        }

        public override string CostName => "StressCost";

        public override bool CostSatisfied(int cardCost, PlayableCard card)
        {
            return true;
        }

        // this is called after a card with this cost resolves on the board
        // if your cost spends a resource, this is where you'd put that logic
        public override IEnumerator OnPlayed(int cardCost, PlayableCard card)
        {
            stressCounter += cardCost;
            Console.WriteLine(stressCounter);

            yield return true;
        }

        public static Texture2D Texture_3D(int cardCost, CardInfo info, PlayableCard card)
        {
            return TextureHelper.GetImageAsTexture($"StressCost_{cardCost}.png", typeof(StressPlugin).Assembly);
        }

        public static Texture2D Texture_Pixel(int cardCost, CardInfo info, PlayableCard card)
        {
            // if you want the API to handle adding stack numbers, you can instead provide a 7x8 texture like so:
            return Part2CardCostRender.CombineIconAndCount(cardCost, TextureHelper.GetImageAsTexture("pixelcost_stress.png", typeof(StressPlugin).Assembly));
        }
    }


}
