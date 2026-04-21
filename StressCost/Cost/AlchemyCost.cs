using DiskCardGame;
using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.CardCosts;
using InscryptionAPI.Helpers;
using InscryptionCommunityPatch.Card;
using JetBrains.Annotations;
using Newtonsoft.Json.Bson;
using Pixelplacement.TweenSystem;
using Sirenix.Serialization.Utilities;
using StressCost.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static InscryptionAPI.CardCosts.CardCostManager;
using Press = UnityEngine.UI.Button;

namespace StressCost.Cost
{
    public enum AlchemyValue
    {
        Empty,
        Flesh,
        Metal,
        Elixir,
    }

    internal class AlchemyDice : MonoBehaviour
    {
        private AlchemyValue __value = AlchemyValue.Empty;
        public AlchemyValue value { get; set; }

        public bool locked = false;
        private SpriteRenderer renderer;
        public static int texWidth = 0, texHeight = 0;

        public Vector3 pivot = new Vector3(0.38f, 0.35f, 0f);

        private static Sprite fleshSprite = null, metalSprite = null, elixirSprite = null, emptySprite = null;

        

        private void Awake()
        {
            renderer = gameObject.GetComponent<SpriteRenderer>();

            Texture2D texture = TextureHelper.GetImageAsTexture($"displaycost_flesh_alchemy_die.png", typeof(CostmaniaPlugin).Assembly);
            fleshSprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texWidth, texHeight),
                    pivot
                    );
            fleshSprite.name = "flesh_sprite";

            metalSprite = Sprite.Create(
                    TextureHelper.GetImageAsTexture($"displaycost_metal_alchemy_die.png", typeof(CostmaniaPlugin).Assembly),
                    new Rect(0, 0, texWidth, texHeight),
                    pivot
                    );
            metalSprite.name = "metal_sprite";

            elixirSprite = Sprite.Create(
                    TextureHelper.GetImageAsTexture($"displaycost_elixir_alchemy_die.png", typeof(CostmaniaPlugin).Assembly),
                    new Rect(0, 0, texWidth, texHeight),
                    pivot
                    );
            elixirSprite.name = "elixir_sprite";

            emptySprite = Sprite.Create(
                    TextureHelper.GetImageAsTexture($"displaycost_empty_alchemy_die.png", typeof(CostmaniaPlugin).Assembly),
                    new Rect(0, 0, texWidth, texHeight),
                    pivot
                    );
            emptySprite.name = "empty_sprite";

            texWidth = texture.width;
            texHeight = texture.height;
        }

        private void Update()
        {
            switch (value)
            {
                case (AlchemyValue.Flesh):
                    renderer.sprite = fleshSprite;
                    break;
                case (AlchemyValue.Metal):
                    renderer.sprite = metalSprite;
                    break;
                case (AlchemyValue.Elixir):
                    renderer.sprite = elixirSprite;
                    break;
                default:
                    renderer.sprite = emptySprite;
                    locked = false;
                    break;
            }

            float lockedColor = 0.5f * (locked ? 1 : 2);
            renderer.color = new Color(lockedColor, lockedColor, lockedColor);
        }

        public void OnMouseDown()
        {
            if (value != AlchemyValue.Empty)
            {
                if (!locked) AlchemyCounter.GetLastClickedDiceId(this);
                AudioController.Instance.PlaySound2D("crunch_short#2", volume:0.25f);
                locked = !locked;
            }
        }
    }

    public class AlchemyCounter : MonoBehaviour
    {
        private static AlchemyDice[] dies = new AlchemyDice[10];

        private static AlchemyValue[] secondPlayer = new AlchemyValue[10];
        private static bool[] secondPlayerLock = new bool[10];
        public static int lastClicked = 0;
        private static bool dieClicked = false;
        private static bool waitingForClick = false;

        public static int fleshCount
        {
            get
            {
                return dies.Where(die => die.value == AlchemyValue.Flesh).Count();
            }
        }
        public static int metalCount
        {
            get
            {
                return dies.Where(die => die.value == AlchemyValue.Metal).Count();
            }
        }
        public static int elixirCount
        {
            get
            {
                return dies.Where(die => die.value == AlchemyValue.Elixir).Count();
            }
        }
        public static int awakeCount
        {
            get
            {
                return dies.Where(die => die.value != AlchemyValue.Empty).Count();
            }
        }

        public static int awakeCountNotLocked
        {
            get
            {
                return dies.Where(die => die.value != AlchemyValue.Empty && !die.locked).Count();
            }
        }

        private float size = 0.12f;

        private void Awake()
        {
            float x = -2.125f, y = 0.51f;
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("julianperge.inscryption.act2.increaseCardSlots"))
            {
                x = -2.09125f;
                y = 1.09f;
            }

            for (int i = 0; i < dies.Length; i++)
            {
                var position  = new Vector3(x, y, 0.1f);
                GameObject dieObj = Instantiate(PixelResourcesManager.Instance.gameObject.transform.Find("Blood").Find("Blood").gameObject);
                dieObj.name = $"AlchemyDice {i + 1}";

                BoxCollider2D col = dieObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(size, size);
                dieObj.transform.position = position;

                dies[i] = dieObj.AddComponent<AlchemyDice>();
                dies[i].gameObject.transform.SetParent(transform, false);


                if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("julianperge.inscryption.act2.increaseCardSlots"))
                {
                    if ((i + 1) % 2 == 0)
                    {
                        x -= size;
                        y -= size;
                    }
                    else x += size;
                } else
                {
                    if ((i + 1) % 2 == 0)
                    {
                        x += size;
                        y -= size;
                    }
                    else y += size;
                }

                ResetPlayerTwo();
            }
        }

        public static void AddDies(int amount = 1, AlchemyValue? specific = null)
        {
            try
            {
                for (int i = Array.IndexOf(dies, dies.Where(die => die.value == AlchemyValue.Empty).First()); amount > 0 && i < 10; i++)
                {
                    RollDice(i, specific);
                    amount--;
                }
            }
            catch { }
        }

        public static void RollDies(int? index = null, AlchemyValue? specific = null)
        {
            if (index != null) RollDice(index.Value, specific);
            else for(int i = 0; i < awakeCount; i++) RollDice(i, specific);
            AudioController.Instance.PlaySound2D("crunch_short#1", volume: 0.65f);
        }

        private static void RollDice(int index, AlchemyValue? specific = null)
        {
            if (!dies[index].locked)
            {
                if (specific != null) dies[index].value = specific.Value;
                else
                {
                    dies[index].value = (AlchemyValue)UnityEngine.Random.Range(1, 4);
                }
            }
        }

        public static bool PayIfPossible(AlchemyValue type, int amount)
        {
            switch (type)
            {
                case AlchemyValue.Flesh:
                    if (fleshCount < amount) return false;
                    break;
                case AlchemyValue.Metal:
                    if (metalCount < amount) return false;
                    break;
                case AlchemyValue.Elixir:
                    if (elixirCount < amount) return false;
                    break;
                default:
                    throw new ArgumentException("Cannot pay with 'Empty' Alchemy");
            }

            DepleteDies(type, amount);
            AudioController.Instance.PlaySound2D("crunch_short#1", volume: 0.4f);
            return true;
        }

        private static void DepleteDies(AlchemyValue toDrop, int amount)
        {
            for (int i = 0; i < 10 && amount > 0 && dies[i].value != AlchemyValue.Empty; i++)
            {  
                if (dies[i].value == toDrop && amount > 0)
                {
                    dies[i].value = AlchemyValue.Empty;
                    for (int j = i; j < 9 && dies[j + 1].value != AlchemyValue.Empty; j++)
                    {
                        dies[j].value = dies[j + 1].value;
                        dies[j].locked = dies[j + 1].locked;
                        dies[j + 1].value = AlchemyValue.Empty;
                    }
                    amount--;
                    i--;
                }
                
            }
        }

        public static IEnumerator WaitUntilClick(IEnumerator next)
        {
            waitingForClick = true;
            InteractionCursor.Instance.ForceCursorType(CursorType.Target);
            yield return new WaitUntil(() => dieClicked);


            yield return dies[lastClicked].locked = !dies[lastClicked].locked;
            yield return next;

            yield return dieClicked = false;
            yield return waitingForClick = false;
        }

        internal static bool GetLastClickedDiceId(object die)
        {
            if (die is not AlchemyDice) throw new UnauthorizedAccessException("Only counter's Alchemy Dies can call this method.");
            else
            {
                for(int i = 0; i < dies.Length; i++)
                {
                    if (dies[i] == die)
                    {
                        lastClicked = i;
                        if (waitingForClick)
                        {
                            InteractionCursor.Instance.ForceCursorType(CursorType.Default);
                            dieClicked = true;
                        }
                        return true;
                    }
                }

                throw new UnauthorizedAccessException("Only counter's Alchemy Dies can call this method.");
            }
        }

        public void SwitchPlayer()
        {
            AlchemyValue[] curPlayer = dies.Select(die => die.value).ToArray();
            bool[] curPlayerLock = dies.Select(die => die.locked).ToArray();

            for(int i = 0; i < dies.Length; i++)
            {
                dies[i].value = secondPlayer[i];
                dies[i].locked = secondPlayerLock[i];
            }

            secondPlayer = curPlayer;
            secondPlayerLock = curPlayerLock;
        }
        public void ResetPlayerTwo()
        {
            for (int i = 0; i < secondPlayer.Length; i++) secondPlayer[i] = AlchemyValue.Empty;
            secondPlayer[0] = (AlchemyValue)UnityEngine.Random.Range((int)AlchemyValue.Flesh, (int)AlchemyValue.Elixir + 1);
            secondPlayer[1] = (AlchemyValue)UnityEngine.Random.Range((int)AlchemyValue.Flesh, (int)AlchemyValue.Elixir + 1);
        }
    }



    internal class FleshCost : CustomCardCost
    {
        public override string CostName => "FleshCost";

        public override bool CostSatisfied(int cardCost, PlayableCard card)
        {
            return cardCost <= AlchemyCounter.fleshCount;
        }

        public override string CostUnsatisfiedHint(int cardCost, PlayableCard card)
        {
            if (SaveManager.SaveFile.IsPart2) return $"You do not have the right amount of [c:green]Alchemy Dies[c:] to play this card. [c:green]Alchemy Dies[c:] are generated over time.";
            else
            {
                var choice1 = $"[c:green]{card.Info.DisplayedNameLocalized}[c:] can only be played once you've accumulated enough nessecary [c:green]dies[c:].";
                var choice2 = $"Your [c:green]Alchemy Dies[c:] are of insufficient quantities.";
                var choice3 = $"Don't be a fool, you need [c:green]{cardCost} Flesh Dies[c:] to play [c:green]{card.Info.DisplayedNameLocalized}[c:]";

                List<String> strings = new List<String>();
                strings.Add(choice1);
                strings.Add(choice2);
                strings.Add(choice3);

                return strings[UnityEngine.Random.Range(0, strings.Count)];
            }
        }

        public static Texture2D Texture_3D(int cardCost, CardInfo info, PlayableCard card)
        {
            return TextureHelper.GetImageAsTexture($"FleshCost_{cardCost}.png", typeof(CostmaniaPlugin).Assembly);
        }

        public static Texture2D Texture_Pixel(int cardCost, CardInfo info, PlayableCard card)
        {
            // if you want the API to handle adding stack numbers, you can instead provide a 7x8 texture like so:
            return Part2CardCostRender.CombineIconAndCount(cardCost, TextureHelper.GetImageAsTexture("pixelcost_flesh.png", typeof(CostmaniaPlugin).Assembly));
        }
    }
    internal class MetalCost : CustomCardCost
    {
        public override string CostName => "MetalCost";

        public override bool CostSatisfied(int cardCost, PlayableCard card)
        {
            return cardCost <= AlchemyCounter.metalCount;
        }

        public override string CostUnsatisfiedHint(int cardCost, PlayableCard card)
        {
            if (SaveManager.SaveFile.IsPart2) return $"You do not have the right amount of [c:green]Alchemy Dies[c:] to play this card. [c:green]Alchemy Dies[c:] are generated over time.";
            else
            {
                var choice1 = $"[c:green]{card.Info.DisplayedNameLocalized}[c:] can only be played once you've accumulated enough nessecary [c:green]dies[c:].";
                var choice2 = $"Your [c:green]Alchemy Dies[c:] are of insufficient quantities.";
                var choice3 = $"Don't be a fool, you need [c:green]{cardCost} Metal Dies[c:] to play [c:green]{card.Info.DisplayedNameLocalized}[c:]";

                List<String> strings = new List<String>();
                strings.Add(choice1);
                strings.Add(choice2);
                strings.Add(choice3);

                return strings[UnityEngine.Random.Range(0, strings.Count)];
            }
        }

        public static Texture2D Texture_3D(int cardCost, CardInfo info, PlayableCard card)
        {
            return TextureHelper.GetImageAsTexture($"MetalCost_{cardCost}.png", typeof(CostmaniaPlugin).Assembly);
        }

        public static Texture2D Texture_Pixel(int cardCost, CardInfo info, PlayableCard card)
        {
            // if you want the API to handle adding stack numbers, you can instead provide a 7x8 texture like so:
            return Part2CardCostRender.CombineIconAndCount(cardCost, TextureHelper.GetImageAsTexture("pixelcost_metal.png", typeof(CostmaniaPlugin).Assembly));
        }
    }
    internal class ElixirCost : CustomCardCost
    {
        public override string CostName => "ElixirCost";

        public override bool CostSatisfied(int cardCost, PlayableCard card)
        {
            return cardCost <= AlchemyCounter.elixirCount;
        }

        public override string CostUnsatisfiedHint(int cardCost, PlayableCard card)
        {
            if (SaveManager.SaveFile.IsPart2) return $"You do not have the right amount of [c:green]Alchemy Dies[c:] to play this card. [c:green]Alchemy Dies[c:] are generated over time.";
            else
            {
                var choice1 = $"[c:green]{card.Info.DisplayedNameLocalized}[c:] can only be played once you've accumulated enough nessecary [c:green]dies[c:].";
                var choice2 = $"Your [c:green]Alchemy Dies[c:] are of insufficient quantities.";
                var choice3 = $"Don't be a fool, you need [c:green]{cardCost} Elixir Dies[c:] to play [c:green]{card.Info.DisplayedNameLocalized}[c:]";

                List<String> strings = new List<String>();
                strings.Add(choice1);
                strings.Add(choice2);
                strings.Add(choice3);

                return strings[UnityEngine.Random.Range(0, strings.Count)];
            }
        }

        public static Texture2D Texture_3D(int cardCost, CardInfo info, PlayableCard card)
        {
            return TextureHelper.GetImageAsTexture($"ElixirCost_{cardCost}.png", typeof(CostmaniaPlugin).Assembly);
        }

        public static Texture2D Texture_Pixel(int cardCost, CardInfo info, PlayableCard card)
        {
            // if you want the API to handle adding stack numbers, you can instead provide a 7x8 texture like so:
            return Part2CardCostRender.CombineIconAndCount(cardCost, TextureHelper.GetImageAsTexture("pixelcost_elixir.png", typeof(CostmaniaPlugin).Assembly));
        }
    }

    public static class CardAlchemyExpansion
    {
        public static int FleshCost(this CardInfo card)
        {
            int? baseVal = card.GetExtendedPropertyAsInt("FleshCost");
            if (baseVal == null) baseVal = 0;

            return baseVal.Value;
        }

        public static int MetalCost(this CardInfo card)
        {
            int? baseVal = card.GetExtendedPropertyAsInt("MetalCost");
            if (baseVal == null) baseVal = 0;

            return baseVal.Value;
        }

        public static int ElixirCost(this CardInfo card)
        {
            int? baseVal = card.GetExtendedPropertyAsInt("ElixirCost");
            if (baseVal == null) baseVal = 0;

            return baseVal.Value;
        }
    }
}
