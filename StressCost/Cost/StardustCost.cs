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
    internal class StardustCost : CustomCardCost
    {
        private static int __prev = 0;
        private static int __counter = 0;
        public static int stardustCounter
        {
            get
            {
                return __counter;
            }

            set
            {
                //__prev = __counter;
                __counter = value;
                try { StressPlugin.disStardustCounter.DisplayValue(Cost.StardustCost.stardustCounter); } catch { }
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

        public override string CostName => "StardustCost";

        public override bool CostSatisfied(int cardCost, PlayableCard card) => cardCost <= stardustCounter;

        public override string CostUnsatisfiedHint(int cardCost, PlayableCard card)
        {
            if (SaveManager.SaveFile.IsPart2) return $"You do not possess enough [c:blue]Stardust[c:]. Play more cards to gain [c:blue]Stardust[c:].";
            else
            {
                var choice1 = $"[c:blue]{card.Info.DisplayedNameLocalized}[c:] needs more [c:blue]Stardust[c:].";
                var choice2 = $"Didn't play enough cards for the nessecary [c:blue]Stardust[c:], challenger.";
                var choice3 = $"Don't you read? You need to play more cards before [c:blue]{card.Info.DisplayedNameLocalized}'s Stardust[c:] cost is satisfied.";

                List<String> strings = new List<String>();
                strings.Add(choice1);
                strings.Add(choice2);
                strings.Add(choice3);

                return strings[UnityEngine.Random.Range(0, strings.Count)];
            }
        }

        public static Texture2D Texture_3D(int cardCost, CardInfo info, PlayableCard card)
        {
            return TextureHelper.GetImageAsTexture($"StardustCost_{cardCost}.png", typeof(StressPlugin).Assembly);
        }

        public static Texture2D Texture_Pixel(int cardCost, CardInfo info, PlayableCard card)
        {
            // if you want the API to handle adding stack numbers, you can instead provide a 7x8 texture like so:
            return Part2CardCostRender.CombineIconAndCount(cardCost, TextureHelper.GetImageAsTexture("pixelcost_stardust.png", typeof(StressPlugin).Assembly));
        }
    }


}
