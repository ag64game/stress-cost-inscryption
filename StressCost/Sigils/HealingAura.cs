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
    public class AbilHealingAura : AbilityBehaviour
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToTurnEnd(bool playerTurnEnd) => playerTurnEnd;

        public override IEnumerator OnTurnEnd(bool playerTurnEnd)
        {
            yield return PreSuccessfulTriggerSequence();
            Card.Anim.StrongNegationEffect();

            List<PlayableCard> cards = new List<PlayableCard>();
            PlayableCard left = null;
            PlayableCard right = null;
            try { left = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, true).Card; } catch { Console.WriteLine("Left failed"); }
            try { right = Singleton<BoardManager>.Instance.GetAdjacent(base.Card.Slot, false).Card; } catch { Console.WriteLine("Right failed"); }

            if (left != null) cards.Add(left);
            if (right != null) cards.Add(right);

            foreach (PlayableCard card in cards) if (card.Health < card.MaxHealth + 1) card.HealDamage(1);

            yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName} tended to the wounds of it's allies.", TextBox.Style.Neutral);

            yield return LearnAbility(0.2f);
        }

        public static void AddHealingAura()
        {
            const string rulebookDescription = "At the end of it's owner's turn, [creature] heals it's 2 adjascent allies by 1 Health. [creature] can only heal up to 1 plus a creature's Maximum Health.";

            AbilityInfo info = AbilityManager.New("StressSigils",
                "Healing Aura",
                rulebookDescription,
                typeof(AbilHealingAura),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_healingaura.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_healingaura.png", typeof(StressPlugin).Assembly));
            AbilHealingAura.ability = info.ability;
        }
    }
}
