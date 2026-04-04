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
    public class AbilFearmonger : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public static void AddFearmonger()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Fearmonger",
                "[creature] strikes the opposing card every time the Stress Counter goes up.",
                typeof(AbilIronclad),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_fearmonger.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_fearmonger.png", typeof(StressPlugin).Assembly));
            AbilFearmonger.ability = info.ability;
        }
    }
}
