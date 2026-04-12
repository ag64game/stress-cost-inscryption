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
    public class AbilAfterimage : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        private bool doDodge = true;

        public override bool RespondsToUpkeep(bool playerUpkeep) => playerUpkeep == !Card.OpponentCard;

        public override bool RespondsToSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
        {
            return slot.Equals(Card.Slot) && doDodge && 
                (Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, true).Card != null || Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, false).Card != null);
        }

        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            doDodge = true;
            yield return base.OnUpkeep(playerUpkeep);
        }

        public override IEnumerator OnSlotTargetedForAttack(CardSlot slot, PlayableCard attacker)
        {
            doDodge = false;
            yield return base.PreSuccessfulTriggerSequence();
            yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName} responds with a vicious counter attack!", (GBC.TextBox.Style)Card.Info.temple);

            base.Card.Anim.PlayAttackAnimation(attacker.OpponentCard, attacker.Slot);
            yield return new WaitForSeconds(0.175f);
            yield return attacker.TakeDamage(1, Card);
            yield return new WaitForSeconds(0.175f);

            CardSlot right = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, false);
            if (right.Card != null) yield return DoMovement(right, false); else yield return DoMovement(Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, true), true);


           yield return base.LearnAbility(0.1f);
        }

        private IEnumerator DoMovement(CardSlot destination, bool isLeft)
        {
            base.Card.RenderInfo.flippedPortrait = (isLeft && base.Card.Info.flipPortraitForStrafe);
            base.Card.RenderCard();

            CardSlot oldSlot = base.Card.Slot;
            yield return Singleton<BoardManager>.Instance.AssignCardToSlot(base.Card, destination, 0.1f, null, true);

            yield return new WaitForSeconds(0.25f);
        }

        public static void AddAfterimage()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Afterimage",
                "Once per round, [creature] may dodge an incoming attack, dealing 1 damage to the attacker.",
                typeof(AbilAfterimage),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_afterimage.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_afterimage.png", typeof(CostmaniaPlugin).Assembly));
            AbilAfterimage.ability = info.ability;
        }
    }
}
