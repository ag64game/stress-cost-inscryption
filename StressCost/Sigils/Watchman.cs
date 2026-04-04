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
    public class AbilWatchman : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        private bool textDisplayed = false;

        public override bool RespondsToOtherCardResolve(PlayableCard otherCard)
        {
            return otherCard.OpponentCard != base.Card.OpponentCard;
        }
        public override bool RespondsToResolveOnBoard()
        {
            return true;
        }

        public override IEnumerator OnResolveOnBoard()
        {
            textDisplayed = false;
            return base.OnResolveOnBoard();
        }

        public override IEnumerator OnOtherCardResolve(PlayableCard otherCard)
        {
            yield return base.PreSuccessfulTriggerSequence();

            if (!textDisplayed)
            {
                textDisplayed = true;
                yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName} spied on incoming {otherCard.Info.displayedName}!", TextBox.Style.Neutral);
            }

            base.Card.Anim.StrongNegationEffect();

            yield return Singleton<CardDrawPiles>.Instance.DrawCardFromDeck(null, null);
            yield return base.LearnAbility(0.1f);
        }

        public static void AddWatchman()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Watchman",
                "While [creature] is on the board, it's owner draws a card every time an opponent enters play.",
                typeof(AbilWatchman),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_watchman.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_watchman.png", typeof(StressPlugin).Assembly));
            AbilWatchman.ability = info.ability;
        }
    }
}
