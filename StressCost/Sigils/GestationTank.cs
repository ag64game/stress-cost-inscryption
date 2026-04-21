using DiskCardGame;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GBC;
using InscryptionAPI.Helpers.Extensions;
namespace StressCost.Sigils
{
    public class AbilGestationTank : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;
        private int turnsPassed = 1;
        private string curIceCube = null;


        public override bool RespondsToUpkeep(bool playerUpkeep)
        {
            return playerUpkeep;
        }
        public override bool RespondsToDie(bool wasSacrifice, PlayableCard killer)
        {
            return !wasSacrifice;
        }


        public override IEnumerator OnUpkeep(bool playerUpkeep)
        {
            turnsPassed++;
            string iceCubeName = Card.Info.GetExtendedProperty($"GestationTankName{turnsPassed}");
            if (iceCubeName != null) curIceCube = iceCubeName;

            Card.Anim.StrongNegationEffect();
            return base.OnUpkeep(playerUpkeep);
        }

        public override IEnumerator OnDie(bool wasSacrifice, PlayableCard killer)
        {
            PreSuccessfulTriggerSequence();
            yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName} releases the monster inside!", (GBC.TextBox.Style)Card.Info.temple);
            if (curIceCube != null) yield return Card.Slot.CreateCardInSlot(CardLoader.GetCardByName(curIceCube));
            else
            {
                string firstCube = Card.Info.GetExtendedProperty($"GestationTankName1");
                if (firstCube != null ) yield return Card.Slot.CreateCardInSlot(CardLoader.GetCardByName(firstCube));
                else yield return Card.Slot.CreateCardInSlot(CardLoader.GetCardByName("Alchemy_Spite"));
            }

        }

        public static void AddGestationTank()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Gestation Tank",
                "When [creature] perishes, it's ever growing spawn is released in it's place.",
                typeof(AbilGestationTank),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_gestationtank.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_gestationtank.png", typeof(CostmaniaPlugin).Assembly));
            info.SetPowerlevel(4);
            AbilGestationTank.ability = info.ability;
        }
    }
}
