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
    public class AbilSigilEater : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer) => killer == Card;

        public override IEnumerator OnOtherCardDie(PlayableCard card, CardSlot deathSlot, bool fromCombat, PlayableCard killer)
        {
            yield return base.PreSuccessfulTriggerSequence();
            yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName} absorbs the powers of {deathSlot.Card.Info.displayedName}.", TextBox.Style.Neutral);
            yield return new WaitForSeconds(0.2f);

            CardModificationInfo newMod = new CardModificationInfo();
            List<Ability> abils = deathSlot.Card.Info.Abilities;
            abils.AddRange(deathSlot.Card.GetAbilitiesFromAllMods());
            newMod.abilities.AddRange(abils);
            if(newMod.abilities.Count > 0 && newMod.abilities.Contains(AbilSigilEater.ability)) newMod.abilities.Remove(AbilSigilEater.ability);

            newMod.singletonId = "SigilEater_absorbed";
            Card.AddTemporaryMod(newMod);

            Card.Anim.StrongNegationEffect();
            yield return LearnAbility(0.2f);
        }

        public static void AddSigilEater()
        {

            AbilityInfo info = AbilityManager.New("StressSigils",
                "Sigil Eater",
                "When [creature] strikes an opposing creature and it perishes, replace all sigils except this one with those of the slain.",
                typeof(AbilSigilEater),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_sigileater.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_sigileater.png", typeof(StressPlugin).Assembly));
            AbilSigilEater.ability = info.ability;
        }
    }
}
