using DiskCardGame;
using EasyFeedback.APIs;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using InscryptionAPI.Helpers.Extensions;


namespace StressCost.Sigils
{
    public class AbilPigify : CostmaniaActivatedAbility
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override int ValorRankCost
        {
            get
            {
                return 2; 
            }
        }

        private List<CardSlot> available = new List<CardSlot>();
        private CardSlot selected;
        public override bool CanActivate()
        {
            Debug.Log(BoardManager.Instance.AllSlots.Where(slot => slot.Card != null && !slot.Card.Info.HasTrait(Trait.Terrain) && !slot.Equals(Card.Slot)).ToList().Count);
            return BoardManager.Instance.AllSlots.Any(slot => slot.Card != null && !slot.Card.Info.HasTrait(Trait.Terrain) && !slot.Equals(Card.Slot)) && base.CanActivate();
        }

        public override System.Collections.IEnumerator Activate()
        {
            yield return PreSuccessfulTriggerSequence();
            Singleton<ViewManager>.Instance.Controller.SwitchToControlMode(Singleton<BoardManager>.Instance.ChoosingSlotViewMode, false);
            Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Locked;

            available = BoardManager.Instance.AllSlots.Where(slot => slot.Card != null && !slot.Card.Info.HasTrait(Trait.Terrain) && !slot.Equals(Card.Slot)).ToList();
            yield return BoardManager.Instance.ChooseTarget(BoardManager.Instance.AllSlots, available, ChosenTarget, ChooseFail, CursorEnteredSlot, () => selected == Card.slot, CursorType.Target);

            yield return Singleton<TextBox>.Instance.ShowUntilInput($"{selected.Card.Info.displayedName} underwent Pigification!", (GBC.TextBox.Style)Card.Info.temple);
            base.Card.Anim.PlayAttackAnimation(false, selected);
            
            if(selected != null)
            {
                yield return selected.Card.Die(false);
                yield return new WaitForSeconds(0.175f);
                yield return selected.CreateCardInSlot(CardLoader.GetCardByName("Valor_Pig"));
                yield return new WaitForSeconds(0.375f);
            }

            Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;
            Singleton<ViewManager>.Instance.Controller.SwitchToControlMode(Singleton<BoardManager>.Instance.DefaultViewMode, false);
            yield return base.LearnAbility(0f);

            yield break;
        }

        private void ChosenTarget(CardSlot slot)
        {
            if (!slot.Equals(Card.slot)) selected = slot;
        }

        private void ChooseFail(CardSlot slot)
        {
            AudioController.Instance.PlaySound2D("toneless_negate", volume: 0.65f);
        }

        private void CursorEnteredSlot(CardSlot slot)
        {
            if (slot.Card != null) slot.Card.Anim.StrongNegationEffect();
        }

        public static void AddPigify()
        {
            const string rulebookDescription = "Expend 2 Valor Rank from [creature] to turn a chosen sacrificable creature into a Pig. A Pig is defined as: 0, 2, Sprinter";

            AbilityInfo info = AbilityManager.New("StressSigils",
                "Pigify",
                rulebookDescription,
                typeof(AbilPigify),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_pigify.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_pigify.png", typeof(CostmaniaPlugin).Assembly));
            info.SetActivated(true);
            info.SetPowerlevel(4);
            AbilPigify.ability = info.ability;
        }
    }
}
