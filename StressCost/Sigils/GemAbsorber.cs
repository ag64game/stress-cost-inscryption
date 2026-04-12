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
    public class AbilGemAbsorber : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            yield return PreSuccessfulTriggerSequence();
            List<CardSlot> slots = new List<CardSlot>();
            if (base.Card.OpponentCard) slots = Singleton<BoardManager>.Instance.opponentSlots; else slots = Singleton<BoardManager>.Instance.playerSlots;

            slots = slots.FindAll(slot => slot.Card != null && slot.Card.Info.HasTrait(Trait.Gem));

            Console.WriteLine(slots.Count);

            

            if (slots.Count > 0)
            {
                yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName} absorbs the mana of the crystals!", (GBC.TextBox.Style)Card.Info.temple);

                foreach (CardSlot gemSlot in slots)
                {
                    base.Card.Anim.PlayAttackAnimation(gemSlot.Card.OpponentCard, gemSlot);
                    yield return new WaitForSeconds(0.175f);

                    gemSlot.Card.AddTemporaryMod(new CardModificationInfo(0, -999));
                    yield return gemSlot.Card.Die(false);

                    
                    yield return new WaitForSeconds(0.015f);
                    base.Card.Anim.StrongNegationEffect();
                    base.Card.AddTemporaryMod(new CardModificationInfo(1, 1));

                    yield return new WaitForSeconds(0.165f);
                }

                yield return LearnAbility(0.2f);
                yield return true;
            }
            else
            {
                base.Card.Anim.StrongNegationEffect();
                yield return false;
            }            
        }

        public static void AddGemAbsorber()
        {
            const string rulebookDescription = "When [creature] is placed on the board. All Mox cards you control perish, each granting it +1 Power and Health.";

            AbilityInfo info = AbilityManager.New("StressSigils",
                "Gem Absorber",
                rulebookDescription,
                typeof(AbilGemAbsorber),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_gemabsorber.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_gemabsorber.png", typeof(CostmaniaPlugin).Assembly));
            AbilGemAbsorber.ability = info.ability;
        }
    }
}
