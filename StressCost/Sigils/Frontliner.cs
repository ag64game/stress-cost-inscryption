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
    public class AbilFrontliner : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => playerTurnEnd;

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            base.Card.Anim.StrongNegationEffect();
            yield return new WaitForSeconds(0.2f);
            yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName}'s bravery amplified it's morale", TextBox.Style.Neutral);

            var mod = new CardModificationInfo(0, 0);
            mod.SetExtendedProperty("ValorRank", 1);

            base.Card.AddTemporaryMod(mod);

            yield return new WaitForSeconds(0.2f);
            yield return LearnAbility(0.2f);
        }

        public static void AddFrontliner()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Frontliner",
                "At the end of every turn, [creature]'s Valor Rank increases by 1.",
                typeof(AbilFrontliner),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_frontliner.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_frontliner.png", typeof(StressPlugin).Assembly));
            AbilFrontliner.ability = info.ability;
        }
    }
}
