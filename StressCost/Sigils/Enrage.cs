using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace StressCost.Sigils
{
    public class AbilEnrage : StressActivatedAbility
    {
        public static Ability ability;
        public override Ability Ability => ability;

        public override int StressCost
        {
            get
            {
                return 2; 
            }
        }

        public override System.Collections.IEnumerator Activate()
        {
            yield return Singleton<TextBox>.Instance.ShowUntilInput($"{Card.Info.displayedName}'s rage crept further", TextBox.Style.Neutral);

            this.Card.temporaryMods.Add(new CardModificationInfo(1, 1));

            this.Card.Anim.StrongNegationEffect();
             yield return base.LearnAbility(0f);

            yield break;
        }

        public static void AddEnrage()
        {
            const string rulebookDescription = "Pay 2 stress for [creature] to gain 1 Power and Health.";

            AbilityInfo info = AbilityManager.New("StressSigils",
                "Enrage",
                rulebookDescription,
                typeof(AbilEnrage),
                "StressCards/StressCost/StressCost/Resources/Sigils/3d_enrage.png");

            info.SetPixelAbilityIcon(TextureHelper.GetImageAsTexture($"pixel_enrage.png", typeof(StressPlugin).Assembly));
            info.SetActivated(true);
            AbilEnrage.ability = info.ability;
        }
    }
}
