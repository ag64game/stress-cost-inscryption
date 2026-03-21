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
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace StressCost.Cost
{
    internal class ValorCost : CustomCardCost
    {
        public override string CostName => "ValorCost";

        private static int? __maxrank = 0;
        public static int? MaxRank
        {
            get
            {
                return __maxrank;
            }

            set
            {
                __maxrank = value;
                try { StressPlugin.disValorCounter.DisplayValue((int)Cost.ValorCost.MaxRank); } catch { }
            }
        }

        public static void SetMaxRank()
        {
            var board = Singleton<BoardManager>.Instance;
            try
            {
                CardSlot maxCard = board.playerSlots.FindAll(slot => slot.Card != null)
                    .OrderByDescending(slot => slot.Card.Info.GetExtendedPropertyAsInt("ValorRank") + slot.Card.TemporaryMods.Sum(mod => mod.GetExtendedPropertyAsInt("ValorRank")))
                    .First();
                int? sumMods = 0;

                int? maxBase = maxCard.Card.Info.GetExtendedPropertyAsInt("ValorRank");
                if (maxBase == null) maxBase = 0;

                int? maxMods = maxCard.Card.TemporaryMods.Sum(mod => mod.GetExtendedPropertyAsInt("ValorRank"));
                if (maxMods == null) maxMods = 0;

                MaxRank = maxBase + maxMods;
            } catch { MaxRank = 0; }
        }

        public override bool CostSatisfied(int cardCost, PlayableCard card)
        {
            return cardCost <= MaxRank;
        }

        public override string CostUnsatisfiedHint(int cardCost, PlayableCard card)
        {
            if (SaveManager.SaveFile.IsPart2) return $"No card you have has enough [c:bG]Valor[c:] Rank to allow you to play this. Promote cards to give them [c:bG]Valor[c:].";
            else
            {
                var choice1 = $"[c:bG]{card.Info.DisplayedNameLocalized}[c:] is missing a leader of a higher [c:bG]Valor Rank[c:] to play.";
                var choice2 = $"You lack the nessecary [c:bG]Valor[c:] to play this.";
                var choice3 = $"Bonehead, your cards have not achieved enough [c:bG]Valor Ranks[c:] for [c:bG]{card.Info.DisplayedNameLocalized}[c:]";

                List<String> strings = new List<String>();
                strings.Add(choice1);
                strings.Add(choice2);
                strings.Add(choice3);

                return strings[UnityEngine.Random.Range(0, strings.Count)];
            }
        }

        public static Texture2D Texture_3D(int cardCost, CardInfo info, PlayableCard card)
        {
            return TextureHelper.GetImageAsTexture($"ValorCost_{cardCost}.png", typeof(StressPlugin).Assembly);
        }

        public static Texture2D Texture_Pixel(int cardCost, CardInfo info, PlayableCard card)
        {
            // if you want the API to handle adding stack numbers, you can instead provide a 7x8 texture like so:
            return Part2CardCostRender.CombineIconAndCount(cardCost, TextureHelper.GetImageAsTexture("pixelcost_valor.png", typeof(StressPlugin).Assembly));
        }
    }
}
