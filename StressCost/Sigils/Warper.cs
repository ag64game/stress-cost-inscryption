using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace StressCost.Sigils
{
    public class AbilWarper : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        private CardSlot selected;

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => playerTurnEnd;

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            yield return PreSuccessfulTriggerSequence();

            var board = Singleton<BoardManager>.Instance;

            List<CardSlot> all = board.playerSlots;

            if (Card.OpponentCard)
            {
                if (board.playerSlots.Where(slot => slot.Card == null).Count() == 0) selected = board.opponentSlots[UnityEngine.Random.Range(0, board.opponentSlots.Count)];
                else
                {
                    var available = board.playerSlots.Where(slot => slot.Card == null).ToList();
                    selected = available[UnityEngine.Random.Range(0, available.Count)].opposingSlot;
                }

            } else
            {
                yield return board.ChooseTarget(all, all, MovementSuccess, MovementFailed, CursorEnteredSlot, () => false, CursorType.Target);
            }

            yield return DoMovement(selected);

            yield return LearnAbility(0.2f);
        }

        private void MovementSuccess(CardSlot slot)
        {
            selected = slot;
        }

        private IEnumerator DoMovement(CardSlot slot)
        {
            if (slot.Card != null)
            {
                PlayableCard switched = slot.Card;
                if(!slot.Equals(Card.Slot)) yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName} switched places with {switched.Info.displayedName}.", (GBC.TextBox.Style)Card.Info.temple);
                yield return Singleton<BoardManager>.Instance.AssignCardToSlot(switched, base.Card.Slot, 0.1f, null, true);
            }
            yield return Singleton<BoardManager>.Instance.AssignCardToSlot(base.Card, slot, 0.1f, null, true);
        }

        private void MovementFailed(CardSlot slot)
        {
             Card.Anim.StrongNegationEffect();
             slot.PlaySound();
        }

        private void CursorEnteredSlot(CardSlot slot)
        {

        }

        public static void AddWarper()
        {
            const string rulebookDescription = "At the end of it's owner's turn, [creature] moves to a slot chosen by it's owner. Should a creature reside in that slot, the two will switch places.";

            AbilityInfo info = AbilityManager.New("StressSigils",
                "Warper",
                rulebookDescription,
                typeof(AbilWarper),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_warper.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_warper.png", typeof(CostmaniaPlugin).Assembly));
            AbilWarper.ability = info.ability;
        }
    }
}
