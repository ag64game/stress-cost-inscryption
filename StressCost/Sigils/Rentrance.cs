using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StressCost.Sigils
{
    public class AbilRentrance : ActivatedAbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override int EnergyCost => 4;

        private CardSlot selected;

        public override System.Collections.IEnumerator Activate()
        {
            yield return PreSuccessfulTriggerSequence();

            var board = Singleton<BoardManager>.Instance;

            List<CardSlot> all = board.AllSlots.FindAll(slot => !slot.Equals(Card.Slot) && slot.Card != null);

            yield return board.ChooseTarget(all, all, EntranceSuccess, EntranceFailed, CursorEnteredSlot, () => false, CursorType.Target);

            try { yield return TimeTravel(selected); } finally { }

            yield return LearnAbility(0.2f);
            yield break;
        }

        private void EntranceSuccess(CardSlot slot)
        {
            selected = slot;
        }

        private void EntranceFailed(CardSlot slot)
        {
            Card.Anim.StrongNegationEffect();
            slot.PlaySound();
        }

        private void CursorEnteredSlot(CardSlot slot)
        {

        }

        private IEnumerator TimeTravel(CardSlot slot)
        {
            string traveller = slot.Card.Info.name;
            yield return slot.Card.Die(false);

            Singleton<TextBox>.Instance.ShowUntilInput($"{slot.Card.Info.displayedName} underwent Time Travel!", (GBC.TextBox.Style)Card.Info.temple);
            
            yield return slot.CreateCardInSlot(CardLoader.GetCardByName(traveller));
            yield return true;
        }

        public static void AddRentrance()
        {
            const string rulebookDescription = "Pay 4 Energy then choose a creature on the board. It will perish and be replayed.";

            AbilityInfo info = AbilityManager.New("StressSigils",
                "Rentrance",
                rulebookDescription,
                typeof(AbilRentrance),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_rentrance.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_rentrance.png", typeof(CostmaniaPlugin).Assembly));
            info.SetActivated(true);
            AbilRentrance.ability = info.ability;
        }
    }
}
