using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GBC;
namespace StressCost.Sigils
{
    public class AbilIronclad : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public static void AddIronclad()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Ironclad",
                "Any damage [creature] takes is reduced by 1.",
                typeof(AbilIronclad),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_ironclad.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_ironclad.png", typeof(StressPlugin).Assembly));
            AbilIronclad.ability = info.ability;
        }
    }
}
