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
    public class AbilEldritchPower : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep == !Card.OpponentCard;

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            yield return PreSuccessfulTriggerSequence();
            yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName}'s eldritch presence beckons towards you.", TextBox.Style.Neutral);
            yield return new WaitForSeconds(0.2f);
            try
            {
                StressPlugin.disAlchemyCounter.AddDies();
            }
            catch { }
            yield return new WaitForSeconds(0.3f);
            yield return LearnAbility(0.2f);
        }

        public static void AddEldritchPower()
        {

            AbilityInfo info = AbilityManager.New("StressSigils",
                "Eldritch Power",
                "At the beginning of each turn, [creature] provides it's owner with 1 Alchemy Die.",
                typeof(AbilEldritchPower),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_eldritchpower.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_eldritchpower.png", typeof(StressPlugin).Assembly));
            AbilEldritchPower.ability = info.ability;
        }
    }
}
