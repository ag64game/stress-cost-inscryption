using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace StressCost.Sigils
{
    public class AbilFirstStrike : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            yield return PreSuccessfulTriggerSequence();
            yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName} took a free shot!", TextBox.Style.Neutral);

            CardSlot opponent = base.Card.Slot.opposingSlot;

            base.Card.Anim.PlayAttackAnimation(false, opponent);
            yield return new WaitForSeconds(0.175f);

            if (opponent.Card != null && !opponent.Card.FaceDown) yield return opponent.Card.TakeDamage(base.Card.Attack, base.Card);
            else
            {
                yield return new WaitForSeconds(0.175f);
                yield return Singleton<LifeManager>.Instance.ShowDamageSequence(base.Card.Attack, base.Card.Attack, base.Card.OpponentCard, 0.3f, null, 0.15f, true);
            }
            yield return new WaitForSeconds(0.3f);
            yield return LearnAbility(0.2f);
        }

        public static void AddFirstStrike()
        {
            const string rulebookDescription = "[creature] attacks the moment it is placed.";

            AbilityInfo info = AbilityManager.New("StressSigils",
                "First Strike",
                rulebookDescription,
                typeof(AbilFirstStrike),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_firststrike.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_firststrike.png", typeof(StressPlugin).Assembly));
            AbilFirstStrike.ability = info.ability;
        }
    }
}
