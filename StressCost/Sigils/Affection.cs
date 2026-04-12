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
    public class AbilAffection : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => playerTurnEnd;

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            base.Card.Anim.StrongNegationEffect();
            yield return new WaitForSeconds(0.2f);
            yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName} preformed a seducive dance!", (GBC.TextBox.Style)Card.Info.temple);

            if (Cost.StressCost.stressCounter > 0) Cost.StressCost.stressCounter -= 1;

            Console.WriteLine(Cost.StressCost.stressCounter);
            yield return new WaitForSeconds(0.3f);
            yield return LearnAbility(0.2f);
        }

        public static void AddAffection()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Affection",
                "At the end of every turn, [creature] lowers the Stress Counter by 1.",
                typeof(AbilAffection),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_affection.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_affection.png", typeof(CostmaniaPlugin).Assembly));
            AbilAffection.ability = info.ability;
        }
    }
}
