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
                try { CostmaniaPlugin.disValorCounter.DisplayValue((int)Cost.ValorCost.MaxRank); } catch { }
            }
        }

        public static void SetMaxRank()
        {
            var board = Singleton<BoardManager>.Instance;
            try
            {
                List<CardSlot> validSlots = board.playerSlots.FindAll(slot => slot.Card != null && slot.IsPlayerSlot);
                CardSlot maxCard = validSlots[0];
                int? max = 0;

                foreach (CardSlot slot in validSlots)
                {
                    int? curStand = slot.Card.Info.GetExtendedPropertyAsInt("ValorRank");
                    if (curStand == null) curStand = 0;

                    int? curMod = slot.Card.TemporaryMods.Sum(mod => mod.GetExtendedPropertyAsInt("ValorRank"));
                    if (curMod == null) curMod = 0;

                    int cur = curStand.Value + curMod.Value;

                    if (cur > max)
                    {
                        maxCard = slot;
                        max = cur;
                    }
                        
                        
                }

                MaxRank = max;
            }
            catch { MaxRank = 0; }
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
            return TextureHelper.GetImageAsTexture($"ValorCost_{cardCost}.png", typeof(CostmaniaPlugin).Assembly);
        }

        public static Texture2D Texture_Pixel(int cardCost, CardInfo info, PlayableCard card)
        {
            // if you want the API to handle adding stack numbers, you can instead provide a 7x8 texture like so:
            return Part2CardCostRender.CombineIconAndCount(cardCost, TextureHelper.GetImageAsTexture("pixelcost_valor.png", typeof(CostmaniaPlugin).Assembly));
        }
    }
}
