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
    public class AbilStarbringer : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            Cost.StardustCost.stardustCounter++;
            yield return LearnAbility(0.2f);
            yield return true;
        }

        public static void AddStrbringer()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Starbringer",
                "While [creature] is on the board, it's owner starts every turn with an extra Stardust.",
                typeof(AbilStarbringer),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_starbringer.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_starbringer.png", typeof(StressPlugin).Assembly));
            AbilStarbringer.ability = info.ability;
        }
    }
}
