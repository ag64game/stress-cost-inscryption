using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GBC;
using InscryptionAPI.Encounters;
using System.Linq;
using InscryptionAPI.Helpers.Extensions;
namespace StressCost.Sigils
{
    public class AbilWatchman : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        private bool textDisplayed = false;
        public static int spiedCountPlayer = 0, spiedCountEnemy = 0;

        public override bool RespondsToOtherCardResolve(PlayableCard otherCard)
        {
            return otherCard.OpponentCard != base.Card.OpponentCard;
        }
        public override bool RespondsToUpkeep(bool playerUpkeep)
        {
            return true;
        }

        public override IEnumerator OnOtherCardResolve(PlayableCard otherCard)
        {
            yield return base.PreSuccessfulTriggerSequence();

            if (!textDisplayed)
            {
                textDisplayed = true;
                yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName} spied on incoming {otherCard.Info.displayedName}!", (GBC.TextBox.Style)Card.Info.temple);
            }

            base.Card.Anim.StrongNegationEffect();

            if (Card.OpponentCard) spiedCountEnemy++; else spiedCountPlayer++;
            yield return base.LearnAbility(0.1f);
        }

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            textDisplayed = false;

            return base.OnUpkeep(playerUpkeep);
        }

        public static void AddWatchman()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Watchman",
                "While [creature] is on the board, it's owner draws a card every time an opponent enters play.",
                typeof(AbilWatchman),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_watchman.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_watchman.png", typeof(CostmaniaPlugin).Assembly));
            AbilWatchman.ability = info.ability;
        }
    }
}
