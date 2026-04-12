using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace StressCost.Sigils
{
    public class AbilArmyBuilder : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToResolveOnBoard() => true;

        public override IEnumerator OnResolveOnBoard()
        {
            yield return PreSuccessfulTriggerSequence();
            CardSlot[] slots = { Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, true), Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, false) };

            if (slots.Where(slot => slot.Card == null).Count() > 0)
            {
                yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName} called upon the strength of it's soliders!", (GBC.TextBox.Style)Card.Info.temple);

                string troopName = "Squirrel";
                string modPrefix = Card.Info.GetModPrefix();
                if (Card.Info.GetExtendedProperty("TroopName") != null) troopName = Card.Info.GetExtendedProperty("TroopName");
                else
                {
                    if (modPrefix == null || !CostmaniaPlugin.NEW_TEMPLES.Any(newTemple => modPrefix.Contains(newTemple)))
                    {
                        switch (Card.Info.temple)
                        {
                            case (CardTemple.Nature):
                                troopName = "Bee";
                                break;
                            case (CardTemple.Undead):
                                troopName = "Zombie";
                                break;
                            case (CardTemple.Tech):
                                troopName = "Automaton";
                                break;
                            case (CardTemple.Wizard):
                                troopName = "FlyingMage";
                                break;
                        }
                    }
                    else
                    {
                        if (modPrefix.Contains("Alchemy")) troopName = "Alchemy_Homonculus";
                        else if (modPrefix.Contains("Stress")) troopName = "Stress_Micro";
                        else if (modPrefix.Contains("Space")) troopName = "Space_ShootingStar";
                        else if (modPrefix.Contains("Valor")) troopName = "Valor_InfantryKnight";
                    }
                }

                foreach (CardSlot slot in slots)
                {
                    if (slot.Card == null)
                    {
                        Card.Anim.PlayAttackAnimation(false, slot);
                        yield return slot.CreateCardInSlot(CardLoader.GetCardByName(troopName));
                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }
            else Card.Anim.StrongNegationEffect();
            
            yield return LearnAbility(0.2f);
        }

        public static void AddArmyBuilder()
        {
            const string rulebookDescription = "When [creature] is played, it's troops are placed to it's left and right.";

            AbilityInfo info = AbilityManager.New("StressSigils",
                "Army Builder",
                rulebookDescription,
                typeof(AbilArmyBuilder),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_armybuilder.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_armybuilder.png", typeof(CostmaniaPlugin).Assembly));
            AbilArmyBuilder.ability = info.ability;
        }
    }
}
