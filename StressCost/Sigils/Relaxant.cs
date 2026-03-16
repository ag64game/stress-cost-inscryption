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
    public class AbilRelaxant : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            yield return PreSuccessfulTriggerSequence();

            yield return new WaitForSeconds(0.2f);
            ViewManager.Instance.SwitchToView(View.Default);
            yield return new WaitForSeconds(0.2f);
            if (Cost.StressCost.stressCounter > 0) Cost.StressCost.stressCounter -= 1;

            Console.WriteLine(Cost.StressCost.stressCounter);
            yield return new WaitForSeconds(0.3f);
            yield return LearnAbility(0.2f);
        }

        public static void AddRelaxant()
        {
            const string rulebookDescription = "When [creature] is played, it provides an energy soul to its owner.";

            AbilityInfo info = AbilityManager.New("StressSigils",
                "Relaxant",
                "[creature] lowers the Stress Counter by 1 upon resolving on the board",
                typeof(AbilRelaxant),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_relaxant.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_relaxant.png", typeof(StressPlugin).Assembly));
            AbilRelaxant.ability = info.ability;
        }
    }
}
