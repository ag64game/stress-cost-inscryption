using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StressCost.Sigils.VariableStats
{
    public class VariablestatMightierPen : VariableStatBehaviour
    {
        private static SpecialStatIcon iconType;

        public override SpecialStatIcon IconType => iconType;

        public override int[] GetStatValues()
        {
            int power = 0;
            try {   power += Singleton<BoardManager>.Instance.GetAdjacent(base.PlayableCard.Slot, true).Card.Info.GetExtendedPropertyAsInt("ValorRank").Value; } catch { Console.WriteLine("Left failed"); }
            try { power += Singleton<BoardManager>.Instance.GetAdjacent(base.PlayableCard.Slot, false).Card.Info.GetExtendedPropertyAsInt("ValorRank").Value; } catch { Console.WriteLine("Right failed"); }

            return new int[] { power, 0 };
        }

        public static void AddMightierPen()
        {
            const string rulebookDescription = "When [creature] is played, it provides an energy soul to its owner.";

            StatIconInfo info = StatIconManager.New("StressSigils",
                "Mightier Pen",
                "The value represented in this sigil is equal to the sum of the Valor Rank of the bearer's adjascent allies",
                typeof(VariablestatMightierPen));

            info.SetIcon(TextureHelper.GetImageAsTexture($"3d_mightierpen.png", typeof(StressPlugin).Assembly));
            info.SetPixelIcon(TextureHelper.GetImageAsTexture($"pixel_mightierpen.png", typeof(StressPlugin).Assembly));
            info.appliesToAttack = true;
            info.appliesToHealth = false;
            info.gbcDescription = "[creature]'s power is equal to the sum of the Valor Rank of it's adjascent allies";
            VariablestatMightierPen.iconType = info.iconType;
        }
    }
}
