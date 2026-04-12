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
    public class AbilBloodGuzzler : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToDealDamage(int amount, PlayableCard target)
        {
            return base.Card.NotDead() && amount > 0;
        }

        public override IEnumerator OnDealDamage(int amount, PlayableCard target)
        {
            yield return PreSuccessfulTriggerSequence();
            Card.Anim.StrongNegationEffect();
            yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName} slakes on the blood of {target.Info.DisplayedNameEnglish}", (GBC.TextBox.Style)Card.Info.temple);
            yield return new WaitForSeconds(0.25f);

            base.Card.AddTemporaryMod(new CardModificationInfo(0, amount));
            Card.OnStatsChanged();
            
            base.Card.RenderCard();
            yield return new WaitForSeconds(0.25f);
            yield return LearnAbility(0.25f);
        }

        public static void AddBloodGuzzler()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Blood Guzzler",
                "When [creature] deals damage, it gains 1 Health for each damage dealt.",
                typeof(AbilBloodGuzzler),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_bloodguzzler.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_bloodguzzler.png", typeof(CostmaniaPlugin).Assembly));
            AbilBloodGuzzler.ability = info.ability;
        }
    }
}
