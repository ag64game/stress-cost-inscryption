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
    public class AbilLiftoff : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            yield return PreSuccessfulTriggerSequence();

            base.Card.Anim.PlayJumpAnimation();
            yield return new WaitForSeconds(0.2f);

            List<PlayableCard> cards = new List<PlayableCard>();
            PlayableCard left = null;
            PlayableCard right = null;
            try { left = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, true).Card; } catch { Console.WriteLine("Left failed"); }
            try { right = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, false).Card; } catch { Console.WriteLine("Right failed"); }

            if (left != null) cards.Add(left);
            if (right != null) cards.Add(right);

            foreach (PlayableCard card in cards)
            {
                CardModificationInfo mod = new CardModificationInfo(Ability.Flying);
                mod.singletonId = "Liftoff_buff";
                card.Status.hiddenAbilities.Add(Ability.Flying);
                card.AddTemporaryMod(mod);
            }


            yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName} granted their friends the power of flight!", (GBC.TextBox.Style)Card.Info.temple);

            yield return LearnAbility(0.2f);
        }

        public static void AddLiftoff()
        {
            const string rulebookDescription = "[creature] lifts adjascent allies to the air making them go Airborne.";

            AbilityInfo info = AbilityManager.New("StressSigils",
                "Liftoff",
                rulebookDescription,
                typeof(AbilLiftoff),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_liftoff.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_liftoff.png", typeof(CostmaniaPlugin).Assembly));
            AbilLiftoff.ability = info.ability;
        }
    }
}
