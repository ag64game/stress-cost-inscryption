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
    public class VariablestatDeathToll : VariableStatBehaviour
    {
        private static SpecialStatIcon iconType;

        public override SpecialStatIcon IconType => iconType;

        public static int killCount = 0;

        public override int[] GetStatValues()
        {
            return new int[] { killCount, 0 };
        }

        public static void AddDeathToll()
        {
            StatIconInfo info = StatIconManager.New("StressSigils",
                "Death Toll",
                "The value represented in this sigil is equal to the amount of opposing cards that have perished.",
                typeof(VariablestatDeathToll));

            info.SetIcon(TextureHelper.GetImageAsTexture($"3d_deathtoll.png", typeof(StressPlugin).Assembly));
            info.SetPixelIcon(TextureHelper.GetImageAsTexture($"pixel_deathtoll.png", typeof(StressPlugin).Assembly));
            info.appliesToAttack = true;
            info.appliesToHealth = false;
            info.gbcDescription = "[creature]'s power is equal to the amount of opposing cards that have perished.";
            VariablestatDeathToll.iconType = info.iconType;
        }
    }
}
