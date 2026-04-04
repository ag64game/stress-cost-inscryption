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
    public class AbilRandomAbility : RandomAbility
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override bool RespondsToDrawn() => Card.Status.hiddenAbilities.Contains(Ability);

        public override IEnumerator OnDrawn()
        {
            yield return base.OnDrawn();
            Card.Status.hiddenAbilities.Add(Ability);
        }

        public static void AddRandomAbility()
        {
            AbilityInfo info = AbilityManager.New("StressSigils",
                "Random Ability",
                "When [creature] is drawn, this sigil is changed to a new one at random.",
                typeof(AbilRandomAbility),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_randomability.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_randomability.png", typeof(StressPlugin).Assembly));
            AbilIronclad.ability = info.ability;
        }
    }
}
