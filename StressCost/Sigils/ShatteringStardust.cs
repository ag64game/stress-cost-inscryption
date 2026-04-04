using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StressCost.Sigils
{
    public class AbilShatteringStardust : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            yield return PreSuccessfulTriggerSequence();

            yield return new WaitForSeconds(0.2f);
            Cost.StardustCost.stardustCounter -= 2;
            if (Cost.StardustCost.stardustCounter < 0) Cost.StardustCost.stardustCounter = 0;
            yield return LearnAbility(0.2f);
        }

        public static void AddShatteringStardust()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Shattering Stardust",
                "The owner of [creature] loses 2 Stardust when playing it instead of gaining 1.",
                typeof(AbilShatteringStardust),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_shatteringstardust.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_shatteringstardust.png", typeof(StressPlugin).Assembly));
            AbilShatteringStardust.ability = info.ability;
        }
    }
}
